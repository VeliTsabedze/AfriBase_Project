using AfriBase.Data;
using AfriBase.Models;
using AfriBase.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace AfriBase.Controllers
{
    /// <summary>
    /// Controller handling user authentication and account management
    /// </summary>
    public class AccountController(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        ApplicationDbContext context,
        IWebHostEnvironment webHostEnvironment,
        ILogger<AccountController> logger) : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager = userManager;
        private readonly SignInManager<ApplicationUser> _signInManager = signInManager;
        private readonly ApplicationDbContext _context = context;
        private readonly IWebHostEnvironment _webHostEnvironment = webHostEnvironment;
        private readonly ILogger<AccountController> _logger = logger;

        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (ModelState.IsValid)
            {
                try
                {
                    var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, lockoutOnFailure: true);

                    if (result.Succeeded)
                    {
                        var user = await _userManager.FindByEmailAsync(model.Email);
                        if (user != null)
                        {
                            user.LastLoginDate = DateTime.Now;
                            await _userManager.UpdateAsync(user);
                        }

                        HttpContext.Session.SetString("IsLoggedIn", "true");
                        return RedirectToLocal(returnUrl);
                    }
                    if (result.RequiresTwoFactor)
                    {
                        return RedirectToAction("LoginWith2fa", new { returnUrl, model.RememberMe });
                    }
                    if (result.IsLockedOut)
                    {
                        return RedirectToAction(nameof(Lockout));
                    }

                    ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Login failed for user {Email}", model.Email);
                    ModelState.AddModelError(string.Empty, "An error occurred while processing your request. Please try again later.");
                }
            }

            return View(model);
        }

        [HttpGet]
        public IActionResult Register(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (ModelState.IsValid)
            {
                try
                {
                    var user = new ApplicationUser
                    {
                        UserName = model.Email,
                        Email = model.Email,
                        FirstName = model.FirstName,
                        LastName = model.LastName,
                        PhoneNumber = model.PhoneNumber,
                        Country = model.Country,
                        Bio = model.Bio,
                        RegistrationDate = DateTime.Now,
                        ProfileImageUrl = "/images/default-profile.jpg"
                    };

                    var result = await _userManager.CreateAsync(user, model.Password);

                    if (result.Succeeded)
                    {
                        await _userManager.AddToRoleAsync(user, "User");
                        await _userManager.AddToRoleAsync(user, "Bidder");
                        await _signInManager.SignInAsync(user, isPersistent: false);
                        HttpContext.Session.SetString("IsLoggedIn", "true");
                        return RedirectToLocal(returnUrl);
                    }

                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Registration failed for user {Email}", model.Email);
                    ModelState.AddModelError(string.Empty, "An error occurred while processing your request. Please try again later.");
                }
            }

            return View(model);
        }

        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var user = await _userManager.FindByEmailAsync(model.Email);
                    if (user == null || !(await _userManager.IsEmailConfirmedAsync(user)))
                    {
                        return RedirectToAction(nameof(ForgotPasswordConfirmation));
                    }

                    var code = await _userManager.GeneratePasswordResetTokenAsync(user);
                    var callbackUrl = Url.Action("ResetPassword", "Account",
                                             new { userId = user.Id, code },
                                             protocol: HttpContext.Request.Scheme);

                    // You could send email here

                    return RedirectToAction(nameof(ForgotPasswordConfirmation));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "ForgotPassword failed for {Email}", model.Email);
                    ModelState.AddModelError(string.Empty, "An error occurred while processing your request. Please try again later.");
                }
            }

            return View(model);
        }

        [HttpGet]
        public IActionResult ForgotPasswordConfirmation()
        {
            return View();
        }

        [HttpGet]
        public IActionResult ResetPassword(string? code = null)
        {
            if (code == null)
            {
                return BadRequest("A code must be supplied for password reset.");
            }

            var model = new ResetPasswordViewModel { Code = code };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user == null)
                {
                    return RedirectToAction(nameof(ResetPasswordConfirmation));
                }

                var result = await _userManager.ResetPasswordAsync(user, model.Code, model.Password);
                if (result.Succeeded)
                {
                    return RedirectToAction(nameof(ResetPasswordConfirmation));
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ResetPassword failed for {Email}", model.Email);
                ModelState.AddModelError(string.Empty, "An error occurred while processing your request. Please try again later.");
            }

            return View(model);
        }

        [HttpGet]
        public IActionResult ResetPasswordConfirmation()
        {
            return View();
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Profile()
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return NotFound();
                }

                var recentBids = await _context.Bids
                    .Where(b => b.UserId == user.Id)
                    .OrderByDescending(b => b.BidDate)
                    .Take(5)
                    .ToListAsync();

                var ownedArtifacts = await _context.Artifacts
                    .Where(a => a.OwnerId == user.Id)
                    .OrderByDescending(a => a.DateAdded)
                    .Take(5)
                    .ToListAsync();

                var model = new ProfileViewModel
                {
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email,
                    PhoneNumber = user.PhoneNumber,
                    Country = user.Country,
                    Bio = user.Bio,
                    ProfileImageUrl = user.ProfileImageUrl,
                    RecentBids = recentBids,
                    OwnedArtifacts = ownedArtifacts
                };

                return View(model);
            }
            catch (Exception ex)
            {
                string? username = User.Identity?.Name ?? "unknown";
                _logger.LogError(ex, "Error retrieving profile for user {Username}", username);
                return RedirectToAction("Error", "Home");
            }
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Profile(ProfileViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return NotFound();
                }

                user.FirstName = model.FirstName;
                user.LastName = model.LastName;
                user.PhoneNumber = model.PhoneNumber;
                user.Country = model.Country;
                user.Bio = model.Bio;

                if (model.ProfileImage != null)
                {
                    var fileName = $"{Guid.NewGuid()}{Path.GetExtension(model.ProfileImage.FileName)}";
                    var profilesDirectory = Path.Combine(_webHostEnvironment.WebRootPath, "images", "profiles");

                    // Ensure directory exists
                    Directory.CreateDirectory(profilesDirectory);

                    var filePath = Path.Combine(profilesDirectory, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await model.ProfileImage.CopyToAsync(stream);
                    }

                    user.ProfileImageUrl = $"/images/profiles/{fileName}";
                }

                var result = await _userManager.UpdateAsync(user);
                if (result.Succeeded)
                {
                    await _signInManager.RefreshSignInAsync(user);
                    TempData["SuccessMessage"] = "Your profile has been updated.";
                    return RedirectToAction(nameof(Profile));
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }
            catch (Exception ex)
            {
                string? username = User.Identity?.Name ?? "unknown";
                _logger.LogError(ex, "Profile update failed for user {Username}", username);
                ModelState.AddModelError(string.Empty, "An error occurred while processing your request. Please try again later.");
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            try
            {
                await _signInManager.SignOutAsync();
                HttpContext.Session.Clear();
                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                string? username = User.Identity?.Name ?? "unknown";
                _logger.LogError(ex, "Logout failed for user {Username}", username);
                return RedirectToAction("Error", "Home");
            }
        }

        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Lockout()
        {
            return View();
        }

        private IActionResult RedirectToLocal(string? returnUrl)
        {
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction(nameof(HomeController.Index), "Home");
        }
    }
}
