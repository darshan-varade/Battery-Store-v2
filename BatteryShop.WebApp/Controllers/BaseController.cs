using System;
using System.Web.Mvc;

namespace BatteryShop.WebApp.Controllers
{
    public class BaseController : Controller
    {
        protected int CurrentOwnerId => Convert.ToInt32(Session["OwnerId"]);
        protected string CurrentRoleName => Session["RoleName"]?.ToString();
        protected bool IsAdmin => CurrentRoleName == "Admin";
    }
}
