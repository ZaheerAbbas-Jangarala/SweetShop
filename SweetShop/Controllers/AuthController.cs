using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SweetShop.Mvc.Models;
using SweetShop.Mvc.Services;
using System.Data;

namespace SweetShop.Mvc.Controllers
{
    public class AuthController : Controller
    {
        private readonly AuthService _authService;

        public AuthController(AuthService authService)
        {
            _authService = authService;
        }

        // GET: Login page (Sign In + Sign Up)
        [HttpGet]
        public IActionResult Login()
        {
            return View(); // Login.cshtml
        }

        // POST: Register
        [HttpPost]
        public async Task<IActionResult> Register(AuthRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Name))
                return BadRequest(new { error = "Name is required." });

            if (string.IsNullOrWhiteSpace(request.Email))
                return BadRequest(new { error = "Email is required." });

            if (string.IsNullOrWhiteSpace(request.Password))
                return BadRequest(new { error = "Password is required." });
            if (ModelState.IsValid)
            {
                BadRequest(new { error = "Email is Invalid." });
            }
            try
            {
                var result = await _authService.RegisterAsync(request);
                return Ok(new { message = "Registration successful! You can now login." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        // POST: Login
        [HttpPost]
        public async Task<IActionResult> Login([FromBody] AuthRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Email))
                return BadRequest(new { error = "Email is required." });

            if (string.IsNullOrWhiteSpace(request.Password))
                return BadRequest(new { error = "Password is required." });
           
            try
            {
                var result = await _authService.LoginAsync(request);

                // Check if user exists and password matches
                if (result != null && result.User != null)
                {
                    // Store session info (optional if needed)
                    HttpContext.Session.SetString("JWToken", result.Token);
                    HttpContext.Session.SetInt32("UserId", result.User.Id);
                    HttpContext.Session.SetString("UserRole", result.User.Role);
                    HttpContext.Session.SetString("UserName", result.User.Name);
                    HttpContext.Session.SetString("UserEmail", result.User.Email);

                    // Send JSON response including role for client-side redirect
                    return Ok(new
                    {
                        message = "Login successful!",
                        user = new
                        {
                            result.User.Id,
                            result.User.Name,
                            result.User.Email,
                            result.User.Role
                        }
                    });
                }
                else
                {
                    // Invalid credentials
                    return Unauthorized(new { error = "Invalid Email or Password." });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }


        // Logout
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            TempData["Success"] = "Logged out successfully.";
            return RedirectToAction("Login");
        }
    }
}
