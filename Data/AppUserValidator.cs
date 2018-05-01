using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace ReactSpa.Data
{
    public class AppUserValidator<TUser> : IUserValidator<TUser> where TUser : class
    {
        public async Task<IdentityResult> ValidateAsync(UserManager<TUser> manager, TUser user)
        {
            if (manager == null)
                throw new ArgumentNullException("manager");
            if ((object) user == null)
                throw new ArgumentNullException("user");
            var error = await this.ValidateEmail(manager, user);
            if (error != null)
                return IdentityResult.Failed(error);
            var _user = user as UserInfo;
            return await Task.FromResult(string.IsNullOrWhiteSpace(_user.UserName)
                ? IdentityResult.Failed(new Describer().InvalidBlankName())
                : IdentityResult.Success);
        }

        private async Task<IdentityError> ValidateEmail(UserManager<TUser> manager, TUser user)
        {
            string email = await manager.GetEmailAsync(user);
            if (string.IsNullOrWhiteSpace(email))
            {
                return new Describer().InvalidEmail(email);
            }
            else
            {
                TUser byEmailAsync = await manager.FindByEmailAsync(email);
                bool flag = (object) byEmailAsync != null;
                if (flag)
                {
                    string a = await manager.GetUserIdAsync(byEmailAsync);
                    string userIdAsync = await manager.GetUserIdAsync(user);
                    flag = !string.Equals(a, userIdAsync);
                    a = (string) null;
                }
                if (!flag)
                    return null;
                return new Describer().DuplicateEmail(email);
            }
        }

        public class Describer : IdentityErrorDescriber
        {
            public IdentityError InvalidBlankName()
            {
                return new IdentityError();
            }
        }
    }
}