using System;
using identity;
using Microsoft.Extensions.DependencyInjection;

namespace identity_provider
{
    public static class IdentityServerBuilderExtensions
    {
	    public static IIdentityServerBuilder AddUserStore(this IIdentityServerBuilder builder)
	    {
		    builder.AddProfileService<UserProfileService>();
		    return builder;
	    }
    }
}
