using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ABCCompanyService.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using ABCCompanyService.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication.Google;
using Google.Apis.Calendar.v3;
using System.IO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using System.Diagnostics;
using Microsoft.Extensions.PlatformAbstractions;

namespace ABCCompanyService
{
    public class ProcessingTimeMiddleware
    {
        private readonly RequestDelegate _next;

        public ProcessingTimeMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            if (context.Request.Path.Equals("/Account/AccessDenied") || context.Request.Path.Equals("/Account/LogOff") || context.Request.Path.Equals("/Account/LogIn"))
            {
                context.Response.StatusCode = 401;
                return;
            }
            else
                await _next.Invoke(context);
        }
    }

    public static class MiddlewareExtensions
    {
        public static IApplicationBuilder UseProcessingTimeMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<ProcessingTimeMiddleware>();
        }
    }



    public class Startup
    {
        string root;
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();
           
            if (env.IsDevelopment())
            {
                // For more details on using the user secret store see http://go.microsoft.com/fwlink/?LinkID=532709
                builder.AddUserSecrets();
            }

            builder.AddEnvironmentVariables();

            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }


        private string GetXmlCommentsPath()
        {
            var app = PlatformServices.Default.Application;
            return System.IO.Path.Combine(app.ApplicationBasePath, "WebApplication2.xml");
        }
        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<ApplicationDbContext>(options => options.UseSqlite("Filename = ApplicationDbContext.db"));
            services.AddIdentity<ApplicationUser, IdentityRole>()
           .AddEntityFrameworkStores<ApplicationDbContext>()
           .AddDefaultTokenProviders();

            services.AddSingleton<IConfiguration>(Configuration);

            // Add framework services.
            services.AddMvc();

            // Inject an implementation of ISwaggerProvider with defaulted settings applied
            services.AddSwaggerGen(options =>
            {
                options.DescribeAllEnumsAsStrings();
                options.IncludeXmlComments(GetXmlCommentsPath());
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            // Simple error page to avoid a repo dependency.
            using (var serviceScope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>()
                       .CreateScope())
            {
                //serviceScope.ServiceProvider.GetService<ApplicationDbContext>().Database.Migrate();


                var userManager = app.ApplicationServices.GetService<UserManager<ApplicationUser>>();
                var roleManager = app.ApplicationServices.GetService<RoleManager<IdentityRole>>();

                Task.Run(() => serviceScope.ServiceProvider.GetService<ApplicationDbContext>().EnsureSeedData(userManager, roleManager));

            }

            app.UseMiddleware<ProcessingTimeMiddleware>();

            app.UseIdentity();

            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AutomaticAuthenticate = true,
                AutomaticChallenge = true,
                LoginPath = null,
                AuthenticationScheme = "Identity.External",
                //AuthenticationScheme = "Identity.Application",
                Events = new CookieAuthenticationEvents
                {
                    OnRedirectToLogin = async ctx =>
                    {
                        if (ctx.Response.StatusCode == 200)
                        {
                            ctx.Response.StatusCode = 401;
                        }
                        //if (ctx.Request.Path.StartsWithSegments("/api") && ctx.Response.StatusCode == 200)
                        //{
                        //    ctx.Response.StatusCode = 401;
                        //}
                        //else
                        //{
                        //    ctx.Response.Redirect(ctx.RedirectUri);
                        //}
                        await Task.FromResult(0);
                        //await Task.Yield();
                    },
                    OnRedirectToAccessDenied = async ctx =>
                    {
                        if (ctx.Response.StatusCode == 200)
                        {
                            ctx.Response.StatusCode = 401;
                        }

                        await Task.FromResult(0);
                    }
                }
            });

       
            GoogleOptions ops = new GoogleOptions()
            {

                // ClientId = Configuration["Authentication:Google:ClientId"],
                // ClientSecret = Configuration["Authentication:Google:ClientSecret"],

                 ClientId = Configuration["ClientId"],
                 ClientSecret = Configuration["ClientSecret"],

                SaveTokens = true,
                Events = new OAuthEvents
                {
                    OnRedirectToAuthorizationEndpoint = (context) =>
                    {
                        context.Options.SaveTokens = true;
                        if (context.HttpContext.Items.ContainsKey("OnRedirectToAuthorizationEndpointRequest") && (bool)context.HttpContext.Items["OnRedirectToAuthorizationEndpointRequest"])
                        {
                            context.HttpContext.Items["OnRedirectToAuthorizationEndpoint"] = context;
                        }
                        return Task.FromResult(0);
                    },
                    OnTicketReceived = (context) =>
                     {
                         context.Options.SaveTokens = true;
                         context.HttpContext.Items["Ticket"] = context.Ticket;
                         return Task.FromResult(0);
                     },
                    OnCreatingTicket = (context) =>
                    {
                        context.Options.SaveTokens = true;
                        context.Ticket.Properties.Items.Add("token", context.AccessToken);
                        context.Ticket.Properties.Items.Add("id_token", context.TokenResponse.Response.GetValue("id_token").ToString());
                        return Task.FromResult(0);
                    }
                }
            };
            ops.Scope.Add(CalendarService.Scope.Calendar);

            app.UseGoogleAuthentication(ops);

            app.Use(async (context, next) =>
            {
                try
                {
                    await next();
                }
                catch (Exception ex)
                {
                    if (context.Response.HasStarted)
                    {
                        throw;
                    }
                    context.Response.StatusCode = 500;
                    await context.Response.WriteAsync(ex.ToString());
                }
            });

            app.UseMvc();

            // Enable middleware to serve generated Swagger as a JSON endpoint
            app.UseSwagger();

            // Enable middleware to serve swagger-ui assets (HTML, JS, CSS etc.)
            app.UseSwaggerUi();


        }
    }
}
