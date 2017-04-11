using LNF.Cache;
using LNF.Data;
using LNF.Impl;
using LNF.Models.Data;
using Microsoft.Owin;
using Owin;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

[assembly: OwinStartup(typeof(CacheUtility.Startup))]

namespace CacheUtility
{
    public class Startup : OwinStartup
    {
        public override void ConfigureAuth(IAppBuilder app)
        {
            // lock down everything for devs ONLY!
            GlobalFilters.Filters.Add(new ClientPrivilegeAuthorizeAttribute(ClientPrivilege.Developer));
            base.ConfigureAuth(app);
        }

        public override void ConfigureRoutes(RouteCollection routes)
        {
            AreaRegistration.RegisterAllAreas();
            RouteConfig.RegisterRoutes(routes);
        }
    }

    public class ClientPrivilegeAuthorizeAttribute : AuthorizeAttribute
    {
        private ClientPrivilege _priv;

        public ClientPrivilegeAuthorizeAttribute(ClientPrivilege priv)
        {
            _priv = priv;
        }

        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            var user = httpContext.Request.RequestContext.HttpContext.User;

            if (user == null || user.Identity == null)
                return false;

            if (!user.Identity.IsAuthenticated)
                return false;

            var client = CacheManager.Current.GetClient(user.Identity.Name);

            if (client == null)
                return false;

            return client.HasPriv(_priv);
        }

        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            filterContext.HttpContext.Response.Clear();
            filterContext.HttpContext.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            filterContext.HttpContext.Response.ContentType = "text/plain";
            filterContext.HttpContext.Response.Write("401 unauthorized");
            filterContext.HttpContext.Response.End();
        }
    }
}