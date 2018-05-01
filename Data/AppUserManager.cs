using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ReactSpa.Data
{
    public class SignInManager : SignInManager<UserInfo>
    {
        public SignInManager(UserManager<UserInfo> userManager, IHttpContextAccessor contextAccessor, IUserClaimsPrincipalFactory<UserInfo> claimsFactory, IOptions<IdentityOptions> optionsAccessor, ILogger<SignInManager<UserInfo>> logger) : base(userManager, contextAccessor, claimsFactory, optionsAccessor, logger)
        {
        }

        public override async Task SignInAsync(UserInfo user, AuthenticationProperties authenticationProperties, string authenticationMethod = null)
        {
            var roles = await this.UserManager.GetRolesAsync(user);

            var claims = new List<Claim> {
                new Claim(ClaimTypes.Name, user.UserName, ClaimValueTypes.String),
                new Claim(ClaimTypes.Email, user.Email, ClaimValueTypes.String),
                new Claim(ClaimTypes.NameIdentifier, user.Id, ClaimValueTypes.String),
            };

            foreach (var role in roles)
                claims.Add(new Claim(ClaimTypes.Role, role, ClaimValueTypes.String));

            if (roles.Contains("inactive"))
                return;

            var userPrincipal = new ClaimsPrincipal(new ClaimsIdentity(claims, Options.Cookies.ApplicationCookieAuthenticationScheme));
            
            if (authenticationMethod != null)
                userPrincipal.Identities.First<ClaimsIdentity>().AddClaim(new Claim("http://schemas.microsoft.com/ws/2008/06/identity/claims/authenticationmethod", authenticationMethod));
            await Context.Authentication.SignInAsync(Options.Cookies.ApplicationCookieAuthenticationScheme, userPrincipal, authenticationProperties ?? new AuthenticationProperties());
        }
    }

    public class UserManager : UserManager<UserInfo>
    {
        public UserManager(IUserStore<UserInfo> store, IOptions<IdentityOptions> optionsAccessor, IPasswordHasher<UserInfo> passwordHasher, IEnumerable<IUserValidator<UserInfo>> userValidators, IEnumerable<IPasswordValidator<UserInfo>> passwordValidators, ILookupNormalizer keyNormalizer, IdentityErrorDescriber errors, IServiceProvider services, ILogger<UserManager<UserInfo>> logger) : base(store, optionsAccessor, passwordHasher, userValidators, passwordValidators, keyNormalizer, errors, services, logger)
        {
        }
    }
}
