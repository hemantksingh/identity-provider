﻿using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;

namespace client_webapp
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
			services.AddTransient(p => new HttpClient(new System.Net.Http.HttpClient(), p.GetService<ILogger<HttpClient>>()));

			services.AddAuthentication(options =>
				{
					options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
					options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
					options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
				})
				.AddCookie()
				.AddOpenIdConnect(options =>
				{
					options.Authority = "https://localhost:44376/";
					options.ClientId = "client-webapp";
					options.ClientSecret = "secret";
					options.RequireHttpsMetadata = true;
					options.ResponseType = "code id_token";
					options.Scope.Add("openid");
					options.Scope.Add("profile");
					options.Scope.Add("address");
					options.Scope.Add("roles");
					options.Scope.Add("resourceapi");
					options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
					options.SaveTokens = true;
					options.GetClaimsFromUserInfoEndpoint = true;
					options.UseTokenLifetime = false;
					options.TokenValidationParameters = new TokenValidationParameters
					{
						NameClaimType = "name"
					};
					options.Events.OnRedirectToIdentityProvider = context =>
					{
						if (context.ProtocolMessage.RequestType == OpenIdConnectRequestType.Authentication)
						{
							context.ProtocolMessage.AcrValues = "tenant:default";
						}
						return Task.FromResult(0);
					};
				});

			services.AddMvc();
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
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

			app.UseAuthentication();

			app.UseStaticFiles();

			app.UseMvc(routes =>
			{
				routes.MapRoute(
					name: "default",
					template: "{controller=Home}/{action=Index}/{id?}");
			});
		}
	}
}