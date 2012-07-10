using System;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Security;
using Octgn.App.Models;

namespace Octgn.App.Controllers
{

	[HandleError]
	public class AccountController : Controller
	{

		public IFormsAuthenticationService FormsService { get; set; }
		public IMembershipService MembershipService { get; set; }

		protected override void Initialize(RequestContext requestContext)
		{
			if (FormsService == null) { FormsService = new FormsAuthenticationService(); }
			if (MembershipService == null) { MembershipService = new AccountMembershipService(); }

			base.Initialize(requestContext);
		}

		// **************************************
		// URL: /Account/LogOn
		// **************************************

		public ActionResult LogOn()
		{
			return RedirectToAction("Index", "Home");
		}

		[HttpPost]
		public ActionResult LogOn(LogOnModel model, string returnUrl)
		{
			if (ModelState.IsValid)
			{
				if (MembershipService.ValidateUser(model.UserName, model.Password))
				{
					FormsService.SignIn(model.UserName, model.RememberMe);
					if (!String.IsNullOrEmpty(returnUrl))
					{
						return Redirect(returnUrl);
					}
					return RedirectToAction("Index", "Home");
				}
				ModelState.AddModelError("", "The user name or password provided is incorrect.");
			}

			// If we got this far, something failed, redisplay form
			return RedirectToAction("Index", "Home");
		}

		// **************************************
		// URL: /Account/LogOff
		// **************************************

		public ActionResult LogOff()
		{
			FormsService.SignOut();

			return RedirectToAction("Index", "Home");
		}

		// **************************************
		// URL: /Account/Register
		// **************************************

		public ActionResult Register()
		{
			ViewData["PasswordLength"] = MembershipService.MinPasswordLength;
			return View();
		}

		[HttpPost]
		public ActionResult Register(RegisterModel model)
		{
			if (ModelState.IsValid)
			{
				// Attempt to register the user
				MembershipCreateStatus createStatus = MembershipService.CreateUser(model.UserName, model.Password, model.Email);
				switch(createStatus)
				{
					case MembershipCreateStatus.Success:
						FormsService.SignIn(model.UserName, false /* createPersistentCookie */);	
						return JsonReturn.Success();
					case MembershipCreateStatus.DuplicateEmail:
						return JsonReturn.Error("Email","Email already in use");
					case MembershipCreateStatus.DuplicateUserName:
						return JsonReturn.Error("UserName","Username already in use");
					case MembershipCreateStatus.InvalidEmail:
						return JsonReturn.Error("Email","Email is invalid");
					case MembershipCreateStatus.InvalidUserName:
						return JsonReturn.Error("UserName","Username is invalid");
					case MembershipCreateStatus.InvalidPassword:
						return JsonReturn.Error("Password","Password is invalid");	
				}
			}
			return JsonReturn.Error("","Please try again later.");
		}

		// **************************************
		// URL: /Account/ChangePassword
		// **************************************

		[Authorize]
		public ActionResult ChangePassword()
		{
			ViewData["PasswordLength"] = MembershipService.MinPasswordLength;
			return View();
		}

		[Authorize]
		[HttpPost]
		public ActionResult ChangePassword(ChangePasswordModel model)
		{
			if (ModelState.IsValid)
			{
				if (MembershipService.ChangePassword(User.Identity.Name, model.OldPassword, model.NewPassword))
				{
					return RedirectToAction("ChangePasswordSuccess");
				}
				ModelState.AddModelError("", "The current password is incorrect or the new password is invalid.");
			}

			// If we got this far, something failed, redisplay form
			ViewData["PasswordLength"] = MembershipService.MinPasswordLength;
			return View(model);
		}

		// **************************************
		// URL: /Account/ChangePasswordSuccess
		// **************************************

		public ActionResult ChangePasswordSuccess()
		{
			return View();
		}

	}
}
