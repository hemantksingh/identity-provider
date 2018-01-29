using identity;
using identity_provider.Quickstart.Users;
using Microsoft.Extensions.DependencyInjection;

namespace identity_provider
{
    public static class IdentityServerBuilderExtensions
    {
	    public static IIdentityServerBuilder AddUserStore(this IIdentityServerBuilder builder, UserRepository repository)
	    {
		    builder.Services.AddTransient(provider => repository);
		    builder.AddProfileService<UserProfileService>();
		    return builder;
	    }
    }
}
