using Microsoft.AspNetCore.Mvc;
using RaceDayMVC.Models.ViewModels;
using System.Net.Http.Json;
using System.Text.Json;


namespace RaceDayMVC.Controllers
{
    public class AuthController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public AuthController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        // GET: Login
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        // POST: Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var client = _httpClientFactory.CreateClient("RaceDayAPI");

            var response = await client.PostAsJsonAsync(
                "api/auth/login", model);

            if (!response.IsSuccessStatusCode)
            {
                ModelState.AddModelError(string.Empty,
                    "Invalid email or password. Please try again.");

                return View(model);
            }

            var json = await response.Content.ReadAsStringAsync();

            using var document = JsonDocument.Parse(json);
            var root = document.RootElement;

            string token = root.GetProperty("token").GetString() ?? "";

            if (string.IsNullOrWhiteSpace(token))
            {
                ModelState.AddModelError(string.Empty,
                    "The API did not return an authentication token.");

                return View(model);
            }

            var user = root.GetProperty("user");

            string firstName = user.TryGetProperty("firstName", out var first)
                ? first.GetString() ?? ""
                : "";

            string email = user.TryGetProperty("email", out var emailValue)
                ? emailValue.GetString() ?? model.Email
                : model.Email;

            int userId = user.TryGetProperty("userID", out var idValue)
                ? idValue.GetInt32()
                : 0;

            string role = "Participant";

            if (user.TryGetProperty("roles", out var roles) &&
                roles.ValueKind == JsonValueKind.Array)
            {
                foreach (var item in roles.EnumerateArray())
                {
                    string currentRole = item.GetString() ?? "";

                    if (currentRole == "Organiser")
                    {
                        role = "Organiser";
                        break;
                    }

                    if (currentRole == "Participant")
                        role = "Participant";
                }
            }

            HttpContext.Session.SetString("JWToken", token);
            HttpContext.Session.SetString("UserRole", role);
            HttpContext.Session.SetString("UserEmail", email);
            HttpContext.Session.SetString("FirstName", firstName);
            HttpContext.Session.SetInt32("UserID", userId);

            if (role == "Organiser")
                return RedirectToAction("Dashboard", "Organiser");

            return RedirectToAction("Dashboard", "Participant");
        }

        // GET: Register
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        // POST: Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            if (model.Role != "Participant" && model.Role != "Organiser")
            {
                ModelState.AddModelError("Role", "Select a valid role.");
                return View(model);
            }

            var client = _httpClientFactory.CreateClient("RaceDayAPI");

            var registrationData = new
            {
                firstName = model.FirstName,
                lastName = model.LastName,
                email = model.Email,
                password = model.Password,
                phone = model.Phone,
                role = model.Role
            };

            var response = await client.PostAsJsonAsync(
                "api/auth/register", registrationData);

            if (!response.IsSuccessStatusCode)
            {
                string error = await response.Content.ReadAsStringAsync();

                ModelState.AddModelError(string.Empty,
                    "Registration failed. " + error);

                return View(model);
            }

            TempData["Success"] =
                "Registration successful! You can now log in.";

            return RedirectToAction("Login");
        }

        // Logout
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();

            return RedirectToAction("Login");
        }
    }
}
