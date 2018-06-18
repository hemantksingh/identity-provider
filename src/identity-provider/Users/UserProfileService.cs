using System.Threading.Tasks;
using IdentityServer4.Extensions;
using IdentityServer4.Models;
using IdentityServer4.Services;

namespace identity
{
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

		public Task IsActiveAsync(IsActiveContext context)
		{
			var subjectId = context.Subject.GetSubjectId();
			context.IsActive = _userRepository.IsUserActive(subjectId);
			return Task.FromResult(0);
		}
	}
}