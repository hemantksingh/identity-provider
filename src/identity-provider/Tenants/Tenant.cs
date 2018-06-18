using System;

namespace identity
{
	public class Tenant
	{
		public Guid Id { get; set; }
		public string Name { get; set; }
		public string OrganizationName { get; set; }
		public string License { get; set; }
		public bool IsActive { get; set; }
	}
}