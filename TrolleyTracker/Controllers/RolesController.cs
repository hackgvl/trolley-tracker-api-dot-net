using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TrolleyTracker.Models;
using Microsoft.AspNet.Identity;
using NLog;

namespace TrolleyTracker.Controllers
{
    public class RolesController : Controller
    {
        ApplicationDbContext context = new ApplicationDbContext();

        private static Logger logger = LogManager.GetCurrentClassLogger();

        //
        // GET: /Roles/
        [CustomAuthorize(Roles = "Administrators")]
        public ActionResult Index()
        {
            var roles = context.Roles.ToList();
            return View(roles);
        }

        //
        // GET: /Roles/Create
        [CustomAuthorize(Roles = "Administrators")]
        public ActionResult Create()
        {
            return View();
        }

        //
        // POST: /Roles/Create
        [CustomAuthorize(Roles = "Administrators")]
        [HttpPost]
        public ActionResult Create(FormCollection collection)
        {
            try
            {
                var roleName = collection["RoleName"].Trim();
                context.Roles.Add(new Microsoft.AspNet.Identity.EntityFramework.IdentityRole()
                {
                    Name = roleName
                });
                context.SaveChanges();
                ViewBag.ResultMessage = "Role created successfully !";

                logger.Info($"Created Role '{roleName}'");

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        //
        // GET: /Roles/Edit/5
        [CustomAuthorize(Roles = "Administrators")]
        public ActionResult Edit(string roleName)
        {
            var thisRole = context.Roles.Where(r => r.Name.Equals(roleName, StringComparison.CurrentCultureIgnoreCase)).FirstOrDefault();

            return View(thisRole);
        }

        //
        // POST: /Roles/Edit/5
        [CustomAuthorize(Roles = "Administrators")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Microsoft.AspNet.Identity.EntityFramework.IdentityRole role)
        {
            try
            {
                context.Entry(role).State = System.Data.Entity.EntityState.Modified;
                context.SaveChanges();

                logger.Info($"Changed Role '{role.Name}'");

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        //
        // GET: /Roles/Delete/5
        [CustomAuthorize(Roles = "Administrators")]
        public ActionResult Delete(string RoleName)
        {
            var thisRole = context.Roles.Where(r => r.Name.Equals(RoleName, StringComparison.CurrentCultureIgnoreCase)).FirstOrDefault();
            logger.Info($"Deleted Role '{thisRole.Name}'");
            context.Roles.Remove(thisRole);
            context.SaveChanges();
            return RedirectToAction("Index");
        }



        [CustomAuthorize(Roles = "Administrators")]
        public ActionResult ManageUserRoles()
        {
            var list = context.Roles.OrderBy(r => r.Name).ToList().Select(rr => new SelectListItem { Value = rr.Name.ToString(), Text = rr.Name }).ToList();
            ViewBag.Roles = list;
            var userNames = context.Users.OrderBy(u => u.UserName).ToList().Select(un => new SelectListItem { Value = un.UserName.ToString(), Text = un.UserName }).ToList();
            ViewBag.UserNames = userNames;
            return View();
        }

        [CustomAuthorize(Roles = "Administrators")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult RoleAddToUser(string UserName, string RoleName)
        {
            ApplicationUser user = context.Users.Where(u => u.UserName.Equals(UserName, StringComparison.CurrentCultureIgnoreCase)).FirstOrDefault();
            var account = new AccountController();
            account.UserManager.AddToRole(user.Id, RoleName);

            logger.Info($"Added user {UserName} to role {RoleName}");
            ViewBag.ResultMessage = "Role created successfully !";

            // prepopulat roles for the view dropdown
            var list = context.Roles.OrderBy(r => r.Name).ToList().Select(rr => new SelectListItem { Value = rr.Name.ToString(), Text = rr.Name }).ToList();
            ViewBag.Roles = list;
            var userNames = context.Users.OrderBy(u => u.UserName).ToList().Select(un => new SelectListItem { Value = un.UserName.ToString(), Text = un.UserName }).ToList();
            ViewBag.UserNames = userNames;

            if (user != null)
            {
                ViewBag.RolesForThisUser = account.UserManager.GetRoles(user.Id);
            }

            return View("ManageUserRoles");
        }

        [CustomAuthorize(Roles = "Administrators")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult GetRoles(string UserName)
        {
            if (!string.IsNullOrWhiteSpace(UserName))
            {
                ApplicationUser user = context.Users.Where(u => u.UserName.Equals(UserName, StringComparison.CurrentCultureIgnoreCase)).FirstOrDefault();
                var account = new AccountController();
                if (user != null)
                {
                    ViewBag.RolesForThisUser = account.UserManager.GetRoles(user.Id);
                }

            }
            // Prepopulate roles for the view dropdown
            var list = context.Roles.OrderBy(r => r.Name).ToList().Select(rr => new SelectListItem { Value = rr.Name.ToString(), Text = rr.Name }).ToList();
            ViewBag.Roles = list;
            var userNames = context.Users.OrderBy(u => u.UserName).ToList().Select(un => new SelectListItem { Value = un.UserName.ToString(), Text = un.UserName }).ToList();
            ViewBag.UserNames = userNames;

            return View("ManageUserRoles");
        }

        [CustomAuthorize(Roles = "Administrators")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteRoleForUser(string UserName, string RoleName)
        {
            var account = new AccountController();
            ApplicationUser user = context.Users.Where(u => u.UserName.Equals(UserName, StringComparison.CurrentCultureIgnoreCase)).FirstOrDefault();

            if (account.UserManager.IsInRole(user.Id, RoleName))
            {
                account.UserManager.RemoveFromRole(user.Id, RoleName);
                ViewBag.ResultMessage = "Role removed from this user successfully !";

                logger.Info($"Removed user {UserName} from role {RoleName}");

            }
            else
            {
                ViewBag.ResultMessage = "This user doesn't belong to selected role.";
            }

            if (user != null)
            {
                ViewBag.RolesForThisUser = account.UserManager.GetRoles(user.Id);
            }

            // prepopulat roles for the view dropdown
            var list = context.Roles.OrderBy(r => r.Name).ToList().Select(rr => new SelectListItem { Value = rr.Name.ToString(), Text = rr.Name }).ToList();
            ViewBag.Roles = list;
            var userNames = context.Users.OrderBy(u => u.UserName).ToList().Select(un => new SelectListItem { Value = un.UserName.ToString(), Text = un.UserName }).ToList();
            ViewBag.UserNames = userNames;

            return View("ManageUserRoles");
        }
    }
}
