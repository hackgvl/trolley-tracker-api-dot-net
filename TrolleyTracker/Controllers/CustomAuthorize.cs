using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace TrolleyTracker.Controllers
{
    public class CustomAuthorize : AuthorizeAttribute
    {
        private static bool firstTime = true;

        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            if (!filterContext.HttpContext.User.Identity.IsAuthenticated)
            {
                filterContext.Result = new HttpUnauthorizedResult();
            }
            else
            {
                filterContext.Result = new RedirectToRouteResult(new
                    RouteValueDictionary(new { controller = "AccessDenied" }));
            }
        }


        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            if (!httpContext.Request.IsAuthenticated)
                return false;

            var rolesProvider = System.Web.Security.Roles.Providers["DBRoleProvider"];

            if (firstTime)
            {
                ValidateRoles(rolesProvider, httpContext.User.Identity.Name);
                firstTime = false;
            }

            string[] userRoles = rolesProvider.GetRolesForUser(httpContext.User.Identity.Name);

            var allowedRoles = SplitString(Roles);

            foreach(var allowedRole in allowedRoles)
            {
                foreach (var userRole in userRoles)
                {
                    if (allowedRole.Equals(userRole, StringComparison.CurrentCultureIgnoreCase))
                    {
                        return true;
                    }
                }

            }

            return false;
        }

        /// <summary>
        /// Check for and handle the case of a new database - if so, seed the database
        /// with the required roles for this application and make the currently logged-in
        /// user as the administrator.
        /// NOTE: Possible security hole when rebuilding a web site with a blank database - 
        /// anyone could register as the admin.   Double check the user list when rebuilding
        /// a blank user database.
        /// </summary>
        private void ValidateRoles(System.Web.Security.RoleProvider rolesProvider, string userName)
        {

            if (!rolesProvider.RoleExists("Administrators"))
            {
                rolesProvider.CreateRole("Administrators");
                rolesProvider.CreateRole("RouteManagers");
                rolesProvider.CreateRole("Vehicles");
                if (!rolesProvider.GetRolesForUser(userName).Contains("Administrators"))
                {
                    rolesProvider.AddUsersToRoles(new[] { userName }, new[] { "Administrators", "RouteManagers", "Vehicles"});
                }
            }
        }

        private string[] SplitString(string original)
        {
            if (String.IsNullOrEmpty(original))
            {
                return new string[0];
            }

            var split = from piece in original.Split(',')
                        let trimmed = piece.Trim()
                        where !String.IsNullOrEmpty(trimmed)
                        select trimmed;
            return split.ToArray();

        }


    }
}