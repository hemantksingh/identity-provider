using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
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

			var client = new HttpClient { BaseAddress = new Uri("https://localhost:44357/") };
			client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
			string json = await client.GetStringAsync("api/identity");

			ViewBag.Json = JArray.Parse(json).ToString();

			return View();
		}
	}
}