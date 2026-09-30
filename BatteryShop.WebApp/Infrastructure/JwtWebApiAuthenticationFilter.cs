using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;

namespace BatteryShop.WebApp.Infrastructure
{
    public class JwtWebApiAuthenticationFilter : AuthorizationFilterAttribute
    {
        public override void OnAuthorization(HttpActionContext actionContext)
        {
            string authorizationHeader =
                actionContext.Request.Headers.Authorization?.ToString();

            if (string.IsNullOrEmpty(authorizationHeader))
            {
                actionContext.Response =
                    actionContext.Request.CreateResponse(
                        HttpStatusCode.Unauthorized,
                        new
                        {
                            success = false,
                            message = "Authorization header is missing."
                        });

                return;
            }

            if (!authorizationHeader.StartsWith("Bearer "))
            {
                actionContext.Response =
                    actionContext.Request.CreateResponse(
                        HttpStatusCode.Unauthorized,
                        new
                        {
                            success = false,
                            message = "Bearer token is required."
                        });

                return;
            }

            string token =
                authorizationHeader.Substring("Bearer ".Length).Trim();

            ClaimsPrincipal principal =
                JwtHelper.ValidateAccessToken(token);

            if (principal == null)
            {
                actionContext.Response =
                    actionContext.Request.CreateResponse(
                        HttpStatusCode.Unauthorized,
                        new
                        {
                            success = false,
                            message = "Invalid or expired access token."
                        });

                return;
            }
            
            actionContext.RequestContext.Principal = principal;
        }
    }
}