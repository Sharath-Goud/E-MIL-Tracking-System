using E_MIL_Tracking_system.DTOs;
using E_MIL_Tracking_system.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace E_MIL_Tracking_system.Controllers
{
    public class AuthController : Controller
    {
        private readonly IAuthService _service;
        private readonly IWebHostEnvironment _env;

        public AuthController(IAuthService service, IWebHostEnvironment env)
        {
            _service = service;
            _env = env;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginDto model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Error = "Please enter Employee ID and Password.";
                return View(model);
            }

            var user = await _service.GetUserAsync(model.EmpId, model.Password);

            if (user != null)
            {
                HttpContext.Session.SetString("EmpId", user.EmpId ?? "");
                HttpContext.Session.SetString("FullName", user.FullName ?? "");
                HttpContext.Session.SetString("Designation", user.Designation ?? "");
                HttpContext.Session.SetString("Role", user.Role ?? "");
                HttpContext.Session.SetString("ProfileImagePath", user.ProfileImagePath ?? "");

                return RedirectToAction("Index", "Dashboard");
            }

            ViewBag.Error = "Invalid credentials";
            return View(model);
        }

        [HttpGet]
        public IActionResult Registration()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Registration(RegisterDto model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Error = "Please fill all required fields.";
                return View(model);
            }

            if (model.Password != model.ConfirmPassword)
            {
                ViewBag.Error = "Password and Confirm Password do not match.";
                return View(model);
            }

            if (string.IsNullOrWhiteSpace(model.Role))
            {
                ViewBag.Error = "Please select Admin or User.";
                return View(model);
            }

            bool exists = await _service.UserExistsAsync(model.UserId);

            if (exists)
            {
                ViewBag.Error = "Employee ID already exists.";
                return View(model);
            }

            bool result = await _service.RegisterAsync(model, _env.WebRootPath);

            if (result)
            {
                TempData["SuccessMessage"] = "Registration completed successfully.";
                return RedirectToAction("Index", "Dashboard");
            }

            ViewBag.Error = "Registration failed. Please try again.";
            return View(model);
        }
    }
}