using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ReactSpa.Data;

namespace ReactSpa.Controllers
{
    [Route("[controller]/[action]")]
    public class AccountController : Controller
    {
        private readonly UserManager<UserInfo> _userManager;
        private readonly SignInManager<UserInfo> _signInManager;
        private readonly ILogger _logger;
        private readonly string _externalCookieScheme;
        private readonly UserInfoManager _userInfoManager;

        public AccountController(
            UserManager<UserInfo> userManager,
            SignInManager<UserInfo> signInManager,
            IOptions<IdentityCookieOptions> identityCookieOptions,
            ILoggerFactory loggerFactory,
            IOptions<ConnectionInfo> config)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _externalCookieScheme = identityCookieOptions.Value.ExternalCookieAuthenticationScheme;
            _logger = loggerFactory.CreateLogger<AccountController>();
            _userInfoManager = new UserInfoManager(config);
        }

        //
        // GET: /Account/Login
        [AllowAnonymous]
        public async Task<IActionResult> Login(string returnUrl = null)
        {
            // Clear the existing external cookie to ensure a clean login process
            await HttpContext.Authentication.SignOutAsync(_externalCookieScheme);

            ViewData["ReturnUrl"] = returnUrl;
            return View("Login");
        }

        //
        // POST: /Account/ExternalLogin
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public IActionResult ExternalLogin(string provider, string returnUrl = null)
        {
            // Request a redirect to the external login provider.
            var redirectUrl = Url.Action(nameof(ExternalLoginCallback), "Account", new {ReturnUrl = returnUrl});
            var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
            return Challenge(properties, provider);
        }

        //
        // GET: /Account/ExternalLoginCallback
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> ExternalLoginCallback(string returnUrl = null, string remoteError = null)
        {
            if (remoteError != null)
            {
                ModelState.AddModelError(string.Empty, $"Error from external provider: {remoteError}");
                return View(nameof(Login));
            }
            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                return RedirectToAction(nameof(Login));
            }

            // Sign in the user with this external login provider if the user already has a login.
            var result = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey,
                isPersistent: false);
            if (result.Succeeded)
            {
                _logger.LogInformation(5, "User logged in with {Name} provider.", info.LoginProvider);
                Debug.WriteLine($"auth = {User.Identity.IsAuthenticated}");
                Debug.WriteLine($"id = {User.FindFirstValue(ClaimTypes.NameIdentifier)}");
                Debug.WriteLine($"email = {User.FindFirstValue(ClaimTypes.Email)}");
                Debug.WriteLine($"name = {User.FindFirstValue(ClaimTypes.Name)}");
                return RedirectToAction("Index", "Home");
            }

            // if no external login login, but email is already existed, create an external login
            var email = info.Principal.FindFirstValue(ClaimTypes.Email);
            var userInfo = await _userInfoManager.FindUserByEmailAsync(email);
            Debug.WriteLine($"Email = {email}, UserInfo = {userInfo}");
            if (userInfo != null)
            {
                var addLoginResult = await _userManager.AddLoginAsync(userInfo, info);
                if (addLoginResult.Succeeded)
                {
                    var claimResult = await _userManager.AddClaimsAsync(userInfo,
                        new Claim[]
                            {new Claim(ClaimTypes.Email, userInfo.Email), new Claim(ClaimTypes.Name, userInfo.UserName)});
                    var addRoleResult = await _userManager.AddToRoleAsync(userInfo, "default");
                    if (claimResult.Succeeded && addRoleResult.Succeeded)
                    {
                        var signInReault = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider,
                            info.ProviderKey,
                            isPersistent: false);
                        if (signInReault.Succeeded)
                            return RedirectToAction("Index", "Home");
                    }
                }
                return RedirectToAction(nameof(Login));
            }
            // If the user does not have an account, then create an account for user.
            ViewData["ReturnUrl"] = returnUrl;
            ViewData["LoginProvider"] = info.LoginProvider;

            var user = new UserInfo
            {
                Email = email,
                UserName = info.Principal.FindFirstValue(ClaimTypes.Name)
            };
            var registerResult = await _userInfoManager.CreateUserAsync(user);
            if (registerResult.Succeeded)
            {
                var loginInfoResult = await _userManager.AddLoginAsync(user, info);
                if (loginInfoResult.Succeeded)
                {
                    var claimResult = await _userManager.AddClaimsAsync(user,
                        new Claim[]
                            {new Claim(ClaimTypes.Email, user.Email), new Claim(ClaimTypes.Name, user.UserName)});
                    var addRoleResult = await _userManager.AddToRoleAsync(user, "default");
                    if (claimResult.Succeeded && addRoleResult.Succeeded)
                    {
                        var signInReault = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider,
                            info.ProviderKey,
                            isPersistent: false);
                        if (signInReault.Succeeded)
                            return RedirectToAction("Index", "Home");
                    }
                }
            }
            return RedirectToAction(nameof(Login));
        }

        public async Task<ActionResult> SignOut()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Login");
        }
    }
}