using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Security;
using TrolleyTracker.Models;

namespace TrolleyTracker.Controllers
{

    /// <summary>
    /// The purpose of this role provider is to support membership + roles in a MSSQL LocalDB.
    /// It works around the limitation of the standard SqlRoleProvider
    /// which requires stored procedures in a SQL Server database.
    /// </summary>
    public class CustomRoleProvider : RoleProvider
    {
        public override string ApplicationName { get; set; }


        public override string[] GetRolesForUser(string username)
        {
            using (var usersContext = new ApplicationDbContext())
            {
                return GetRolesForUser(usersContext, username);
            }
        }

        public string[] GetRolesForUser(ApplicationDbContext usersContext, string username)
        {
            var user = usersContext.Users
                        .Include(u => u.Roles)
                        .FirstOrDefault(u => u.UserName.Equals(username, StringComparison.CurrentCultureIgnoreCase)
                                             || u.Email.Equals(username, StringComparison.CurrentCultureIgnoreCase));

            if (user == null) return new string[] { };

            var roleIds = user.Roles.Select(r => r.RoleId);

            var roles = from r in usersContext.Roles
                        where roleIds.Contains(r.Id)
                        select r.Name.Trim();

            if (roles != null)
                return roles.ToArray();
            else
                return new string[] { };
        }


        public override void AddUsersToRoles(string[] usernames, string[] roleNames)
        {
            foreach (var userName in usernames)
            {
                var roles = GetRolesForUser(userName);
                foreach (var roleName in roleNames)
                {
                    if (!roles.Contains(roleName))
                    {
                        using (var usersContext = new ApplicationDbContext())
                        {
                            var user = usersContext.Users.FirstOrDefault(u => u.UserName.Equals(userName, StringComparison.CurrentCultureIgnoreCase));
                            if (user != null)
                            {
                                var role = usersContext.Roles.FirstOrDefault(r => r.Name.Equals(roleName, StringComparison.CurrentCultureIgnoreCase));
                                if (role != null)
                                {
                                    var userRole = new Microsoft.AspNet.Identity.EntityFramework.IdentityUserRole();
                                    userRole.RoleId = role.Id;
                                    userRole.UserId = user.Id;
                                    user.Roles.Add(userRole);
                                }
                            }
                            usersContext.SaveChanges();
                        }
                    }
                }
            }
        }


        public override string[] GetAllRoles()
        {
            using (var usersContext = new ApplicationDbContext())
            {
                return usersContext.Roles.Select(r => r.Name).ToArray();
            }
        }


        public override bool IsUserInRole(string username, string roleName)
        {
            return this.GetRolesForUser(username).Contains(roleName);
        }

        public override void CreateRole(string roleName)
        {
            var role = new Microsoft.AspNet.Identity.EntityFramework.IdentityRole();
            role.Id = Guid.NewGuid().ToString();
            role.Name = roleName;

            using (var usersContext = new ApplicationDbContext())
            {
                usersContext.Roles.Add(role);
                usersContext.SaveChanges();
            }

        }

        public override bool DeleteRole(string roleName, bool throwOnPopulatedRole)
        {
            var usersInRole = GetUsersInRole(roleName);
            if (throwOnPopulatedRole)
            {
                if (usersInRole.Length > 0)
                {
                    throw new Exception("Role " + roleName + " is not empty");
                }
            }

            var roleNameArray = new string[1];
            roleNameArray[0] = roleName;
            RemoveUsersFromRoles(usersInRole, roleNameArray);

            using (var usersContext = new ApplicationDbContext())
            {
                var role = usersContext.Roles.FirstOrDefault(r => r.Name.Equals(roleName, StringComparison.CurrentCultureIgnoreCase));
                if (role != null)
                {
                    usersContext.Roles.Remove(role);
                    usersContext.SaveChanges();
                    return true;
                }
                return false;
            }

        }

        public override bool RoleExists(string roleName)
        {
            using (var usersContext = new ApplicationDbContext())
            {
                var role = usersContext.Roles.FirstOrDefault(r => r.Name.Equals(roleName, StringComparison.CurrentCultureIgnoreCase));
                return (role != null);
            }
        }


        public override void RemoveUsersFromRoles(string[] usernames, string[] roleNames)
        {

            foreach (var userName in usernames)
            {
                var roles = GetRolesForUser(userName);
                foreach (var roleName in roleNames)
                {
                    if (!roles.Contains(roleName))
                    {
                        using (var usersContext = new ApplicationDbContext())
                        {
                            var user = usersContext.Users.FirstOrDefault(u => u.UserName.Equals(userName, StringComparison.CurrentCultureIgnoreCase));
                            if (user != null)
                            {
                                var role = usersContext.Roles.FirstOrDefault(r => r.Name.Equals(roleName, StringComparison.CurrentCultureIgnoreCase));
                                if (role != null)
                                {
                                    var userRole = new Microsoft.AspNet.Identity.EntityFramework.IdentityUserRole();
                                    userRole.RoleId = role.Id;
                                    userRole.UserId = user.Id;
                                    user.Roles.Remove(userRole);
                                }
                            }
                            usersContext.SaveChanges();
                        }
                    }
                }
            }
        }

        public override string[] GetUsersInRole(string roleName)
        {
            using (var usersContext = new ApplicationDbContext())
            {
                var role = usersContext.Roles
                                  .Include(r => r.Users)
                                  .FirstOrDefault(r => r.Name.Equals(roleName, StringComparison.CurrentCultureIgnoreCase));

                if (role == null) return new string[] { };

                string[] userIds = role.Users.Select(u => u.UserId).ToArray();

                var users = from u in usersContext.Users
                            where userIds.Contains(u.Id)
                            select u.UserName.Trim();

                if (users != null)
                    return users.ToArray();
                else
                    return new string[] { };
            }
        }


        public override string[] FindUsersInRole(string roleName, string usernameToMatch)
        {
            using (var usersContext = new ApplicationDbContext())
            {
                return FindUsersInRole(usersContext, roleName, usernameToMatch);
            }
        }

        public string[] FindUsersInRole(ApplicationDbContext usersContext, string roleName, string usernameToMatch)
        {
            var regEx = new System.Text.RegularExpressions.Regex(usernameToMatch);

            var role = usersContext.Roles
                                   .Include(r => r.Users)
                                   .FirstOrDefault(r => r.Name.Equals(roleName, StringComparison.CurrentCultureIgnoreCase));

            if (role == null) return new string[] { };

            // First get the possible users from the database
            string[] userIds = role.Users.Select(u => u.UserId).ToArray();
            IEnumerable<string> usernames = usersContext.Users.Where(u => userIds.Contains(u.Id)).Select(u => u.UserName).ToArray();

            // Then filter the usernames with the name to match
            usernames = usernames.Where(u => regEx.IsMatch(u)).Select(u => u.Trim());

            if (usernames != null)
                return usernames.ToArray();
            else
                return new string[] { };
        }
    }

}