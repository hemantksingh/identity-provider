using System.Collections.Generic;
using IdentityServer4.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace identity_provider
{
	public class Startup
	{
		// This method gets called by the runtime before the Configure method. Use this method to add services to the container.
		// For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
		public void ConfigureServices(IServiceCollection services)
		{
            services.AddMvc();

			services.AddIdentityServer()
				.AddDeveloperSigningCredential()
				.AddTestUsers(SeedData.GetUsers())
				.AddInMemoryClients(SeedData.GetClients())
				.AddInMemoryIdentityResources(new List<IdentityResource>
					{
						new IdentityResources.OpenId(),
						new IdentityResources.Profile()
					});
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
		{
			loggerFactory.AddConsole();
			loggerFactory.AddDebug();

			var logger = loggerFactory.CreateLogger<Startup>();
			logger.LogInformation("Configuring the application");

			if (env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
			}

            app.UseIdentityServer();
			app.UseStaticFiles();
            app.UseMvcWithDefaultRoute();
		}
	}
}