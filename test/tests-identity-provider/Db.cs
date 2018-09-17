using System;
using System.Data.SqlClient;
using Bogus;
using identity;
using identity_provider.Tenants;

namespace tests_identity_provider
{
    class Db
    {
	    private static readonly string ConnectionString =
		    Environment.GetEnvironmentVariable("identityConnection") ??
		    "Server=localhost;Database=identity;Data Source=.;Initial Catalog=identity;Integrated Security=True";

	    private readonly TenantRepository _tenantRepository;

	    public Db()
	    {
		    _tenantRepository = new TenantRepository(Connection);
	    }
	    public static SqlConnection Connection()	
	    {
		    var connection = new SqlConnection(ConnectionString);
		    connection.Open();
		    return connection;
	    }

	    public Tenant AddTenant()
	    {
		    var tenant = new Faker<Tenant>().StrictMode(true)
			    .RuleFor(t => t.Id, Guid.NewGuid())
			    .RuleFor(t => t.IsActive, true)
			    .RuleFor(t => t.License, "Standard")
			    .RuleFor(t => t.Name, t => t.Random.String2(10, 10))
			    .RuleFor(t => t.OrganizationName, t => t.Company.CompanyName())
			    .Generate();

		    return _tenantRepository.AddTenant(tenant);
		}
	}
}
