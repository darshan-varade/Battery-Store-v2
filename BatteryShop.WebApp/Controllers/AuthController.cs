using System;
using System.Web.Mvc;
using System.Web.Security;
using BatteryShop.DataAccess.DAL;
using BatteryShop.DataAccess.ViewModels;

namespace BatteryShop.WebApp.Controllers
{
    [AllowAnonymous]
    public class AuthController : Controller
    {
        [HttpGet]
        public ActionResult Login()
        {
            if (User.Identity.IsAuthenticated)
                return RedirectToAction("Index", "Home");

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(LoginViewModel vm)
        {
            if (!ModelState.IsValid)
                return View(vm);

            try
            {
                AuthDAL dal = new AuthDAL();
                var owner = dal.OwnerLogin(vm.Email);

                if (owner == null || !BCrypt.Net.BCrypt.Verify(vm.Password, owner.PasswordHash))
                {
                    ModelState.AddModelError("", "Invalid email or password.");
                    return View(vm);
                }

                FormsAuthentication.SetAuthCookie(owner.OwnerEmail, vm.RememberMe);

                Session["OwnerId"] = owner.OwnerId;
                Session["OwnerName"] = owner.OwnerName;
                Session["OwnerEmail"] = owner.OwnerEmail;
                Session["RoleName"] = owner.RoleName;

                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                Serilog.Log.Error(ex, "Error in Login POST");
                ModelState.AddModelError("", "An error occurred. Please try again.");
                return View(vm);
            }
        }

        [HttpGet]
        public ActionResult Signup()
        {
            if (User.Identity.IsAuthenticated)
                return RedirectToAction("Index", "Home");

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Signup(SignupViewModel vm)
        {
            if (!ModelState.IsValid)
                return View(vm);

            try
            {
                AuthDAL dal = new AuthDAL();

                if (dal.OwnerCheckEmail(vm.OwnerEmail))
                {
                    ModelState.AddModelError("OwnerEmail", "This email is already registered.");
                    return View(vm);
                }

                string passwordHash = BCrypt.Net.BCrypt.HashPassword(vm.Password);

                dal.OwnerRegister(vm.OwnerName, vm.OwnerPhone, vm.OwnerEmail, passwordHash);

                TempData["info"] = "Account created successfully! Please login.";
                return RedirectToAction("Login");
            }
            catch (Exception ex)
            {
                Serilog.Log.Error(ex, "Error in Signup POST");
                ModelState.AddModelError("", "An error occurred. Please try again.");
                return View(vm);
            }
        }

        [HttpGet]
        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            Session.Clear();
            return RedirectToAction("Login");
        }
    }
}
