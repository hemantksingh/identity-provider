using System.Data.SqlClient;
using System.Linq;
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
			const string connectionString = @"Server=localhost;Database=identity;Data Source=.;Initial Catalog=identity;Integrated Security=True";
		    services.AddSingleton(provider => new UserRepository(() => GetConnection(connectionString), () => new UnitOfWork(GetConnection(connectionString))));
			services.AddSingleton(provider => new TenantRepository(() => GetConnection(connectionString)));

			services.AddMvc();
			
			services.AddIdentityServer()
				.AddDeveloperSigningCredential()
				.AddUserStore()
				.AddInMemoryClients(SeedData.GetClients())
				.AddInMemoryIdentityResources(SeedData.GetIdentityResources())
				.AddInMemoryApiResources(SeedData.GetApiResources());
		}

		/// <summary>
		/// Creates a new sql connection per query. Closing the connection after finishing processing the query
		/// does not actually close the physical connection but just returns it to the connection pool, managed
		/// by dotnet. Let dotnet handle connection pooling:
		/// https://docs.microsoft.com/en-us/dotnet/framework/data/adonet/sql-server-connection-pooling
		/// </summary>
		/// <returns>SqlConnection</returns>
		private static SqlConnection GetConnection(string connectionString)
		{
			var sqlConnection = new SqlConnection(connectionString);
			sqlConnection.Open();
			return sqlConnection;
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