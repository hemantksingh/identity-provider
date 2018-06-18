// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading.Tasks;
using identity_provider;
using identity_provider.Tenants;
using IdentityModel;
using IdentityServer4;
using IdentityServer4.Events;
using IdentityServer4.Extensions;
using IdentityServer4.Models;
using IdentityServer4.Services;
using IdentityServer4.Stores;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace identity
{
	/// <summary>
	/// This sample controller implements a typical login/logout/provision workflow for local and external accounts.
	/// The login service encapsulates the interactions with the user data store. This data store is in-memory only and cannot be used for production!
	/// The interaction service provides a way for the UI to communicate with identityserver for validation and context retrieval
	/// </summary>
	[SecurityHeaders]
	public class AccountController : Controller
	{
		private readonly IIdentityServerInteractionService _interaction;
		private readonly IEventService _events;
		private readonly UserRepository _userRepository;
		private readonly TenantRepository _tenantRepository;
		private readonly AccountService _account;

		public AccountController(
			IIdentityServerInteractionService interaction,
			IClientStore clientStore,
			IHttpContextAccessor httpContextAccessor,
			IAuthenticationSchemeProvider schemeProvider,
			IEventService events,
			UserRepository userRepository,
			TenantRepository tenantRepository)
		{
			_interaction = interaction;
			_events = events;
			_userRepository = userRepository;
			_tenantRepository = tenantRepository;
			_account = new AccountService(interaction, httpContextAccessor, schemeProvider, clientStore);
		}

		/// <summary>
		/// Show login page
		/// </summary>
		[HttpGet]
		public async Task<IActionResult> Login(string returnUrl)
		{
			// build a model so we know what to show on the login page
			var vm = await _account.BuildLoginViewModelAsync(returnUrl);

			if (vm.IsExternalLoginOnly)
			{
				// we only have one option for logging in and it's an external provider
				return await ExternalLogin(vm.ExternalLoginScheme, returnUrl);
			}

			return View(vm);
		}

		/// <summary>
		/// Handle postback from username/password login
		/// </summary>
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Login(LoginInputModel model, string button)
		{
			if (button != "login")
			{
				// the user clicked the "cancel" button
				var context = await _interaction.GetAuthorizationContextAsync(model.ReturnUrl);
				if (context != null)
				{
					// if the user cancels, send a result back into IdentityServer as if they 
					// denied the consent (even if this client does not require consent).
					// this will send back an access denied OIDC error response to the client.
					await _interaction.GrantConsentAsync(context, ConsentResponse.Denied);

					// we can trust model.ReturnUrl since GetAuthorizationContextAsync returned non-null
					return Redirect(model.ReturnUrl);
				}
				else
				{
					// since we don't have a valid context, then we just go back to the home page
					return Redirect("~/");
				}
			}

			if (ModelState.IsValid)
			{
				// validate username/password against in-memory store
				if (_userRepository.CredentialsValid(model.Username, model.Password))
				{
					var user = _userRepository.GetByUsername(model.Username);
					await _events.RaiseAsync(new UserLoginSuccessEvent(user.Username, user.SubjectId, user.Username));

					// only set explicit expiration here if user chooses "remember me". 
					// otherwise we rely upon expiration configured in cookie middleware.
					AuthenticationProperties props = null;
					if (AccountOptions.AllowRememberLogin && model.RememberLogin)
					{
						props = new AuthenticationProperties
						{
							IsPersistent = true,
							ExpiresUtc = DateTimeOffset.UtcNow.Add(AccountOptions.RememberMeLoginDuration)
						};
					}

					// issue authentication cookie with subject ID and username
					await HttpContext.SignInAsync(user.SubjectId, user.Username, props);

					// make sure the returnUrl is still valid, and if so redirect back to authorize endpoint or a local page
					if (_interaction.IsValidReturnUrl(model.ReturnUrl) || Url.IsLocalUrl(model.ReturnUrl))
					{
						return Redirect(model.ReturnUrl);
					}

					return Redirect("~/");
				}

				await _events.RaiseAsync(new UserLoginFailureEvent(model.Username, "invalid credentials"));

				ModelState.AddModelError("", AccountOptions.InvalidCredentialsErrorMessage);
			}

			// something went wrong, show form with error
			var vm = await _account.BuildLoginViewModelAsync(model);
			return View(vm);
		}

		/// <summary>
		/// initiate roundtrip to external authentication provider
		/// </summary>
		[HttpGet]
		public async Task<IActionResult> ExternalLogin(string provider, string returnUrl)
		{
			var props = new AuthenticationProperties
			{
				RedirectUri = UrlHelperExtensions.Action(Url, "ExternalLoginCallback"),
				Items =
				{
					{"returnUrl", returnUrl}
				}
			};

			// windows authentication needs special handling
			// since they don't support the redirect uri, 
			// so this URL is re-triggered when we call challenge
			if (AccountOptions.WindowsAuthenticationSchemeName == provider)
			{
				// see if windows auth has already been requested and succeeded
				var result = await HttpContext.AuthenticateAsync(AccountOptions.WindowsAuthenticationSchemeName);
				if (result?.Principal is WindowsPrincipal wp)
				{
					props.Items.Add("scheme", AccountOptions.WindowsAuthenticationSchemeName);

					var id = new ClaimsIdentity(provider);
					id.AddClaim(new Claim(JwtClaimTypes.Subject, wp.Identity.Name));
					id.AddClaim(new Claim(JwtClaimTypes.Name, wp.Identity.Name));

					// add the groups as claims -- be careful if the number of groups is too large
					if (AccountOptions.IncludeWindowsGroups)
					{
						var wi = wp.Identity as WindowsIdentity;
						var groups = wi.Groups.Translate(typeof(NTAccount));
						var roles = groups.Select(x => new Claim(JwtClaimTypes.Role, x.Value));
						id.AddClaims(roles);
					}

					await HttpContext.SignInAsync(
						IdentityServerConstants.ExternalCookieAuthenticationScheme,
						new ClaimsPrincipal(id),
						props);
					return Redirect(props.RedirectUri);
				}
				// challenge/trigger windows auth
				return Challenge(AccountOptions.WindowsAuthenticationSchemeName);
			}
			// start challenge and roundtrip the return URL
			props.Items.Add("scheme", provider);
			return Challenge(props, provider);
		}

		/// <summary>
		/// Post processing of external authentication
		/// </summary>
		[HttpGet]
		public async Task<IActionResult> ExternalLoginCallback()
		{
			// read external identity from the temporary cookie
			var result = await HttpContext.AuthenticateAsync(IdentityServerConstants.ExternalCookieAuthenticationScheme);
			if (result?.Succeeded != true)
			{
				throw new Exception("External authentication error");
			}

			var externalUser = new ExternalUser(result.Properties.Items["scheme"], result.Principal);
			var provider = externalUser.Provider;
			var userId = externalUser.UniqueIdentifier();
			var user = _userRepository.GetUser(userId, provider);
			if (user == null)
			{
				// this sample simply auto-provisions new external user
				// another common approach is to start a registrations workflow first
				var context = _interaction.GetAuthorizationContextAsync(result.Properties.Items["returnUrl"]);
				var tenant = _tenantRepository.GetTenantByName(context.Result.Tenant);

				user = externalUser.ProvisionUser(userId, tenant.Id.ToString());
				_userRepository.AddUser(user);
			}

			// if the external provider issued an id_token, we'll keep it for signout
			AuthenticationProperties props = null;
			var id_token = AuthenticationTokenExtensions.GetTokenValue(result.Properties, "id_token");
			if (id_token != null)
			{
				props = new AuthenticationProperties();
				props.StoreTokens(new[] {new AuthenticationToken {Name = "id_token", Value = id_token}});
			}

			// issue authentication cookie for user
			await _events.RaiseAsync(new UserLoginSuccessEvent(provider, userId, user.SubjectId, user.Username));
			await HttpContext.SignInAsync(user.SubjectId, user.Username, provider, props, 
				externalUser.SessionIdClaim(), 
				user.Claims.First(x => x.Type == ClaimType.TenantId));

			// delete temporary cookie used during external authentication
			await HttpContext.SignOutAsync(IdentityServerConstants.ExternalCookieAuthenticationScheme);

			// validate return URL and redirect back to authorization endpoint or a local page
			var returnUrl = result.Properties.Items["returnUrl"];
			if (_interaction.IsValidReturnUrl(returnUrl) || Url.IsLocalUrl(returnUrl))
			{
				return Redirect(returnUrl);
			}

			return Redirect("~/");
		}

		/// <summary>
		/// Show logout page
		/// </summary>
		[HttpGet]
		public async Task<IActionResult> Logout(string logoutId)
		{
			// build a model so the logout page knows what to display
			var vm = await _account.BuildLogoutViewModelAsync(logoutId);

			if (vm.ShowLogoutPrompt == false)
			{
				// if the request for logout was properly authenticated from IdentityServer, then
				// we don't need to show the prompt and can just log the user out directly.
				return await Logout(vm);
			}

			return View(vm);
		}

		/// <summary>
		/// Handle logout page postback
		/// </summary>
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Logout(LogoutInputModel model)
		{
			// build a model so the logged out page knows what to display
			var vm = await _account.BuildLoggedOutViewModelAsync(model.LogoutId);

			var user = HttpContext.User;
			if (user?.Identity.IsAuthenticated == true)
			{
				// delete local authentication cookie
				await HttpContext.SignOutAsync();

				// raise the logout event
				await _events.RaiseAsync(new UserLogoutSuccessEvent(user.GetSubjectId(), user.GetDisplayName()));
			}

			// check if we need to trigger sign-out at an upstream identity provider
			if (vm.TriggerExternalSignout)
			{
				// build a return URL so the upstream provider will redirect back
				// to us after the user has logged out. this allows us to then
				// complete our single sign-out processing.
				string url = UrlHelperExtensions.Action(Url, "Logout", new {logoutId = vm.LogoutId});

				// this triggers a redirect to the external provider for sign-out
				return SignOut(new AuthenticationProperties {RedirectUri = url}, vm.ExternalAuthenticationScheme);
			}

			return View("LoggedOut", vm);
		}

		[HttpGet]
		public IActionResult Register(string returnUrl)
		{
			var viewModel = new RegisterViewModel {ReturnUrl = returnUrl};
			return View(viewModel);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Register(RegisterViewModel model)
		{
			if (!ModelState.IsValid)
				return View(model);
			Task<AuthorizationRequest> context = _interaction.GetAuthorizationContextAsync(model.ReturnUrl);
			Tenant tenant = _tenantRepository.GetTenantByName(context.Result.Tenant);
			var user = new User
			{
				SubjectId = Guid.NewGuid().ToString(),
				TenantId = tenant.Id.ToString(),
				Username = model.Username,
				Password = model.Password,
				IsActive = true,
				Claims = new List<Claim>
				{
					new Claim(JwtClaimTypes.Name, $"{model.Firstname}  {model.Lastname}"),
					new Claim(JwtClaimTypes.GivenName, model.Firstname),
					new Claim(JwtClaimTypes.FamilyName, model.Lastname),
					new Claim(JwtClaimTypes.Address,
						$"{{ 'street_address': '{model.Address}', 'locality': 'Default', 'postal_code': 000000, 'country': '{model.Country}' }}",
						IdentityServerConstants.ClaimValueTypes.Json),
					new Claim(JwtClaimTypes.Email, model.Email),
					new Claim(JwtClaimTypes.Role, "FreeUser"),
					new Claim(ClaimType.TenantId, tenant.Id.ToString())
				}
			};
			_userRepository.AddUser(user);

			await HttpContext.SignInAsync(user.SubjectId, user.Username, new Claim(ClaimType.TenantId, tenant.Id.ToString()));

			if (_interaction.IsValidReturnUrl(model.ReturnUrl) || Url.IsLocalUrl(model.ReturnUrl))
				return Redirect(model.ReturnUrl);

			return Redirect("~/");
		}
	}
}