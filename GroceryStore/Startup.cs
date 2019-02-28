using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GroceryStore.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using GroceryStore.Models;
using Newtonsoft.Json;
using Microsoft.Extensions.Logging;
using GroceryStore.Areas.Identity.Pages.Account;
using GroceryStore.Services;

namespace GroceryStore
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            services.Configure<IdentityOptions>(options =>
            {
                // Password options for each user
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;

                // User options for each user
                options.User.RequireUniqueEmail = true;
            });

            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(
                    Configuration.GetConnectionString("AccountConnection")));

            // inject custom class for common db functionality 
            // -----------------------------------------------------------
            DbContextOptionsBuilder<ApplicationDbContext> optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
            optionsBuilder.UseSqlServer(Configuration.GetConnectionString("AccountConnection"));

            services.AddSingleton(new DbCommonFunctionality(optionsBuilder.Options));
            // -----------------------------------------------------------

            // Use add identity instead and added add default ui with add default token providers due to bug in 
            // asp.net core 2.1 with checking what role current logged in user belongs to
            services.AddIdentity<ApplicationUser, ApplicationRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultUI()
                .AddDefaultTokenProviders();

            services.AddDbContext<GroceryStoreContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("GroceryStoreConnection")));

            // if admin role name changes then changes to role will take affect in authroizing roles
            services.AddAuthorization(options =>
            {
                options.AddPolicy("Admin", policy =>
                    policy.RequireRole(Configuration.GetSection("AdminRole").Value));
            });

            services.AddMvc()
                .AddJsonOptions(options => options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore)
                .SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

            services.AddDistributedMemoryCache();   // required for session variables
            services.AddSession();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ApplicationDbContext context,
            RoleManager<ApplicationRole> roleManager, UserManager<ApplicationUser> userManager, ILogger<RegisterModel> logger, 
            DbCommonFunctionality dbCommonFunctionality)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseCookiePolicy();

            app.UseAuthentication();

            app.UseSession();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });

            SeedData.Initialize(context, userManager, roleManager, Configuration, logger, dbCommonFunctionality).Wait();
        }
    }
}
