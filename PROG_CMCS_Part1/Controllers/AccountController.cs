using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace PROG_CMCS_Part1.Controllers
{
    public class AccountController : Controller
    {
        // Handles user sign-in
        private readonly SignInManager<ApplicationUser> _signInManager;
        // Handles user management
        private readonly UserManager<ApplicationUser> _userManager;
        // Constructor injects ASP.NET Identity services
        public AccountController(
            SignInManager<ApplicationUser> signInManager,
            UserManager<ApplicationUser> userManager)
        {
            _signInManager = signInManager;
            _userManager = userManager;
        }
        // GET: Display the login page
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }
        // POST: Handle login form submission

        [HttpPost]
        public async Task<IActionResult> Login(string email, string password)
        {
            // Basic validation: ensure email and password are provided
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                ViewBag.Error = "Email and password are required.";
                return View();
            }
            // Attempt to sign in the user with the provided credentials
            var result = await _signInManager.PasswordSignInAsync(email, password, false, false);

            if (result.Succeeded)
            {
                // Redirect to Home page on successful login
                return RedirectToAction("Index", "Home");
            }
            // Show error if login fails
            ViewBag.Error = "Invalid login attempt.";
            return View();
        }
        // Handle user logout
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            // Redirect to login page after logout
            return RedirectToAction("Login");
        }
    }
}