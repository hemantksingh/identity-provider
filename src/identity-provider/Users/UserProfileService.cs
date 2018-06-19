using System.Threading.Tasks;
using IdentityServer4.Extensions;
using IdentityServer4.Models;
using IdentityServer4.Services;

namespace identity
{
	/// <summary>
	/// Gets identity information about users when creating tokens or when handling requests to the userinfo
	/// or introspection endpoints. It is impractical to put all of the possible claims needed for users into the cookie, 
	/// so IdentityServer defines an extensibility point for allowing claims to be dynamically loaded as needed for a user.
	/// </summary>
	public class UserProfileService : IProfileService
	{
		private readonly UserRepository _userRepository;

		public UserProfileService(UserRepository userRepository)
		{
			_userRepository = userRepository;
		}
		public Task GetProfileDataAsync(ProfileDataRequestContext context)
		{
			var subjectId = context.Subject.GetSubjectId();
			var claims = _userRepository.GetClaims(subjectId);
			context.AddRequestedClaims(claims);
			return Task.FromResult(0);
		}

		/// <summary>
		/// Determines if the user is currently allowed to obtain tokens.
		/// </summary>
		/// <param name="context"></param>
		/// <returns></returns>
		public Task IsActiveAsync(IsActiveContext context)
		{
			var subjectId = context.Subject.GetSubjectId();
			context.IsActive = _userRepository.IsUserActive(subjectId);
			return Task.FromResult(0);
		}
	}
}