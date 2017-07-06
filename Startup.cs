using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hangfire;
using Hangfire.Annotations;
using Hangfire.Dashboard;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SpaServices.Webpack;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ReactSpa.Data;
using ReactSpa.Extension;

namespace ReactSpa
{
    public class Startup
    {
        public IConfigurationRoot Configuration { get; }

        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<ConnectionInfo>(Configuration.GetSection("ConnectionStrings"));

            services.AddDbContext<AppDbContext>(
                options => options.UseSqlServer(Configuration.GetConnectionString("LocalSQLServer")));

            services.AddCustomIdentity<UserInfo, IdentityRole>().AddEfStores<UserInfo, IdentityRole, AppDbContext>();

//            services.AddIdentity<UserInfo, IdentityRole>()
//                .AddEntityFrameworkStores<AppDbContext>()
//                .AddDefaultTokenProviders();

            services.Configure<IdentityOptions>(options =>
            {
                options.User.RequireUniqueEmail = true;
                options.Cookies.ApplicationCookie.ExpireTimeSpan = TimeSpan.FromHours(1);
                options.Cookies.ApplicationCookie.LoginPath = "/account/login";
                options.Cookies.ApplicationCookie.LogoutPath = "/account/logout";
            });

            services.AddHangfire(config =>
                config.UseSqlServerStorage(Configuration.GetConnectionString("LocalSQLServer")));
            // Add framework services.
            services.AddMvc(options =>
            {
                options.SslPort = 44305;
                options.Filters.Add(new RequireHttpsAttribute());
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
                app.UseWebpackDevMiddleware(new WebpackDevMiddlewareOptions
                {
                    HotModuleReplacement = true,
                    ReactHotModuleReplacement = true
                });
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseStaticFiles();

            app.UseIdentity();

//            app.UseForwardedHeaders(new ForwardedHeadersOptions
//            {
//                ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
//            });

            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AuthenticationScheme = "External",
                CookieHttpOnly = true
            });

            app.UseGoogleAuthentication(new GoogleOptions
            {
                ClientId = "801390402609-u1p4juuc8432gaqd0gr80o2i1v5fcbsh.apps.googleusercontent.com",
                ClientSecret = "-m9m9iP2q0SfDi1AtL_Zx7ku",
                AuthenticationScheme = "Google",
                CallbackPath = "/account/callback-google"
            });

            // schedule task
            app.UseHangfireServer();
            RecurringJob.AddOrUpdate(() => RegisterScheduleTask(), Cron.Daily);
            app.UseHangfireDashboard("/hangfire", new DashboardOptions
            {
                Authorization = new[] {new CustomAuthorizeFilter()}
            });

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");

                routes.MapSpaFallbackRoute(
                    name: "spa-fallback",
                    defaults: new {controller = "Home", action = "Index"});
            });
            InitDatabaseHelper.SeedRoles(app.ApplicationServices).Wait();
        }

        public void RegisterScheduleTask()
        {
            DbContextOptionsBuilder<AppDbContext> builder = new DbContextOptionsBuilder<AppDbContext>();
            builder.UseSqlServer(Configuration.GetConnectionString("LocalSQLServer"));
            using (var dbContext = new AppDbContext(builder.Options))
            {
                dbContext.ChangeTracker.AutoDetectChangesEnabled = false;
                var users = dbContext.UserInfo.Select(s => s).ToList();

                var today = DateTime.Today;

                users.ForEach(s =>
                {
                    var date = s.DateOfEmployment;
                    if (date == null)
                        return;
                    if ((date.Value.Day == today.Day && date.Value.Month == today.Month) ||
                        (date.Value.Day == 29 && date.Value.Month == 2 && today.Day == 28 && today.Month == 2))
                    {
                        var obj = dbContext.RemainingDayOff.FirstOrDefault(
                            z => z.UserId == s.Id && z.Year == (today.Year - 1).ToString());
                        if (obj == null)
                        {
                            dbContext.RemainingDayOff.Add(new RemainingDayOff
                            {
                                Id = Guid.NewGuid().ToString(),
                                AnnualLeaves = s.AnnualLeaves,
                                FamilyCareLeaves = 0,
                                SickLeaves = 0,
                                UserId = s.Id,
                                Year = (today.Year - 1).ToString()
                            });
                        }
                        else
                        {
                            obj.AnnualLeaves = s.AnnualLeaves;
                            dbContext.Entry(obj).State = EntityState.Modified;
                        }
                        s.AnnualLeaves = GetNumberOfAnnualLeaves(s.DateOfEmployment);
                        dbContext.Entry(s).State = EntityState.Modified;
                    }
                    if (today.Day == 1 && today.Month == 1)
                    {
                        var obj = dbContext.RemainingDayOff.FirstOrDefault(
                            z => z.UserId == s.Id && z.Year == (today.Year - 1).ToString());
                        if (obj == null)
                        {
                            dbContext.RemainingDayOff.Add(new RemainingDayOff
                            {
                                Id = Guid.NewGuid().ToString(),
                                AnnualLeaves = 0,
                                FamilyCareLeaves = s.FamilyCareLeaves,
                                SickLeaves = s.SickLeaves,
                                UserId = s.Id,
                                Year = (today.Year - 1).ToString()
                            });
                        }
                        else
                        {
                            obj.FamilyCareLeaves = s.FamilyCareLeaves;
                            obj.SickLeaves = s.SickLeaves;
                            dbContext.Entry(obj).State = EntityState.Modified;
                        }
                        s.FamilyCareLeaves = 7;
                        s.SickLeaves = 30;
                        dbContext.Entry(s).State = EntityState.Modified;
                    }
                });
                dbContext.SaveChanges();
            }
        }

        public int GetNumberOfAnnualLeaves(DateTime? _date)
        {
            if (_date == null)
                return 0;
            DateTime date = _date.Value;
            DateTime today = DateTime.Today;
            var diff = today.Subtract(date).Days / (365.25 / 12);
            if (diff >= 6 && diff < 12)
                return 3;
            if (diff >= 12 && diff < 24)
                return 7;
            if (diff >= 24 && diff < 36)
                return 10;
            if (diff >= 36 && diff < 60)
                return 14;
            if (diff >= 60 && diff < 120)
                return 15;
            if (diff >= 120)
            {
                int result = 15 + (int) ((diff - 108) / 12);
                if (result > 30)
                    return 30;
                return result;
            }
            return 0;
        }
    }

    public static class InitDatabaseHelper
    {
        private static readonly string[] Roles = {"boss", "admin", "manager", "default", "inactive"};

        public static async Task SeedRoles(IServiceProvider serviceProvider)
        {
            using (var serviceScope = serviceProvider.GetRequiredService<IServiceScopeFactory>().CreateScope())
            {
                var dbContext = serviceScope.ServiceProvider.GetService<AppDbContext>();

                if (dbContext.Database.GetPendingMigrations().Any())
                {
                    await dbContext.Database.MigrateAsync();

                    var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

                    foreach (var role in Roles)
                    {
                        if (!await roleManager.RoleExistsAsync(role))
                        {
                            await roleManager.CreateAsync(new IdentityRole(role));
                        }
                    }
                }
            }
        }
    }

    public class CustomAuthorizeFilter : IDashboardAuthorizationFilter
    {
        public bool Authorize([NotNull] DashboardContext context)
        {
            var httpcontext = context.GetHttpContext();
            if (httpcontext.User.Identity.IsAuthenticated && httpcontext.User.IsInRole("admin"))
                return true;
            return false;
        }
    }

    public class ConnectionInfo
    {
        public string LocalSQLServer { get; set; }
        public string Azure { get; set; }
    }
}