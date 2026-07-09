using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace BatteryShop.WebApp
{
    public enum Role
    {
        Admin,
        Owner
    }

    public class AuthorizeRoleAttribute : AuthorizeAttribute
    {
        private readonly Role _requiredRole;

        public AuthorizeRoleAttribute(Role role)
        {
            _requiredRole = role;
        }

        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            if (!base.AuthorizeCore(httpContext)) return false;
            var roleName = httpContext.Session?["RoleName"]?.ToString();
            return roleName == _requiredRole.ToString();
        }

        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            if (filterContext.HttpContext.Request.IsAjaxRequest())
            {
                filterContext.Result = new JsonResult
                {
                    Data = new { success = false, message = "Unauthorized" },
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet
                };
            }
            else
            {
                filterContext.Result = new RedirectToRouteResult(
                    new RouteValueDictionary { { "controller", "Home" }, { "action", "Index" } });
            }
        }
    }
}
