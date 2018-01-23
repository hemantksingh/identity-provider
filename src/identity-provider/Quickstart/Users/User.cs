using System.Collections.Generic;

namespace identity_provider.Quickstart.Users
{
	public class User
	{
		public string SubjectId;
		public string Username;
		public string Password;
		public bool IsActive;
		public List<UserClaim> UserClaims = new List<UserClaim>();
	}
}