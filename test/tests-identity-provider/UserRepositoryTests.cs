using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Security.Claims;
using Bogus;
using identity;
using identity_provider;
using identity_provider.Tenants;
using IdentityModel;
using IdentityServer4;
using Xunit;

namespace tests_identity_provider
{
	public class UserRepositoryTests
	{
		private static SqlConnection GetConnection(string connectionString)
		{
			var sqlConnection = new SqlConnection(connectionString);
			sqlConnection.Open();
			return sqlConnection;
		}

		[Fact]
		public void AddsUser()
		{
			var connectionString = Environment.GetEnvironmentVariable("identityConnection") ??
				"Server=localhost;Database=identity;Data Source=.;Initial Catalog=identity;Integrated Security=True";
			var repository = new UserRepository(
				() => GetConnection(connectionString),
				() => new UnitOfWork(GetConnection(connectionString)));

			var tenantRepository = new TenantRepository(() => GetConnection(connectionString));

			var fakeTenant = new Faker<Tenant>().StrictMode(true)
				.RuleFor(t => t.Id, Guid.NewGuid())
				.RuleFor(t => t.IsActive, true)
				.RuleFor(t => t.License, "Standard")
				.RuleFor(t => t.Name, t => t.Random.String2(10, 10))
				.RuleFor(t => t.OrganizationName, t => t.Company.CompanyName());

			Tenant tenant = tenantRepository.AddTenant(fakeTenant.Generate());

			var fakeUser = new Faker<User>().StrictMode(true)
				.RuleFor(u => u.SubjectId, Guid.NewGuid().ToString())
				.RuleFor(u => u.TenantId, tenant.Id.ToString())
				.RuleFor(u => u.Username, u => u.Internet.UserName(u.Name.FirstName()))
				.RuleFor(u => u.Password, u => u.Internet.Password())
				.RuleFor(u => u.IsActive, true)
				.RuleFor(u => u.Claims, u => new List<Claim>()
				{
					new Claim(JwtClaimTypes.Name, $"{u.Name.FirstName()}  {u.Name.LastName()}"),
					new Claim(JwtClaimTypes.GivenName, u.Name.FirstName()),
					new Claim(JwtClaimTypes.FamilyName, u.Name.LastName()),
					new Claim(JwtClaimTypes.Address,
						$"{{ 'street_address': '{u.Address.StreetAddress()}', 'locality': '{u.Address.City()}', 'postal_code': '{u.Address.ZipCode()}', 'country': '{u.Address.Country()}' }}",
						IdentityServerConstants.ClaimValueTypes.Json),
					new Claim(JwtClaimTypes.Email, u.Internet.Email()),
					new Claim(JwtClaimTypes.Role, "FreeUser"),
					new Claim(ClaimType.TenantId, tenant.Id.ToString())
				})
				.RuleFor(u => u.IdentityProviders, new List<IdentityProvider>());

			var user = fakeUser.Generate();
			repository.AddUser(user);
		}
	}
}