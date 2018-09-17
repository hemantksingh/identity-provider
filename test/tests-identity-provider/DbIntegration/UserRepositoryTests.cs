using System;
using System.Collections.Generic;
using System.Security.Claims;
using Bogus;
using identity;
using identity_provider;
using IdentityModel;
using IdentityServer4;
using Xunit;

namespace tests_identity_provider.DbIntegration
{
	public class UserRepositoryTests
	{
		private readonly UserRepository _userRepository = new UserRepository(
			Db.Connection,
			() => new UnitOfWork(Db.Connection()));

		[Fact]
		public void AddsUser()
		{
			Tenant tenant = new Db().AddTenant();

			var user = new Faker<User>().StrictMode(true)
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
				.RuleFor(u => u.IdentityProviders, new List<IdentityProvider>())
				.Generate();

			_userRepository.AddUser(user);
		}
	}
}