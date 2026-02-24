 
using IPS_PROJECT.Data;
using IPS_PROJECT.Models;
using IPS_PROJECT.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.WebUtilities;
using System.Text;
using System.Text.Encodings.Web;
 

namespace IPS_PROJECT.Controllers
{
     
    public class Account : Controller
    {
        private readonly SignInManager<USERS> _signInManager;
        private readonly UserManager<USERS> _userManager;
        private readonly IEmailSender _emailSender;

        public Account(UserManager<USERS> userManager, SignInManager<USERS> signInManager, IEmailSender emailSender)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _emailSender = emailSender;
        }

        // GET: /Account/Login
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login()
        {
            return View();
        }

        // POST: /Account/Login
        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            if (string.IsNullOrEmpty(model.Email))
            {
                ModelState.AddModelError("", "Email is required.");
                return View(model);
            }

            if (string.IsNullOrEmpty(model.Password))
            {
                ModelState.AddModelError("", "Password is required.");
                return View(model);
            }

            if (string.IsNullOrEmpty(model.Role))
            {
                ModelState.AddModelError("", "Role is required.");
                return View(model);
            }

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                ModelState.AddModelError("", "Invalid login attempt");
                return View(model);
            }

            // تحقق من الدور
            if (!await _userManager.IsInRoleAsync(user, model.Role))
            {
                ModelState.AddModelError("", $"User does not have {model.Role} role");
                return View(model);
            }

            var result = await _signInManager.PasswordSignInAsync(user, model.Password, false, false);

            if (result.Succeeded)
            {
                if (model.Role == "Admin")
                    return RedirectToAction("Index", "DashBoard");

                return RedirectToAction("Index", "UserDashboard");
            }

            if (result.IsNotAllowed)
            {
                ModelState.AddModelError("", "Please confirm your email before logging in.");
                return View(model);
            }

            ModelState.AddModelError("", "Invalid login attempt");
            return View(model);
        }



        // GET: /Account/Register

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Register()
        {
            return View();
        }


        // POST: /Account/Register

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            if (model.Password != model.ConfirmPassword)
            {
                ModelState.AddModelError("", "Passwords do not match");
                return View(model);
            }

            var user = new USERS
            {
                UserName = model.Email,
                Email = model.Email,
                FullName = model.FullName
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                var usersCount = await _userManager.Users.CountAsync();

                if (usersCount == 1)
                {
                    // أول يوزر يبقى Admin
                    await _userManager.AddToRoleAsync(user, "Admin");
                }
                else
                {
                    // باقي اليوزرات يبقوا Users
                    await _userManager.AddToRoleAsync(user, "User");
                }


                // إرسال إيميل التفعيل
                await SendConfirmationEmail(user);

                return RedirectToAction("VerifyEmail");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }

            return View(model);
        }


        private async Task SendConfirmationEmail(USERS user)
        {
            var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

            var callbackUrl = Url.Action(
                "ConfirmEmail",
                "Account",
                new { userId = user.Id, code = code },
                protocol: Request.Scheme);

            if (string.IsNullOrEmpty(user.Email))
            {
                throw new InvalidOperationException("User email is missing, cannot send confirmation email.");
            }

            if (callbackUrl == null)
            {
                throw new InvalidOperationException("Callback URL is null, cannot send email.");
            }


            await _emailSender.SendEmailAsync(
                user.Email,
                "Confirm your email",
                $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");
        }

        // صفحة استقبال التفعيل (مسموحة للجميع)
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> ConfirmEmail(string userId, string code)
        {
            if (userId == null || code == null)
                return RedirectToAction("Login");

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return NotFound();

            code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code));
            var result = await _userManager.ConfirmEmailAsync(user, code);

            if (result.Succeeded)
            {
                // تسجيل الدخول تلقائياً
                await _signInManager.SignInAsync(user, isPersistent: false);

                // تحويل حسب الدور
                if (await _userManager.IsInRoleAsync(user, "Admin"))
                    return RedirectToAction("Index", "DashBoard");
                else
                    return RedirectToAction("Index", "UserDashboard");
            }

            return View("Error");
        }



        // verify email / get

        [HttpGet]
        [AllowAnonymous]
        public IActionResult VerifyEmail()
        {
            return View();
        }


        // resend verification 

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> ResendConfirmationEmail()
        {
            // جلب اليوزر الحالي
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("Login");

            
           
            // ارسال ايميل التفعيل
            await SendConfirmationEmail(user);

            TempData["StatusMessage"] = "Verification email resent. Please check your inbox.";

            return RedirectToAction("VerifyEmail");
        }



    }
}
