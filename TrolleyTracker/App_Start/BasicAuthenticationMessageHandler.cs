using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Security;
using TrolleyTracker.Controllers;
using TrolleyTracker.Models;

namespace TrolleyTracker.App_Start
{
    public class BasicAuthenticationMessageHandler : DelegatingHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var authHeader = request.Headers.Authorization;

            if (authHeader == null)
            {
                return base.SendAsync(request, cancellationToken);
            }

            if (authHeader.Scheme != "Basic")
            {
                return base.SendAsync(request, cancellationToken);
            }

            var encodedUserPass = authHeader.Parameter.Trim();
            var userPass = Encoding.ASCII.GetString(Convert.FromBase64String(encodedUserPass));
            var parts = userPass.Split(":".ToCharArray());
            var username = parts[0];
            var password = parts[1];


            var appManager = new ApplicationUserManager(new UserStore<ApplicationUser>(new ApplicationDbContext()), new EmailService());
            using (var signInManager = new ApplicationSignInManager(appManager,
                    HttpContext.Current.GetOwinContext().Authentication))
            {
                var result = signInManager.PasswordSignIn(username, password, true, false);
                if (result != SignInStatus.Success)
                {
                    // The SuppressFormsAuthenticationRedirect below has no effect currently; 302 is sent anyway
                    HttpContext.Current.Response.SuppressFormsAuthenticationRedirect = true;  // Send 401 code instead of 302 code on fail
                    return base.SendAsync(request, cancellationToken);
                }
            }

            //if (username != "Brigade" || password != "brigade")
            //{
            //    return base.SendAsync(request, cancellationToken);
            //}

            var identity = new GenericIdentity(username, "Basic");
            string[] roles = Roles.Provider.GetRolesForUser(username);
            //string[] roles = new string[1];
            var principal = new GenericPrincipal(identity, roles);
            Thread.CurrentPrincipal = principal;
            if (HttpContext.Current != null)
            {
                HttpContext.Current.User = principal;
            }

            return base.SendAsync(request, cancellationToken);
        }

        public class BasicAuthenticationAttribute : System.Web.Http.Filters.ActionFilterAttribute
        {
            public override void OnActionExecuting(System.Web.Http.Controllers.HttpActionContext actionContext)
            {
                if (actionContext.Request.Headers.Authorization == null)
                {
                    actionContext.Response = new System.Net.Http.HttpResponseMessage(System.Net.HttpStatusCode.Unauthorized);
                }
            }
        }
    }
}