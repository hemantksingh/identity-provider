using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace resource_api.Controllers
{
	[Route("api/[controller]")]
	public class IdentityController : Controller
	{
		[Authorize]
		public IActionResult Get()
		{
			return Json(User.Claims.Select(claim => new
			{
				type = claim.Type,
				value = claim.Value
			}));
		}
	}
}