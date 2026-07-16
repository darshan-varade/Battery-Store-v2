using System;
using System.Security.Claims;
using System.Web.Mvc;

namespace BatteryShop.WebApp.Controllers
{
    public class BaseController : Controller
    {
        protected int CurrentOwnerId
        {
            get
            {
                var claim = ((ClaimsPrincipal)User).FindFirst(ClaimTypes.NameIdentifier);
                return claim != null ? int.Parse(claim.Value) : 0;
            }
        }

        protected string CurrentRoleName
        {
            get
            {
                var claim = ((ClaimsPrincipal)User).FindFirst(ClaimTypes.Role);
                return claim?.Value;
            }
        }

        protected bool IsAdmin => User.IsInRole("Admin");

        protected ActionResult RequireAdmin()
        {
            if (!IsAdmin) return RedirectToAction("Index", "Home");
            return null;
        }
    }
}
