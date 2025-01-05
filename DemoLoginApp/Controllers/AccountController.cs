using Microsoft.AspNetCore.Mvc;
using DemoLoginApp.Models;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Identity.UI.Services;
using DemoLoginApp.Services;

namespace DemoLoginApp.Controllers
{
    public class AccountController : Controller
    {

        private readonly ApplicationDbContext _context;
        private readonly IEmailSender _emailSender;
        private readonly UserManager<User> _userManager;

        public AccountController(ApplicationDbContext context, ICustomEmailSender emailSender, UserManager<User> userManager)
        {
            _context = context;
            _emailSender = emailSender;
            _userManager = userManager;
        }

        // GET: Account/Login
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string username, string password)
        {
            // find the user in the database  
            var user = await _userManager.FindByNameAsync(username);

            if (username != null)
            {
                // check if the password is correct
                var passwordCheck = await _userManager.CheckPasswordAsync(user, password);

                if (passwordCheck)
                {
                    ViewBag.Message = "Login successful!";
                    return RedirectToAction("Index", "Home");
                }
            }
            if (user == null)
            {
                ViewBag.Message = "Invalid username or password";
            }
            return View();
        }

        // GET: Account/Signup
        [Route("Account/Signup")]
        public IActionResult Signup()
        {
            return View();
        }

        // POST: Account/Signup
        [HttpPost]
        [Route("Account/Signup")]
        public async Task<IActionResult> Register(string username, string password, string confirmPassword, string email)
        {
            try
            {
                // Log the username to debug the value
                Console.WriteLine($"Received username: {username}");

                // Check for null or empty username
                if (string.IsNullOrWhiteSpace(username))
                {
                    ViewBag.Message = "Username is required.";
                    return View("Signup");
                }

                // Custom validation for Username
                if (!System.Text.RegularExpressions.Regex.IsMatch(username, @"^[a-zA-Z0-9]+$"))
                {
                    ViewBag.Message = "Username can only contain letters and digits.";
                    return View("Signup");
                }
                // Validate password match  
                if (password != confirmPassword)
                {
                    ViewBag.Message = "Passwords do not match.";
                    return View("Signup");
                }

                var user = new User
                {
                    UserName = username,
                    Email = email,
                    UserType = "Patient"
                };

                var result = await _userManager.CreateAsync(user, password);

                if (result.Succeeded)
                {
                    var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    var callbackUrl = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, code }, protocol: Request.Scheme);
                    await _emailSender.SendEmailAsync(email, "Confirm your account",
                        $"Please confirm your account by clicking this link: <a href='{callbackUrl}'>link</a>");

                    ViewBag.Message = "Registration successful! Please check your email to confirm your account.";
                    return RedirectToAction("Login");
                }

                // Handle errors if the registration fails  
                AddErrors(result);
            }
            catch (Exception ex)
            {
                // Log the exception (log in a file or use a logging framework)  
                ModelState.AddModelError(string.Empty, "An unexpected error occurred. Please try again.");
            }

            return View("Signup"); // return to the same view to display errors  
        }

        // email confirmation action  
        [HttpGet]
        public async Task<IActionResult> ConfirmEmail(string userId, string code)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null || code == null)
            {
                return View("Error");
            }

            // Confirm the email  
            var result = await _userManager.ConfirmEmailAsync(user, code);
            if (result.Succeeded)
            {
                return View("ConfirmedEmail");
            }

            return View("Error"); // Handle potential errors on confirmation  
        }
        public async Task<IActionResult> ForgotPassword(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null || !(await _userManager.IsEmailConfirmedAsync(user)))
            {
                return View("ForgotPasswordConfirmation");
            }

            var code = await _userManager.GeneratePasswordResetTokenAsync(user);
            var callbackUrl = Url.Action("ResetPassword", "Account", new { userId = user.UserID, code }, protocol: Request.Scheme);
            await _emailSender.SendEmailAsync(email, "Reset Password",
                $"Please reset your password by clicking here: <a href='{callbackUrl}'>link</a>");

            return View("ForgotPasswordConfirmation");
        }

        [HttpGet]
        public ActionResult ResetPassword(string code = null)
        {
            return code == null ? View("Error") : View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user == null)
                {
                    return RedirectToAction("ResetPasswordConfirmation", "Account");
                }

                var result = await _userManager.ResetPasswordAsync(user, model.Code, model.Password);
                if (result.Succeeded)
                {
                    return RedirectToAction("ResetPasswordConfirmation", "Account");
                }

                AddErrors(result); // Handle errors if reset fails  
            }
            return View(model);
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }

    }
}
