using identity;
using identity_provider.Quickstart.Users;
using Microsoft.Extensions.DependencyInjection;

namespace identity_provider
{
    public static class IdentityServerBuilderExtensions
    {
	    public static IIdentityServerBuilder AddUserStore(this IIdentityServerBuilder builder)
	    {
		    builder.Services.AddSingleton<UserRepository>();
		    builder.AddProfileService<UserProfileService>();
		    return builder;
	    }
    }
}
