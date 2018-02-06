using System.Collections.Generic;
using System.Security.Claims;
using IdentityModel;
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
				    SubjectId = "818727",
				    Username = "Jawan",
				    Password = "password",
				    Claims = new List<Claim>
				    {
					    new Claim(JwtClaimTypes.Name, "Jawan Kisan"),
						new Claim(JwtClaimTypes.GivenName, "Jawan"),
					    new Claim(JwtClaimTypes.FamilyName, "Kisan"),
					    new Claim(JwtClaimTypes.Email, "jawan@kisan.com"),
					    new Claim(JwtClaimTypes.Address, @"{ 'street_address': '10 Kisaan Marg', 'locality': 'Surajpur', 'postal_code': 201301, 'country': 'India' }", IdentityServerConstants.ClaimValueTypes.Json),
					    new Claim(JwtClaimTypes.Role, "FreeUser")
				    }
			    },
			    new TestUser
			    {
				    SubjectId = "818728",
				    Username = "Jag",
				    Password = "p",

				    Claims = new List<Claim>
				    {
					    new Claim(JwtClaimTypes.Name, "Jag Jivan"),
					    new Claim(JwtClaimTypes.GivenName, "Jag"),
					    new Claim(JwtClaimTypes.FamilyName, "Jivan"),
					    new Claim(JwtClaimTypes.Email, "jag@jivan.com"),
					    new Claim(JwtClaimTypes.Address, @"{ 'street_address': '20 Vikas Marg', 'locality': 'Hisaar', 'postal_code': 201307, 'country': 'India' }", IdentityServerConstants.ClaimValueTypes.Json),
						new Claim(JwtClaimTypes.Role, "PayingUser")
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
			    new ApiResource("resourceapi", "Resource API", new List<string>{"role"})
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
					RequireConsent = false,
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
			    },
		        // JavaScript Client
		        new Client
		        {
		            ClientId = "js",
		            ClientName = "JavaScript Client",
		            AllowedGrantTypes = GrantTypes.Implicit,
		            AllowAccessTokensViaBrowser = true,

		            RedirectUris =           { "http://localhost:52371/callback.html" },
		            PostLogoutRedirectUris = { "http://localhost:52371/index.html" },
		            AllowedCorsOrigins =     { "http://localhost:52371" },

		            AllowedScopes =
		            {
		                IdentityServerConstants.StandardScopes.OpenId,
		                IdentityServerConstants.StandardScopes.Profile,
		                "resourceapi"
                    }
		        }


            };
	    }
    }
}
