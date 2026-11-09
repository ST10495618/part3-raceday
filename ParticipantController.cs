using Microsoft.AspNetCore.Mvc;
using RaceDayMVC.Models.ViewModels;
using System.Text.Json;

namespace RaceDayMVC.Controllers
{
    public class ParticipantController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public ParticipantController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<IActionResult> Dashboard()
        {
            // Check whether the participant is logged in
            if (HttpContext.Session.GetString("JWToken") == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            // Check the user's role
            if (HttpContext.Session.GetString("UserRole") != "Participant")
            {
                return RedirectToAction("Login", "Auth");
            }

            ViewBag.FirstName =
                HttpContext.Session.GetString("FirstName");

            var client = _httpClientFactory.CreateClient("RaceDayAPI");

            try
            {
                // Get events from the RaceDay API
                var response = await client.GetAsync("api/events");

                if (response.IsSuccessStatusCode)
                {
                    var options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    };

                    var events = await response.Content
                        .ReadFromJsonAsync<List<EventViewModel>>(options);

                    // Display only upcoming open events
                    var upcomingEvents = events?
                        .Where(e => e.EventDate.Date >= DateTime.Today
                                    && e.Status == "Open")
                        .OrderBy(e => e.EventDate)
                        .ToList();

                    return View(upcomingEvents ?? new List<EventViewModel>());
                }

                ViewBag.Error = "Unable to load events from the API.";
            }
            catch (HttpRequestException)
            {
                ViewBag.Error = "Cannot connect to RaceDay API. Please make sure it is running.";
            }
            catch (JsonException)
            {
                ViewBag.Error = "The API returned event data in an unexpected format.";
            }

            return View(new List<EventViewModel>());
        }
        public async Task<IActionResult> Details(int id)
        {
            // Check if the user is logged in
            string? token = HttpContext.Session.GetString("JWToken");

            if (token == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            // Check if the user is a participant
            if (HttpContext.Session.GetString("UserRole") != "Participant")
            {
                return RedirectToAction("Login", "Auth");
            }

            ViewBag.FirstName = HttpContext.Session.GetString("FirstName");

            var client = _httpClientFactory.CreateClient("RaceDayAPI");

            try
            {
                // Request the selected event from the API
                var response = await client.GetAsync($"api/events/{id}");

                if (!response.IsSuccessStatusCode)
                {
                    return NotFound();
                }

                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                var eventDetails = await response.Content
                    .ReadFromJsonAsync<EventViewModel>(options);

                if (eventDetails == null)
                {
                    return NotFound();
                }

                // Set the banner URL through the RaceDay API
                if (!string.IsNullOrWhiteSpace(eventDetails.BannerImageUrl))
                {
                    eventDetails.BannerImageUrl =
                        $"api/Events/{id}/banner";
                }

                return View(eventDetails);
            }
            catch (HttpRequestException)
            {
                ViewBag.Error = "Cannot connect to RaceDay API. Please try again.";
                return View("Error");
            }
            catch (JsonException)
            {
                ViewBag.Error = "The event information could not be loaded.";
                return View("Error");
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(int eventID)
        {
            // Get the participant's JWT token
            string? token = HttpContext.Session.GetString("JWToken");

            if (token == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            // Check participant role
            if (HttpContext.Session.GetString("UserRole") != "Participant")
            {
                return RedirectToAction("Login", "Auth");
            }

            var client = _httpClientFactory.CreateClient("RaceDayAPI");

            // Attach JWT token to the API request
            client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue(
                    "Bearer", token);

            try
            {
                // Send registration request to the API
                var response = await client.PostAsJsonAsync(
                    "api/registrations",
                    new { eventID = eventID }
                );

                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] =
                        "You have successfully registered for this event!";

                    return RedirectToAction("Details", new { id = eventID });
                }

                // Read the API error response
                string errorContent =
                    await response.Content.ReadAsStringAsync();

                string errorMessage = "Registration failed. Please try again.";

                try
                {
                    using var document = JsonDocument.Parse(errorContent);

                    if (document.RootElement.TryGetProperty(
                        "message", out var message))
                    {
                        errorMessage = message.GetString() ?? errorMessage;
                    }
                }
                catch (JsonException)
                {
                    // Keep the default error message
                }

                TempData["ErrorMessage"] = errorMessage;

                return RedirectToAction("Details", new { id = eventID });
            }
            catch (HttpRequestException)
            {
                TempData["ErrorMessage"] =
                    "Cannot connect to the RaceDay API. Please try again.";

                return RedirectToAction("Details", new { id = eventID });
            }
        }
        public async Task<IActionResult> MyRegistrations()
        {
            // Check if participant is logged in
            string? token = HttpContext.Session.GetString("JWToken");

            if (token == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            // Check participant role
            if (HttpContext.Session.GetString("UserRole") != "Participant")
            {
                return RedirectToAction("Login", "Auth");
            }

            ViewBag.FirstName = HttpContext.Session.GetString("FirstName");

            var client = _httpClientFactory.CreateClient("RaceDayAPI");

            // Attach JWT token
            client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue(
                    "Bearer", token);

            try
            {
                // Get the participant's registrations
                var response = await client.GetAsync("api/registrations");

                if (response.IsSuccessStatusCode)
                {
                    var options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    };

                    var registrations = await response.Content
                        .ReadFromJsonAsync<List<RegistrationViewModel>>(options);

                    return View(registrations ?? new List<RegistrationViewModel>());
                }

                ViewBag.Error = "Unable to load your registrations.";
            }
            catch (HttpRequestException)
            {
                ViewBag.Error = "Cannot connect to RaceDay API. Please make sure it is running.";
            }
            catch (JsonException)
            {
                ViewBag.Error = "The registration information was returned in an unexpected format.";
            }

            return View(new List<RegistrationViewModel>());
        }

        public async Task<IActionResult> MyResults()
        {
            // Check if participant is logged in
            string? token = HttpContext.Session.GetString("JWToken");

            if (token == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            // Check participant role
            if (HttpContext.Session.GetString("UserRole") != "Participant")
            {
                return RedirectToAction("Login", "Auth");
            }

            // Get logged-in participant ID
            int? userIDValue = HttpContext.Session.GetInt32("UserID");

            if (userIDValue == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            int userID = userIDValue.Value;

            ViewBag.FirstName = HttpContext.Session.GetString("FirstName");

            var client = _httpClientFactory.CreateClient("RaceDayAPI");

            client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue(
                    "Bearer", token);

            try
            {
                // Get results from API
                var response = await client.GetAsync("api/results");

                if (response.IsSuccessStatusCode)
                {
                    var options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    };

                    var allResults = await response.Content
                        .ReadFromJsonAsync<List<ResultViewModel>>(options);

                    // Only display results belonging to logged-in participant
                    var myResults = allResults?
                        .Where(r => r.UserID == userID)
                        .OrderBy(r => r.EventDate)
                        .ToList();

                    return View(myResults ?? new List<ResultViewModel>());
                }

                ViewBag.Error = "Unable to load your race results.";
            }
            catch (HttpRequestException)
            {
                ViewBag.Error =
                    "Cannot connect to RaceDay API. Please make sure it is running.";
            }
            catch (JsonException)
            {
                ViewBag.Error =
                    "The results information was returned in an unexpected format.";
            }

            return View(new List<ResultViewModel>());
        }

        public async Task<IActionResult> MyProfile()
        {
            // Check if participant is logged in
            string? token = HttpContext.Session.GetString("JWToken");

            if (token == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            // Check participant role
            if (HttpContext.Session.GetString("UserRole") != "Participant")
            {
                return RedirectToAction("Login", "Auth");
            }

            ViewBag.FirstName = HttpContext.Session.GetString("FirstName");

            var client = _httpClientFactory.CreateClient("RaceDayAPI");

            // Attach JWT token
            client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue(
                    "Bearer", token);

            try
            {
                // Get logged-in user's profile from API
                var response = await client.GetAsync("api/Users/me");

                if (response.IsSuccessStatusCode)
                {
                    var options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    };

                    var profile = await response.Content
                        .ReadFromJsonAsync<UserProfileViewModel>(options);

                    if (profile != null)
                    {
                        return View(profile);
                    }
                }

                ViewBag.Error = "Unable to load your profile information.";
            }
            catch (HttpRequestException)
            {
                ViewBag.Error = "Cannot connect to RaceDay API. Please make sure it is running.";
            }
            catch (JsonException)
            {
                ViewBag.Error = "The profile information was returned in an unexpected format.";
            }

            return View(new UserProfileViewModel());
        }

        [HttpGet]
        public async Task<IActionResult> EditProfile()
        {
            string? token = HttpContext.Session.GetString("JWToken");

            if (token == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            if (HttpContext.Session.GetString("UserRole") != "Participant")
            {
                return RedirectToAction("Login", "Auth");
            }

            var client = _httpClientFactory.CreateClient("RaceDayAPI");

            client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue(
                    "Bearer", token);

            try
            {
                var response = await client.GetAsync("api/Users/me");

                if (response.IsSuccessStatusCode)
                {
                    var options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    };

                    var profile = await response.Content
                        .ReadFromJsonAsync<UserProfileViewModel>(options);

                    if (profile != null)
                    {
                        var model = new UpdateProfileViewModel
                        {
                            FirstName = profile.FirstName,
                            LastName = profile.LastName,
                            Email = profile.Email,
                            Phone = profile.Phone
                        };

                        return View(model);
                    }
                }

                TempData["Error"] = "Unable to load your profile for editing.";
            }
            catch (HttpRequestException)
            {
                TempData["Error"] = "Cannot connect to RaceDay API.";
            }
            catch (JsonException)
            {
                TempData["Error"] = "Unexpected profile information received.";
            }

            return RedirectToAction("MyProfile");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProfile(UpdateProfileViewModel model)
        {
            string? token = HttpContext.Session.GetString("JWToken");

            if (token == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            if (HttpContext.Session.GetString("UserRole") != "Participant")
            {
                return RedirectToAction("Login", "Auth");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var client = _httpClientFactory.CreateClient("RaceDayAPI");

            client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue(
                    "Bearer", token);

            try
            {
                var response = await client.PutAsJsonAsync(
                    "api/Users/me", model);

                if (response.IsSuccessStatusCode)
                {
                    // Update session information
                    HttpContext.Session.SetString(
                        "FirstName", model.FirstName);

                    HttpContext.Session.SetString(
                        "UserEmail", model.Email);

                    TempData["Success"] = "Your profile has been updated successfully.";

                    return RedirectToAction("MyProfile");
                }

                if (response.StatusCode == System.Net.HttpStatusCode.Conflict)
                {
                    ModelState.AddModelError(
                        "Email",
                        "That email address is already in use.");
                }
                else
                {
                    ModelState.AddModelError(
                        string.Empty,
                        "Unable to update your profile. Please try again.");
                }
            }
            catch (HttpRequestException)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Cannot connect to RaceDay API. Please make sure it is running.");
            }
            catch (System.Text.Json.JsonException)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "The API returned an unexpected response.");
            }

            return View(model);
        }


    }
}
