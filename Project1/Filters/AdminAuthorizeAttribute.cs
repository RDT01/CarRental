using System.Web;
using System.Web.Mvc;

namespace Project1.Filters // or whatever your namespace is
{
    public class AdminAuthorizeAttribute : AuthorizeAttribute
    {
        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            var role = httpContext.Session["Role"];
            return role != null && role.ToString() == "Admin";
        }

        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            filterContext.Result = new RedirectResult("~/Home/Index");
        }
    }
}
