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
            var user = await _userManager.FindByLoginAsync(info.LoginProvider, info.ProviderKey);
            if (user != null)
            {
                if (await _userManager.IsInRoleAsync(user, "inactive"))
                {
                    await _signInManager.SignOutAsync();
                    return View("InActiveAccount", user);
                } 
                await _signInManager.SignInAsync(user, isPersistent: false, authenticationMethod: info.LoginProvider);
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
            if (userInfo != null)
            {
                var addLoginResult = await _userManager.AddLoginAsync(userInfo, info);
                if (addLoginResult.Succeeded)
                {
                    var claimResult = await _userManager.AddClaimsAsync(userInfo,
                        new Claim[]
                        {
                            new Claim(ClaimTypes.Email, userInfo.Email), new Claim(ClaimTypes.Name, userInfo.UserName)
                        });
                    if (claimResult.Succeeded)
                    {
                        if (await _userManager.IsInRoleAsync(userInfo, "inactive"))
                        {
                            await _signInManager.SignOutAsync();
                            return View("InActiveAccount", userInfo);
                        }
                        await _signInManager.SignInAsync(userInfo, isPersistent: false,
                            authenticationMethod: info.LoginProvider);
                        return RedirectToAction("Index", "Home");
                    }
                }
                return RedirectToAction(nameof(Login));
            }
            return View("NoAccount", email);
        }

        public async Task<ActionResult> SignOut()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Login");
        }
    }
}