using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;

namespace KreatoorsAI.WebAPI.Middlewares
{
#pragma warning disable

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class AuthorizeAttribute : Attribute, IAuthorizationFilter
    {
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            string token = (string)context.HttpContext.Items["Token"];
            if (token == null)
            {
                context.Result = new JsonResult(new { message = "Unauthorized" }) { StatusCode = StatusCodes.Status401Unauthorized };
            }
            else
            {
                long tokenExpiry = (long)context.HttpContext.Items["TokenExpirationDateInSeconds"];
                long currentTimeInSeconds = (long)context.HttpContext.Items["CurrentTimeInSconds"];
                if (currentTimeInSeconds > tokenExpiry)
                {
                    context.Result = new JsonResult(new { message = "Forbidden...you're using an expired token" }) { StatusCode = StatusCodes.Status403Forbidden }; ;
                }
            }
        }
    }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class AuthorizeRoleAttribute : Attribute, IAuthorizationFilter
    {
        public readonly string _role;
        public readonly string[] _roles;

        public AuthorizeRoleAttribute(string role)
        {
            _role = role;
        }

        public AuthorizeRoleAttribute(params string[] roles)
        {
            _roles = roles;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            string token = (string)context.HttpContext.Items["Token"];
            if (token == null)
            {
                context.Result = new JsonResult(new { message = "Unauthorized" }) { StatusCode = StatusCodes.Status401Unauthorized };
            }
            else
            {
                long tokenExpiry = (long)context.HttpContext.Items["TokenExpirationDateInSeconds"];
                long currentTimeInSeconds = (long)context.HttpContext.Items["CurrentTimeInSconds"];
                if (currentTimeInSeconds > tokenExpiry)
                {
                    context.Result = new JsonResult(new { message = "Forbidden...you're using an expired token" }) { StatusCode = StatusCodes.Status403Forbidden }; ;
                }
            }
        }

    }


}
