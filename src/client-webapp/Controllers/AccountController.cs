using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Newtonsoft.Json.Linq;

namespace client_webapp.Controllers
{
	public class AccountController : Controller
	{
		private readonly HttpClient _client;

		public AccountController(HttpClient client)
		{
			_client = client;
		}

		public IActionResult Login()
		{
			if (!HttpContext.User.Identity.IsAuthenticated)
			{
				return Challenge(OpenIdConnectDefaults.AuthenticationScheme);
			}

			return RedirectToAction("Index", "Home");
		}

		public IActionResult Logout()
		{
			if (HttpContext.User.Identity.IsAuthenticated)
			{
				return SignOut(CookieAuthenticationDefaults.AuthenticationScheme, OpenIdConnectDefaults.AuthenticationScheme);
			}

			return RedirectToAction("Index", "Home");
		}

		[Authorize]
		public async Task<IActionResult> Claims()
		{
			var token = await HttpContext.GetTokenAsync(OpenIdConnectParameterNames.AccessToken);
			try
			{
				string json = await _client
					.AuthorizationHeader("Bearer", token)
					.GetStringAsync(new Uri("https://localhost:44357/api/identity"));
				ViewBag.Json = JArray.Parse(json).ToString();

				return View();
			}
			catch (HttpException e)
			{
				if (e.StatusCode == HttpStatusCode.Unauthorized || e.StatusCode == HttpStatusCode.Forbidden)
					return RedirectToAction("Index", "Home");

				throw;
			}
		}
	}
}