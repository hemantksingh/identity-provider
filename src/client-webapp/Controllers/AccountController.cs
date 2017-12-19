﻿using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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
		public IActionResult Claims()
		{
			return View();
		}
	}
}