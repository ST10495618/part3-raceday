using Microsoft.AspNetCore.Mvc;
using RaceDayMVC.Models.ViewModels;
using System.Net.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Net.Http.Json;

namespace RaceDayMVC.Controllers
{
    public class OrganiserController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public OrganiserController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }
        public IActionResult Dashboard()
        {
            if (HttpContext.Session.GetString("JWToken") == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            if (HttpContext.Session.GetString("UserRole") != "Organiser")
            {
                return RedirectToAction("Login", "Auth");
            }

            ViewBag.FirstName =
                HttpContext.Session.GetString("FirstName");

            return View();
        }
        public async Task<IActionResult> ManageEvents()
        {
            // Check login token
            string? token = HttpContext.Session.GetString("JWToken");

            if (token == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            // Check organiser role
            if (HttpContext.Session.GetString("UserRole") != "Organiser")
            {
                return RedirectToAction("Login", "Auth");
            }

            // Get logged-in organiser ID
            int? organiserID = HttpContext.Session.GetInt32("UserID");

            if (organiserID == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            // Connect to API
            var client = _httpClientFactory.CreateClient("RaceDayAPI");

            client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue(
                    "Bearer", token);

            try
            {
                // Get events from API
                var response = await client.GetAsync("api/Events");

                if (!response.IsSuccessStatusCode)
                {
                    ViewBag.Error = "Unable to retrieve events from the API.";

                    return View(new List<EventViewModel>());
                }

                var json = await response.Content.ReadAsStringAsync();

                var events = System.Text.Json.JsonSerializer.Deserialize
                    <List<EventViewModel>>(
                        json,
                        new System.Text.Json.JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        });

                // Only display events belonging to this organiser
                var organiserEvents = events?
                    .Where(e => e.OrganiserID == organiserID.Value)
                    .OrderBy(e => e.EventDate)
                    .ToList() ?? new List<EventViewModel>();

                return View(organiserEvents);
            }
            catch (Exception)
            {
                ViewBag.Error = "An error occurred while retrieving events.";

                return View(new List<EventViewModel>());
            }
        }
        private async Task<List<CategoryViewModel>> GetCategories(string token)
        {
            var client = _httpClientFactory.CreateClient("RaceDayAPI");

            client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue(
                    "Bearer", token);

            var response = await client.GetAsync("api/Categories");

            if (!response.IsSuccessStatusCode)
            {
                return new List<CategoryViewModel>();
            }

            var categories = await response.Content
                .ReadFromJsonAsync<List<CategoryViewModel>>();

            return categories ?? new List<CategoryViewModel>();
        }

        private async Task<string?> UploadEventBannerAsync(
            HttpClient client,
            int eventID,
            IFormFile? image)
        {
            // No image selected: nothing to upload
            if (image == null || image.Length == 0)
            {
                return null;
            }

            using var multipartContent = new MultipartFormDataContent();

            using var fileStream = image.OpenReadStream();

            using var streamContent = new StreamContent(fileStream);

            streamContent.Headers.ContentType =
                new System.Net.Http.Headers.MediaTypeHeaderValue(
                    image.ContentType);

            multipartContent.Add(
                streamContent,
                "image",
                Path.GetFileName(image.FileName));

            // Send the image to the RaceDay API
            var response = await client.PostAsync(
                $"api/Events/{eventID}/banner",
                multipartContent);

            if (!response.IsSuccessStatusCode)
            {
                return await response.Content.ReadAsStringAsync();
            }

            return null;
        }

        [HttpGet]
        public async Task<IActionResult> CreateEvent()
        {
            string? token = HttpContext.Session.GetString("JWToken");

            if (token == null ||
                HttpContext.Session.GetString("UserRole") != "Organiser")
            {
                return RedirectToAction("Login", "Auth");
            }

            ViewBag.Categories = await GetCategories(token);

            return View(new CreateEventViewModel());
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateEvent(CreateEventViewModel model)
        {
            string? token = HttpContext.Session.GetString("JWToken");

            if (token == null ||
                HttpContext.Session.GetString("UserRole") != "Organiser")
            {
                return RedirectToAction("Login", "Auth");
            }

            int? organiserID = HttpContext.Session.GetInt32("UserID");

            if (organiserID == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            if (!ModelState.IsValid)
            {
                ViewBag.Categories = await GetCategories(token);
                return View(model);
            }

            var client = _httpClientFactory.CreateClient("RaceDayAPI");

            client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue(
                    "Bearer", token);

            var eventData = new
            {
                categoryID = model.CategoryID,
                name = model.Name,
                description = model.Description,
                eventDate = model.EventDate,
                eventTime = TimeSpan.Parse(model.EventTime),
                location = model.Location,
                entryFee = model.EntryFee,
                maxParticipants = model.MaxParticipants,
                status = model.Status
            };

            try
            {
                var response = await client.PostAsJsonAsync(
                    "api/Events", eventData);

                if (response.IsSuccessStatusCode)
                {
                    // Read the newly created event ID from the API response
                    var createdEvent =
                        await response.Content.ReadFromJsonAsync<System.Text.Json.JsonElement>();

                    int eventID = createdEvent
                        .GetProperty("eventID")
                        .GetInt32();

                    // Upload the banner if the organiser selected one
                    string? bannerError = await UploadEventBannerAsync(
                        client,
                        eventID,
                        model.BannerImage);

                    if (bannerError != null)
                    {
                        TempData["Error"] =
                            "Event was created, but the banner upload failed: "
                            + bannerError;

                        return RedirectToAction("ManageEvents");
                    }

                    TempData["Success"] =
                        "Event created successfully!";

                    return RedirectToAction("ManageEvents");
                }

                string error = await response.Content.ReadAsStringAsync();

                ViewBag.Error = "Unable to create event. API response: " + error;
            }
            catch (HttpRequestException)
            {
                ViewBag.Error = "Could not connect to the RaceDay API.";
            }

            ViewBag.Categories = await GetCategories(token);

            return View(model);
        }
        [HttpGet]
        public async Task<IActionResult> EditEvent(int id)
        {
            string? token = HttpContext.Session.GetString("JWToken");

            if (token == null ||
                HttpContext.Session.GetString("UserRole") != "Organiser")
            {
                return RedirectToAction("Login", "Auth");
            }

            int? organiserID = HttpContext.Session.GetInt32("UserID");

            if (organiserID == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            var client = _httpClientFactory.CreateClient("RaceDayAPI");

            client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue(
                    "Bearer", token);

            try
            {
                var response = await client.GetAsync($"api/Events/{id}");

                if (!response.IsSuccessStatusCode)
                {
                    TempData["Error"] = "Unable to retrieve event.";
                    return RedirectToAction("ManageEvents");
                }

                var raceEvent = await response.Content
                    .ReadFromJsonAsync<EventViewModel>();

                if (raceEvent == null || raceEvent.OrganiserID != organiserID.Value)
                {
                    return RedirectToAction("ManageEvents");
                }

                var model = new CreateEventViewModel
                {
                    Name = raceEvent.Name,
                    Description = raceEvent.Description,
                    CategoryID = raceEvent.CategoryID,
                    EventDate = raceEvent.EventDate,
                    EventTime = raceEvent.EventTime,
                    Location = raceEvent.Location,
                    EntryFee = raceEvent.EntryFee,
                    MaxParticipants = raceEvent.MaxParticipants,
                    Status = raceEvent.Status
                };

                ViewBag.Categories = await GetCategories(token);
                ViewBag.EventID = raceEvent.EventID;

                return View(model);
            }
            catch (HttpRequestException)
            {
                TempData["Error"] = "Could not connect to the API.";
                return RedirectToAction("ManageEvents");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditEvent(
            int id, CreateEventViewModel model)
        {
            string? token = HttpContext.Session.GetString("JWToken");

            if (token == null ||
                HttpContext.Session.GetString("UserRole") != "Organiser")
            {
                return RedirectToAction("Login", "Auth");
            }

            if (!ModelState.IsValid)
            {
                ViewBag.Categories = await GetCategories(token);
                ViewBag.EventID = id;
                return View(model);
            }

            var client = _httpClientFactory.CreateClient("RaceDayAPI");

            client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue(
                    "Bearer", token);

            var updatedEvent = new
            {
                eventID = id,
                categoryID = model.CategoryID,
                name = model.Name,
                description = model.Description,
                eventDate = model.EventDate,
                eventTime = TimeSpan.Parse(model.EventTime),
                location = model.Location,
                entryFee = model.EntryFee,
                maxParticipants = model.MaxParticipants,
                status = model.Status
            };

            try
            {
                var response = await client.PutAsJsonAsync(
                    $"api/Events/{id}", updatedEvent);

                if (response.IsSuccessStatusCode)
                {
                    // Upload a new banner if one was selected.
                    string? bannerError = await UploadEventBannerAsync(
                        client,
                        id,
                        model.BannerImage);

                    if (bannerError != null)
                    {
                        TempData["Error"] =
                            "Event details were updated, but the banner upload failed: "
                            + bannerError;

                        return RedirectToAction("ManageEvents");
                    }

                    TempData["Success"] = "Event updated successfully!";

                    return RedirectToAction("ManageEvents");
                }

                string error = await response.Content.ReadAsStringAsync();

                ViewBag.Error = "Unable to update event: " + error;
            }
            catch (HttpRequestException)
            {
                ViewBag.Error = "Could not connect to the API.";
            }

            ViewBag.Categories = await GetCategories(token);
            ViewBag.EventID = id;

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> DeleteEvent(int id)
        {
            string? token = HttpContext.Session.GetString("JWToken");

            if (token == null ||
                HttpContext.Session.GetString("UserRole") != "Organiser")
            {
                return RedirectToAction("Login", "Auth");
            }

            int? organiserID = HttpContext.Session.GetInt32("UserID");

            if (organiserID == null)
                return RedirectToAction("Login", "Auth");

            var client = _httpClientFactory.CreateClient("RaceDayAPI");

            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            try
            {
                var response = await client.GetAsync($"api/Events/{id}");

                if (!response.IsSuccessStatusCode)
                {
                    TempData["Error"] = "Unable to retrieve the event.";
                    return RedirectToAction("ManageEvents");
                }

                var raceEvent =
                    await response.Content.ReadFromJsonAsync<EventViewModel>();

                if (raceEvent == null ||
                    raceEvent.OrganiserID != organiserID.Value)
                {
                    return RedirectToAction("ManageEvents");
                }

                return View(raceEvent);
            }
            catch (HttpRequestException)
            {
                TempData["Error"] = "Could not connect to the API.";
                return RedirectToAction("ManageEvents");
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmDeleteEvent(int id)
        {
            string? token = HttpContext.Session.GetString("JWToken");

            if (token == null ||
                HttpContext.Session.GetString("UserRole") != "Organiser")
            {
                return RedirectToAction("Login", "Auth");
            }

            var client = _httpClientFactory.CreateClient("RaceDayAPI");

            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            try
            {
                var response = await client.DeleteAsync($"api/Events/{id}");

                if (response.IsSuccessStatusCode)
                {
                    TempData["Success"] = "Event deleted successfully!";
                    return RedirectToAction("ManageEvents");
                }

                string error = await response.Content.ReadAsStringAsync();

                TempData["Error"] = "Unable to delete event: " + error;

                return RedirectToAction("ManageEvents");
            }
            catch (HttpRequestException)
            {
                TempData["Error"] = "Could not connect to the API.";
                return RedirectToAction("ManageEvents");
            }
        }
        [HttpGet]
        public async Task<IActionResult> ManageRegistrations()
        {
            string? token = HttpContext.Session.GetString("JWToken");

            if (token == null ||
                HttpContext.Session.GetString("UserRole") != "Organiser")
            {
                return RedirectToAction("Login", "Auth");
            }

            int? organiserID = HttpContext.Session.GetInt32("UserID");

            if (organiserID == null)
                return RedirectToAction("Login", "Auth");

            var client = _httpClientFactory.CreateClient("RaceDayAPI");

            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            try
            {
                // Retrieve the organiser's events.
                var eventResponse = await client.GetAsync("api/Events");

                if (!eventResponse.IsSuccessStatusCode)
                {
                    ViewBag.Error = "Unable to retrieve events.";
                    return View(new List<RegistrationViewModel>());
                }

                var events = await eventResponse.Content
                    .ReadFromJsonAsync<List<EventViewModel>>();

                var organiserEvents = events?
                    .Where(e => e.OrganiserID == organiserID.Value)
                    .ToList() ?? new List<EventViewModel>();

                var organiserEventIDs = organiserEvents
                    .Select(e => e.EventID)
                    .ToList();

                // Retrieve registrations.
                var registrationResponse =
                    await client.GetAsync("api/Registrations");

                if (!registrationResponse.IsSuccessStatusCode)
                {
                    ViewBag.Error = "Unable to retrieve registrations.";
                    return View(new List<RegistrationViewModel>());
                }

                var registrations = await registrationResponse.Content
                    .ReadFromJsonAsync<List<RegistrationViewModel>>();

                // Only show registrations for this organiser's events.
                var organiserRegistrations = registrations?
                    .Where(r => organiserEventIDs.Contains(r.EventID))
                    .OrderByDescending(r => r.RegistrationDate)
                    .ToList() ?? new List<RegistrationViewModel>();

                ViewBag.TotalRegistrations = organiserRegistrations.Count;
                ViewBag.TotalEvents = organiserEvents.Count;

                return View(organiserRegistrations);
            }
            catch (HttpRequestException)
            {
                ViewBag.Error = "Could not connect to the API.";
                return View(new List<RegistrationViewModel>());
            }
        }
        // GET: Organiser/ManageResults
        public async Task<IActionResult> ManageResults()
        {
            var token = HttpContext.Session.GetString("JWToken");
            var role = HttpContext.Session.GetString("UserRole");
            var organiserID = HttpContext.Session.GetInt32("UserID");

            if (string.IsNullOrEmpty(token) ||
                role != "Organiser" ||
                organiserID == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            var client = _httpClientFactory.CreateClient("RaceDayAPI");

            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            try
            {
                // Get organiser's events
                var eventsResponse = await client.GetAsync("api/Events");

                if (!eventsResponse.IsSuccessStatusCode)
                {
                    TempData["Error"] = "Unable to retrieve events.";
                    return View(new List<RaceResultViewModel>());
                }

                var events = await eventsResponse.Content
                    .ReadFromJsonAsync<List<EventViewModel>>();

                var organiserEvents = events?
                    .Where(e => e.OrganiserID == organiserID.Value)
                    .ToList() ?? new List<EventViewModel>();

                var eventIDs = organiserEvents
                    .Select(e => e.EventID)
                    .ToList();

                // Get all recorded results
                var resultsResponse = await client.GetAsync("api/Results");

                if (!resultsResponse.IsSuccessStatusCode)
                {
                    TempData["Error"] = "Unable to retrieve race results.";
                    return View(new List<RaceResultViewModel>());
                }

                var results = await resultsResponse.Content
                    .ReadFromJsonAsync<List<RaceResultViewModel>>();

                var organiserResults = results?
    .Where(r => eventIDs.Contains(r.EventID))
    .ToList() ?? new List<RaceResultViewModel>();

                return View(organiserResults);
            }
            catch (HttpRequestException)
            {
                TempData["Error"] = "Unable to connect to the RaceDay API.";
                return View(new List<RaceResultViewModel>());
            }
        }
        // GET: Organiser/RecordResult
        [HttpGet]
        public async Task<IActionResult> RecordResult()
        {
            var token = HttpContext.Session.GetString("JWToken");
            var role = HttpContext.Session.GetString("UserRole");
            var organiserID = HttpContext.Session.GetInt32("UserID");

            if (string.IsNullOrEmpty(token) ||
                role != "Organiser" ||
                organiserID == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            var client = _httpClientFactory.CreateClient("RaceDayAPI");

            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            try
            {
                var eventsResponse = await client.GetAsync("api/Events");

                if (!eventsResponse.IsSuccessStatusCode)
                {
                    TempData["Error"] = "Unable to retrieve events.";
                    return RedirectToAction("ManageResults");
                }

                var events = await eventsResponse.Content
                    .ReadFromJsonAsync<List<EventViewModel>>();

                var organiserEvents = events?
                    .Where(e => e.OrganiserID == organiserID.Value)
                    .ToList() ?? new List<EventViewModel>();

                var registrationsResponse = await client.GetAsync("api/Registrations");

                if (!registrationsResponse.IsSuccessStatusCode)
                {
                    TempData["Error"] = "Unable to retrieve registrations.";
                    return RedirectToAction("ManageResults");
                }

                var registrations = await registrationsResponse.Content
                    .ReadFromJsonAsync<List<RegistrationViewModel>>();

                var eventIDs = organiserEvents.Select(e => e.EventID).ToList();

                var availableRegistrations = registrations?
                    .Where(r => eventIDs.Contains(r.EventID))
                    .ToList() ?? new List<RegistrationViewModel>();

                var model = new RecordResultViewModel
                {
                    Registrations = availableRegistrations
                };

                return View(model);
            }
            catch (HttpRequestException)
            {
                TempData["Error"] = "Unable to connect to the RaceDay API.";
                return RedirectToAction("ManageResults");
            }
        }
        // POST: Organiser/RecordResult
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RecordResult(RecordResultViewModel model)
        {
            var token = HttpContext.Session.GetString("JWToken");
            var role = HttpContext.Session.GetString("UserRole");

            if (string.IsNullOrEmpty(token) || role != "Organiser")
            {
                return RedirectToAction("Login", "Auth");
            }

            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Please enter valid result details.";
                return RedirectToAction("RecordResult");
            }

            var client = _httpClientFactory.CreateClient("RaceDayAPI");

            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            try
            {
                TimeSpan? finishTime = null;

                if (!string.IsNullOrWhiteSpace(model.FinishTime))
                {
                    if (!TimeSpan.TryParse(model.FinishTime, out TimeSpan parsedTime))
                    {
                        ModelState.AddModelError(
                            "FinishTime", "Please enter a valid finish time.");

                        return View(model);
                    }

                    finishTime = parsedTime;
                }

                var resultData = new
                {
                    registrationID = model.RegistrationID,
                    position = model.Position,
                    startPosition = model.StartPosition,
                    finishTime = finishTime,
                    pace = model.Pace,
                    status = model.Status
                };

                var response = await client.PostAsJsonAsync(
                    "api/Results", resultData);

                if (response.IsSuccessStatusCode)
                {
                    TempData["Success"] = "Race result recorded successfully!";
                    return RedirectToAction("ManageResults");
                }

                var error = await response.Content.ReadAsStringAsync();

                TempData["Error"] = "Unable to record result: " + error;

                return RedirectToAction("RecordResult");
            }
            catch (HttpRequestException)
            {
                TempData["Error"] = "Unable to connect to the RaceDay API.";
                return RedirectToAction("ManageResults");
            }
        }
    }
}
