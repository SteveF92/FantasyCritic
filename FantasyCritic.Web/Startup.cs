using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using FantasyCritic.MySQL;
using FantasyCritic.Web.Models;
using FantasyCritic.Web.Services;
using FantasyCritic.Lib.Interfaces;
using FantasyCritic.Lib.Domain;
using FantasyCritic.Lib.Services;

namespace FantasyCritic.Web
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
            string connectionString = Configuration.GetConnectionString("DefaultConnection");

            var identityStore = new MySQLFantasyIdentityStore(connectionString);
            var fantasyCriticRepo = new MySQLFantasyCriticRepo(connectionString);

            services.AddScoped<IFantasyCriticUserStore>(factory => identityStore);
            services.AddScoped<IFantasyCriticRoleStore>(factory => identityStore);
            services.AddScoped<IFantasyCriticRepo>(factory => fantasyCriticRepo);
            services.AddScoped<IUserStore<FantasyCriticUser>>(factory => identityStore);
            services.AddScoped<IRoleStore<FantasyCriticRole>>(factory => identityStore);

            services.AddScoped<FantasyCriticUserManager>();
            services.AddScoped<FantasyCriticUserManager>();

            services.AddIdentity<FantasyCriticUser, FantasyCriticRole>()
                .AddDefaultTokenProviders();

            // Add application services.
            services.AddTransient<IEmailSender, EmailSender>();

            services.AddMvc();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseBrowserLink();
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseStaticFiles();

            app.UseAuthentication();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
