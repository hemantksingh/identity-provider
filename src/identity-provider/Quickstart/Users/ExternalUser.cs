using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using IdentityModel;

namespace identity_provider.Quickstart.Users
{
    public class ExternalUser
    {
	    public readonly string Provider;
	    private readonly ClaimsPrincipal _principal;

	    public ExternalUser(string provider, ClaimsPrincipal principal)
	    {
		    Provider = provider;
		    _principal = principal;
	    }

	    public string UniqueIdentifier()
	    {
		    var userIdClaim = UserIdClaim();

		    return userIdClaim == null
			    ? throw new InvalidOperationException(
					$"No unique identifier found for external user issued by {Provider} provider. " +
					$"Attempted claims {JwtClaimTypes.Subject} & {ClaimTypes.NameIdentifier}")
			    : userIdClaim.Value;
	    }

	    private Claim UserIdClaim()
	    {
		    return _principal.Claims.FirstOrDefault(x => x.Type == JwtClaimTypes.Subject)
		           ?? _principal.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier);
	    }

	    public User ProvisionUser(string userId)
	    {
			var filtered = new List<Claim>();

			foreach (var claim in _principal.Claims)
			{
				// if the external system sends a display name - translate that to the standard OIDC name claim
				if (claim.Type == ClaimTypes.Name)
				{
					filtered.Add(new Claim(JwtClaimTypes.Name, claim.Value));
				}
				// if the JWT handler has an outbound mapping to an OIDC claim use that
				else if (JwtSecurityTokenHandler.DefaultOutboundClaimTypeMap.ContainsKey(claim.Type))
				{
					filtered.Add(new Claim(JwtSecurityTokenHandler.DefaultOutboundClaimTypeMap[claim.Type], claim.Value));
				}
				else if(claim.Type != UserIdClaim().Type)
				{
					filtered.Add(claim);
				}
			}

			// if no display name was provided, try to construct by first and/or last name
			if (filtered.All(x => x.Type != JwtClaimTypes.Name))
			{
				var first = filtered.FirstOrDefault(x => x.Type == JwtClaimTypes.GivenName)?.Value;
				var last = filtered.FirstOrDefault(x => x.Type == JwtClaimTypes.FamilyName)?.Value;
				if (first != null && last != null)
				{
					filtered.Add(new Claim(JwtClaimTypes.Name, first + " " + last));
				}
				else if (first != null)
				{
					filtered.Add(new Claim(JwtClaimTypes.Name, first));
				}
				else if (last != null)
				{
					filtered.Add(new Claim(JwtClaimTypes.Name, last));
				}
			}

			var sub = Guid.NewGuid().ToString();
		    return new User
			{
				SubjectId = sub,
				Username = filtered.FirstOrDefault(c => c.Type == JwtClaimTypes.Name)?.Value ?? sub,
				IsActive = true,
				Claims = filtered,
				IdentityProviders = new List<IdentityProvider>
				{
					new IdentityProvider { Name = Provider, ProviderSubjectId = userId}
				}
			};
	    }

	    public Claim SessionIdClaim()
	    {
			var sid = _principal.Claims.FirstOrDefault(x => x.Type == JwtClaimTypes.SessionId);
		    return sid != null ? new Claim(JwtClaimTypes.SessionId, sid.Value) : null;
	    }
    }
}
