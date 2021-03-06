﻿using System.Linq;
using identity;
using identity_provider.Tenants;
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
			services.AddRepositories(@"Server=localhost;Database=identity;Data Source=.;Initial Catalog=identity;Integrated Security=True");

			services.AddMvc();
			
			services.AddIdentityServer()
				.AddDeveloperSigningCredential()
				.AddUserStore()
				.AddInMemoryClients(SeedData.GetClients())
				.AddInMemoryIdentityResources(SeedData.GetIdentityResources())
				.AddInMemoryApiResources(SeedData.GetApiResources());

		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IHostingEnvironment env, 
			ILoggerFactory loggerFactory, 
			TenantRepository tenantRepository,
			UserRepository userRepository)
		{
			loggerFactory.AddConsole();
			loggerFactory.AddDebug();

			var logger = loggerFactory.CreateLogger<Startup>();
			logger.LogInformation("Configuring the application");

			if (env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
			}

			var tenant = tenantRepository.AddInitialTenant(SeedData.GetTenants().First());
			userRepository.AddInitialUsers(SeedData.GetUsers(), tenant);

			app.UseIdentityServer();
			app.UseStaticFiles();
			app.UseMvcWithDefaultRoute();
		}
	}
}