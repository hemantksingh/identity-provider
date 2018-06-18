using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using Dapper;
using identity;

namespace identity_provider.Tenants
{
	public class TenantRepository
	{
		private readonly Func<SqlConnection> _createConnection;

		public TenantRepository(Func<SqlConnection> createConnection)
		{
			_createConnection = createConnection;
		}

		public IEnumerable<Tenant> GetAllTenants()
		{
			using (var connection = _createConnection())
			{
				IEnumerable<dynamic> tenants = connection.Query<dynamic>("SELECT * FROM Tenants");

				return tenants.Select(x => new Tenant
				{
					Id = Guid.Parse(x.TenantId),
					Name = x.TenantName,
					OrganizationName = x.OrganizationName,
					License = x.License,
					IsActive = x.IsActive
				});
			}
		}

		public Tenant AddTenant(Tenant tenant)
		{
			using (var connection = _createConnection())
			{
				connection.Execute(
					"INSERT INTO Tenants (TenantId, TenantName, OrganizationName, License, IsActive)"
					+ " VALUES(@TenantId, @TenantName, @OrganizationName, @License, @IsActive)",
					new
					{
						TenantId = tenant.Id.ToString(),
						TenantName = tenant.Name,
						tenant.OrganizationName,
						tenant.License,
						tenant.IsActive
					});
			}

			return new Tenant
			{
				Id = tenant.Id,
				Name = tenant.Name,
				OrganizationName = tenant.OrganizationName,
				License = tenant.License,
				IsActive = tenant.IsActive
			};
		}

		public Tenant AddInitialTenant(Tenant tenant)
		{
			return GetAllTenants().Any() ? tenant : AddTenant(tenant);
		}

		public Tenant GetTenantByName(string name)
		{
			using (var connection = _createConnection())
			{
				var tenant = connection.Query<dynamic>(
						"SELECT * FROM Tenants WHERE TenantName = @name",
						new {name})
					.FirstOrDefault();

				return tenant == null
					? null
					: new Tenant
					{
						Id = Guid.Parse(tenant.TenantId),
						Name = tenant.TenantName,
						OrganizationName = tenant.OrganizationName,
						License = tenant.License,
						IsActive = tenant.IsActive
					};
			}
		}
	}
}