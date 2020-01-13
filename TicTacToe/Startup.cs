using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using TicTacToe.Services;
using TicTacToe.Extensions;
using Microsoft.AspNetCore.Routing;
using TicTacToe.Models;
using System.Globalization;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.Configuration;
using TicTacToe.Options;
using TicTacToe.Filters;
using TicTacToe.ViewEngines;
using Microsoft.AspNetCore.Mvc.Formatters;
using Halcyon.Web.HAL.Json;
using TicTacToe.Data;
using Microsoft.EntityFrameworkCore;

namespace TicTacToe
{
    public class Startup
    {
        public IConfiguration _configuration { get; }
        public IHostingEnvironment _hostingEnvironment { get; }
        public Startup(IConfiguration configuration, IHostingEnvironment hostingEnvironment)
        {
            _configuration = configuration;
            _hostingEnvironment = hostingEnvironment;
        }

        public void ConfigureCommonServices(IServiceCollection services)
        {
            services.AddLocalization(options => options.ResourcesPath = "Localization"); // wskazanie że plik z językami znajdują się w folderze Localization
            services.AddMvc(o =>
            {
				//Gets a collection of IFilterMetadata which are used to construct filters that apply to all actions.
                o.Filters.Add(typeof(DetectMobileFilter));


				o.OutputFormatters.RemoveType<JsonOutputFormatter>(); // Removes all formaters of the specifed type
                o.OutputFormatters.Add(new JsonHalOutputFormatter(new string[] { "application/hal+json", "application/vnd.example.hal+json", "application/vnd.example.hal.v1+json" })); // dodanie formatowanie z Halcyon
																																														// HAL is a simple format that gives a consistent and easy way to hyperlink between resources in a RESTful API.
			}).AddViewLocalization(LanguageViewLocationExpanderFormat.Suffix, options => options.ResourcesPath = "Localization").AddDataAnnotationsLocalization(); //	Adds MVC view localization services to the application.	



			services.AddSingleton<IUserService, UserService>(); // zarejstrowanie usługi jako SIngleton
            services.AddSingleton<IGameInvitationService, GameInvitationService>(); // zarejstrowanie usługi jako Singleton
            services.AddSingleton<IGameSessionService, GameSessionService>(); // zarejstrowanie usługi jako SIngleton

            var connectionString = _configuration.GetConnectionString("DefaultConnection"); // pobranie conncetionString
            services.AddEntityFrameworkSqlServer()
                .AddDbContext<GameDbContext>((serviceProvider, options) =>
                    options.UseSqlServer(connectionString)
                            .UseInternalServiceProvider(serviceProvider)
                            );

            var dbContextOptionsbuilder = new DbContextOptionsBuilder<GameDbContext>()
                .UseSqlServer(connectionString); // uzycie bazy dannych konkretnego connection string'a

            services.AddSingleton(dbContextOptionsbuilder.Options); // rejestracja bazy bannych jako Singleton

            services.Configure<EmailServiceOptions>(_configuration.GetSection("Email")); // Rejestruje wystąpienie konfiguracji, dla którego zostanie powiązana TOptions
			services.AddEmailService(_hostingEnvironment, _configuration); //dodanie EmialService folder Extentions/EmailServiceEx...
            services.AddTransient<IEmailTemplateRenderService, EmailTemplateRenderService>(); // zarejestrowanie usługi jako Transient czyli ulotnej
            services.AddTransient<EmailViewEngine, EmailViewEngine>(); // dodanie ViewEngine jako Transient

            services.AddRouting(); // dodanie podstawowego adresowania
            services.AddSession(o =>                         //Adds services required for application session state.
			{
                o.IdleTimeout = TimeSpan.FromMinutes(30); //The IdleTimeout indicates how long the session can be idle before its contents are abandoned. Each session access resets the timeout. Note this only applies to the content of the session, not the cookie.
			});
        }

        public void ConfigureDevelopmentServices(IServiceCollection services)
        {
            ConfigureCommonServices(services);
        }

        public void ConfigureStagingServices(IServiceCollection services)
        {
            ConfigureCommonServices(services);
        }

        public void ConfigureProductionServices(IServiceCollection services)
        {
            ConfigureCommonServices(services);
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseBrowserLink();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseStaticFiles();
            app.UseSession();

            var routeBuilder = new RouteBuilder(app);
            routeBuilder.MapGet("CreateUser", context =>
            {
                var firstName = context.Request.Query["firstName"];
                var lastName = context.Request.Query["lastName"];
                var email = context.Request.Query["email"];
                var password = context.Request.Query["password"];
                var userService = context.RequestServices.GetService<IUserService>();
                userService.RegisterUser(new UserModel { FirstName = firstName, LastName = lastName, Email = email, Password = password });
                return context.Response.WriteAsync($"Uzytkownik {firstName} {lastName} zostal pomyslnie utworzony.");
            });
            var newUserRoutes = routeBuilder.Build();
            app.UseRouter(newUserRoutes);

            app.UseWebSockets();
            app.UseCommunicationMiddleware();

            var supportedCultures = CultureInfo.GetCultures(CultureTypes.AllCultures);
            var localizationOptions = new RequestLocalizationOptions
            {
                DefaultRequestCulture = new RequestCulture("pl-PL"),
                SupportedCultures = supportedCultures,
                SupportedUICultures = supportedCultures
            };

            localizationOptions.RequestCultureProviders.Clear();
            localizationOptions.RequestCultureProviders.Add(new CultureProviderResolverService());

            app.UseRequestLocalization(localizationOptions);

            app.UseMvc(routes =>
            {
                routes.MapRoute(name: "areaRoute",
                        template: "{area:exists}/{controller=Home}/{action=Index}");

                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });

            app.UseStatusCodePages("text/plain", "Blad HTTP - kod odpowiedzi: {0}");

            using (var scope = app.ApplicationServices.GetService<IServiceScopeFactory>().CreateScope())
            {
                scope.ServiceProvider.GetRequiredService<GameDbContext>().Database.Migrate();
            }
        }
    }
}
