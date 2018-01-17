using System.Collections.Generic;
using System.Security.Claims;
using IdentityServer4;
using IdentityServer4.Models;
using IdentityServer4.Test;

namespace identity_provider
{
    public static class SeedData
    {
	    public static List<TestUser> GetUsers()
	    {
		    return new List<TestUser>
		    {
			    new TestUser
			    {
				    SubjectId = "d860efca-22d9-47fd-8249-791ba61b07c7",
				    Username = "Jawan",
				    Password = "password",

				    Claims = new List<Claim>
				    {
					    new Claim("given_name", "Jawan"),
					    new Claim("family_name", "Kisan"),
				    }
			    },
			    new TestUser
			    {
				    SubjectId = "b7539694-97e7-4dfe-84da-b4256e1ff5c7",
				    Username = "Claire",
				    Password = "password",

				    Claims = new List<Claim>
				    {
					    new Claim("given_name", "Claire"),
					    new Claim("family_name", "Underwood"),
				    }
			    }
		    };
	    }

	    public static IEnumerable<IdentityResource> GetIdentityResources()
	    {
		    return new List<IdentityResource>
		    {
			    new IdentityResources.OpenId(),
			    new IdentityResources.Profile(),
			    new IdentityResources.Address(),
			    new IdentityResource("roles", "Your role(s)", new List<string> {"role"})
		    };
	    }

	    public static IEnumerable<ApiResource> GetApiResources()
	    {
		    return new List<ApiResource>
		    {
			    new ApiResource("resourceapi", "Resource API")
		    };
	    }

	    public static IEnumerable<Client> GetClients()
	    {
		    return new List<Client>
		    {
			    new Client
			    {
				    ClientName = "WebApp Client",
				    ClientId = "client-webapp",
				    AllowedGrantTypes = GrantTypes.Hybrid,
				    RedirectUris = new List<string>
				    {
					    "https://localhost:44305/signin-oidc"
				    },
					PostLogoutRedirectUris = new List<string>
					{
						"https://localhost:44305/signout-callback-oidc"
					},
				    AllowedScopes =
				    {
					    IdentityServerConstants.StandardScopes.OpenId,
					    IdentityServerConstants.StandardScopes.Profile,
						IdentityServerConstants.StandardScopes.Address,
						"roles",
						"resourceapi"
				    },
				    ClientSecrets =
				    {
					    new Secret("secret".Sha256())
				    }
			    }
		    };
	    }
    }
}
