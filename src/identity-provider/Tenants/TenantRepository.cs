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
			   return connection.Query<Tenant>("SELECT * FROM Tenants");
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
    }
}
