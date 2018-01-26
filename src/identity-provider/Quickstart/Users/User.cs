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

		public User AddClaim(Claim claim)
		{
			Claims.ToList().Add(claim);
			return this;
		}
	}
}