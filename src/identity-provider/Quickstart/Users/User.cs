using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

namespace identity_provider.Quickstart.Users
{
	public class User
	{
		public string SubjectId;
		public string Username;
		public string Password;
		public bool IsActive;
		public IEnumerable<Claim> Claims = new List<Claim>();
		public IEnumerable<IdentityProvider> IdentityProviders = new List<IdentityProvider>();

		public User AddClaim(Claim claim)
		{
			if (string.IsNullOrEmpty(claim.Type) || string.IsNullOrEmpty(claim.Value))
				return this;
			var claims = Claims.ToList();
			claims.Add(claim);
			Claims = claims;
			return this;
		}

		public User AddProvider(IdentityProvider provider)
		{
			if (string.IsNullOrEmpty(provider.Name) || string.IsNullOrEmpty(provider.ProviderSubjectId))
				return this;
			var providers = IdentityProviders.ToList();
			providers.Add(provider);
			IdentityProviders = providers;
			return this;
		}
	}
}