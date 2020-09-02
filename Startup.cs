using System;
using System.IdentityModel.Tokens.Jwt;
using System.Threading.Tasks;
using System.Transactions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SpaServices.AngularCli;
using Microsoft.EntityFrameworkCore;
using Judge1.Data;
using Judge1.Models;
using Judge1.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Hangfire;
using Hangfire.MySql;
using Judge1.Notifications;
using Judge1.Services.Admin;
using Judge1.Services.Judge;
using Microsoft.Extensions.Logging;

namespace Judge1
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;

            // See https://github.com/dotnet/aspnetcore/issues/14160
            // Also https://github.com/AzureAD/azure-activedirectory-identitymodel-extensions-for-dotnet/issues/415
            // JwtSecurityTokenHandler.DefaultInboundClaimTypeMap = new Dictionary<string, string>();
            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Remove(JwtRegisteredClaimNames.Sub);
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseMySql(Configuration.GetConnectionString("MySqlConnection")));

            services.AddDefaultIdentity<ApplicationUser>(options =>
                {
                    options.SignIn.RequireConfirmedEmail = false;
                    options.Password.RequireDigit = false;
                    options.Password.RequireLowercase = false;
                    options.Password.RequireUppercase = false;
                    options.Password.RequireNonAlphanumeric = false;
                })
                .AddRoles<IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>();

            services.AddIdentityServer()
                .AddApiAuthorization<ApplicationUser, ApplicationDbContext>()
                .AddProfileService<ProfileService>();

            services.AddHangfire(configuration => configuration
                .SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
                .UseSimpleAssemblyNameTypeSerializer()
                .UseRecommendedSerializerSettings()
                .UseStorage(new MySqlStorage(Configuration.GetConnectionString("HangfireConnection"),
                    new MySqlStorageOptions
                    {
                        TransactionIsolationLevel = IsolationLevel.ReadCommitted,
                        QueuePollInterval = TimeSpan.FromSeconds(15),
                        JobExpirationCheckInterval = TimeSpan.FromHours(1),
                        CountersAggregateInterval = TimeSpan.FromMinutes(5),
                        PrepareSchemaIfNecessary = true,
                        DashboardJobListLimit = 50000,
                        TransactionTimeout = TimeSpan.FromMinutes(1),
                        TablesPrefix = "Hangfire"
                    })
                )
            );
            services.AddHangfireServer();

            // See https://stackoverflow.com/questions/52526186/net-core-identity
            // and https://stackoverflow.com/questions/60184703/net-core-3-1-403.
            services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = IdentityConstants.ApplicationScheme;
                    options.DefaultChallengeScheme = IdentityConstants.ApplicationScheme;
                    options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
                })
                .AddIdentityServerJwt();
            services.AddAuthorization(options =>
            {
                options.AddPolicy("ManageUsers",
                    policy => { policy.RequireRole(ApplicationRoles.Administrator, ApplicationRoles.UserManager); });
                options.AddPolicy("ManageContests",
                    policy => { policy.RequireRole(ApplicationRoles.Administrator, ApplicationRoles.ContestManager); });
                options.AddPolicy("ManageProblems",
                    policy => { policy.RequireRole(ApplicationRoles.Administrator, ApplicationRoles.ContestManager); });
                options.AddPolicy("ManageSubmissions",
                    policy =>
                    {
                        policy.RequireRole(ApplicationRoles.Administrator, ApplicationRoles.SubmissionManager);
                    });
            });
            services.AddControllersWithViews().AddNewtonsoftJson();
            services.AddRazorPages();

            services.AddOptions();
            services.Configure<ApplicationConfig>(Configuration.GetSection("Application"));
            services.Configure<JudgingConfig>(Configuration.GetSection("Judging"));
            services.Configure<NotificationConfig>(Configuration.GetSection("Notification"));

            services.AddHttpClient(); // IHttpClientFactory

            services.AddScoped<IContestService, ContestService>();
            services.AddScoped<IProblemService, ProblemService>();
            services.AddScoped<ISubmissionService, SubmissionService>();

            services.AddScoped<IAdminUserService, AdminUserService>();
            services.AddScoped<IAdminContestService, AdminContestService>();
            services.AddScoped<IAdminProblemService, AdminProblemService>();
            services.AddScoped<IAdminSubmissionService, AdminSubmissionService>();

            services.AddScoped<IContestJudgeService, ContestJudgeService>();

            services.AddScoped<IDingTalkNotification, DingTalkNotification>();
            services.AddScoped<INotificationBroadcaster, NotificationBroadcaster>();

            // In production, the Angular files will be served from this directory
            services.AddSpaStaticFiles(configuration => { configuration.RootPath = "ClientApp/dist"; });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IServiceProvider provider)
        {
            ConfigureDatabase(provider).Wait();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            if (!env.IsDevelopment())
            {
                app.UseSpaStaticFiles();
            }

            app.UseRouting();

            app.UseAuthentication();
            app.UseIdentityServer();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller}/{action=Index}/{id?}");
                endpoints.MapRazorPages();
                // TODO: add authorization and allow remote access, see link below for instructions
                // https://docs.hangfire.io/en/latest/configuration/using-dashboard.html#configuring-authorization
                endpoints.MapHangfireDashboard();
            });

            app.UseSpa(spa =>
            {
                // To learn more about options for serving an Angular SPA from ASP.NET Core,
                // see https://go.microsoft.com/fwlink/?linkid=864501

                spa.Options.SourcePath = "ClientApp";

                if (env.IsDevelopment())
                {
                    // Check https://github.com/dotnet/aspnetcore/issues/17277
                    // spa.UseProxyToSpaDevelopmentServer("http://localhost:4200");
                    spa.UseAngularCliServer(npmScript: "start:dotnet");
                }
            });
        }

        private async Task ConfigureDatabase(IServiceProvider provider)
        {
            var logger = provider.GetService<ILogger<Startup>>();
            var context = provider.GetService<ApplicationDbContext>();
            await context.Database.MigrateAsync();

            var roleManager = provider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = provider.GetRequiredService<UserManager<ApplicationUser>>();

            foreach (var role in ApplicationRoles.RoleList)
            {
                var exists = await roleManager.RoleExistsAsync(role);
                if (!exists)
                {
                    logger.LogInformation($"Creating role {role}.");
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            if ((await userManager.FindByEmailAsync(Configuration["Application:AdminUser:Email"].ToUpper())) == null)
            {
                logger.LogInformation("Creating admin user.");
                var adminUser = new ApplicationUser()
                {
                    Email = Configuration["Application:AdminUser:Email"],
                    UserName = Configuration["Application:AdminUser:Email"],
                    ContestantId = Configuration["Application:AdminUser:ContestantId"],
                    ContestantName = Configuration["Application:AdminUser:ContestantName"]
                };
                var password = Configuration["Application:AdminUser:Password"];
                var result = await userManager.CreateAsync(adminUser, password);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, ApplicationRoles.Administrator);
                }
            }

            logger.LogInformation("Database configured successfully");
        }
    }
}