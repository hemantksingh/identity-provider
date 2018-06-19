using System.Data.SqlClient;
using identity;
using identity_provider.Tenants;
using Microsoft.Extensions.DependencyInjection;

namespace identity_provider
{
	public static class ServiceCollectionExtensions
	{
		public static IServiceCollection AddRepositories(this IServiceCollection services,
			string connectionString)
		{
			services.AddSingleton(provider => new UserRepository(() =>
				GetConnection(connectionString), () => new UnitOfWork(GetConnection(connectionString))));
			services.AddSingleton(provider => new TenantRepository(() =>
				GetConnection(connectionString)));

			return services;
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
	}
}