using Microsoft.AspNetCore.Mvc;

namespace DemoLoginApp.Controllers
{
    public class AccountController : Controller
    {
        // GET: Account/Login
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login(string username, string password)
        {
            if (username == "admin" && password == "admin123")
            {
                ViewBag.Message = "Login successful!";
                return RedirectToAction("Index", "Home");
            }

            ViewBag.Message = "Invalid username or password";
            return View();
        }

        // GET: Account/Signup
        [Route("Account/Signup")]
        public IActionResult Signup()
        {
            return View();
        }

        // POST: Account/Register
        [HttpPost]
        [Route("Account/Register")]
        public IActionResult Register(string username, string password, string confirmPassword)
        {
            if (password != confirmPassword)
            {
                ViewBag.Message = "Passwords do not match.";
                return View(); 
            }

            // Simulate user registration logic
            ViewBag.Message = "Registration successful!";
            return RedirectToAction("Login");
        }
    }
}
