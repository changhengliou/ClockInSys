using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ReactSpa.Data;

namespace ReactSpa.Extension
{
    public static class IdentityExtension
    {
        public static IServiceCollection AddCustomIdentity<TUser, TRole>(this IServiceCollection services)
            where TUser : class
            where TRole : class
        {
            services.AddAuthentication(
                options => options.SignInScheme = new IdentityCookieOptions()
                    .ExternalCookieAuthenticationScheme);
            services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.TryAddSingleton<IdentityMarkerService>();
            services.TryAddSingleton<IUserValidator<TUser>, AppUserValidator<TUser>>();
            services.TryAddScoped<IPasswordValidator<TUser>, PasswordValidator<TUser>>();
            services.TryAddScoped<IPasswordHasher<TUser>, PasswordHasher<TUser>>();
            services.TryAddScoped<ILookupNormalizer, UpperInvariantLookupNormalizer>();
            services.TryAddScoped<IRoleValidator<TRole>, RoleValidator<TRole>>();
            services.TryAddScoped<IdentityErrorDescriber>();
            services.TryAddScoped<ISecurityStampValidator, SecurityStampValidator<TUser>>();
            services.TryAddScoped<IUserClaimsPrincipalFactory<TUser>,
                UserClaimsPrincipalFactory<TUser, TRole>>();
            services.TryAddScoped<UserManager<TUser>, UserManager<TUser>>();
            services.TryAddScoped<SignInManager<TUser>, SignInManager<TUser>>();
            services.TryAddScoped<RoleManager<TRole>, RoleManager<TRole>>();
            return services;
        }

        public static void AddEfStores<TUser, TRole, TContext>(this IServiceCollection services)
            where TUser : class
            where TRole : class where TContext : DbContext
        {
            IdentityBuilder builder = new IdentityBuilder(typeof(TUser), typeof(TRole), services);
            builder.AddEntityFrameworkStores<TContext>().AddDefaultTokenProviders();
        }
    }
}