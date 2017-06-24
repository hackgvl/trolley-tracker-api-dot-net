using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using TrolleyTracker.Controllers;
using TrolleyTracker.Models;
using UnitTests.EntityFramework;

namespace UnitTests
{
    [TestClass]
    public class DatabaseEfficiencyTests
    {
        private static ApplicationUser testUserA;
        private static ApplicationUser testUserB;

        private static IdentityRole testRoleX;
        private static IdentityRole testRoleY;

        [ClassInitialize]
        public static void ClassInit(TestContext context)
        {
            // Add test users and roles to the database for the 
            // queries below, each of these test cases has potential
            // scaling issues as the number of roles and users increase.
            // This will also make sure the database is initialized
            // to make the query counting accurate in the tests.
            using (var usersContext = new ApplicationDbContext())
            {
                var passwordHash = new PasswordHasher();
                string password = passwordHash.HashPassword(Guid.NewGuid().ToString("N"));

                testUserA = new ApplicationUser
                {
                    UserName = Guid.NewGuid().ToString("N"),
                    PasswordHash = password
                };
                usersContext.Users.Add(testUserA);

                testUserB = new ApplicationUser
                {
                    UserName = Guid.NewGuid().ToString("N"),
                    PasswordHash = password
                };
                usersContext.Users.Add(testUserB);


                testRoleX = new IdentityRole(Guid.NewGuid().ToString("N"));
                usersContext.Roles.Add(testRoleX);

                testRoleY = new IdentityRole(Guid.NewGuid().ToString("N"));
                usersContext.Roles.Add(testRoleY);

                usersContext.SaveChanges();

                // Add the testUser to the role
                testUserA.Roles.Add(
                    new IdentityUserRole
                    {
                        RoleId = testRoleX.Id
                    });

                testUserB.Roles.Add(
                    new IdentityUserRole
                    {
                        RoleId = testRoleX.Id
                    });
                testUserB.Roles.Add(
                    new IdentityUserRole
                    {
                        RoleId = testRoleY.Id
                    });
                usersContext.SaveChanges();
            }
        }

        [ClassCleanup]
        public static void ClassCleanup()
        {
            // Clean up the test user
            using (var usersContext = new ApplicationDbContext())
            {
                usersContext.Users.Remove(testUserA);
                usersContext.Users.Remove(testUserB);
                usersContext.Roles.Remove(testRoleX);
                usersContext.Roles.Remove(testRoleY);
                usersContext.SaveChanges();
            }
        }

        [TestMethod]
        public void TestGetRolesForUserQueryCount()
        {
            var roleProvider = new CustomRoleProvider();
            var queryCount = new EntityFrameworkActivityLogger();

            using (var usersContext = new ApplicationDbContext())
            using (new WithInterception(queryCount))
            {
                roleProvider.GetRolesForUser(usersContext, testUserA.UserName);

                // We expect 1 query to get the user, and 1 query to get the role names
                Assert.AreEqual(2, queryCount.TotalExecutedCount,
                    "The query count for CustomRoleProvider::GetRolesForUser exceeded the expected number.");

                queryCount.Reset();

                roleProvider.GetRolesForUser(usersContext, testUserB.UserName);

                // The query count should be the same, regardless of the number of roles a user is a member of
                Assert.AreEqual(2, queryCount.TotalExecutedCount,
                    "The query count for CustomRoleProvider::GetRolesForUser exceeded the expected number.");
            }
        }

        [TestMethod]
        public void TestFindUsersInRoleQueryCount()
        {
            var roleProvider = new CustomRoleProvider();
            var queryCount = new EntityFrameworkActivityLogger();

            using (var usersContext = new ApplicationDbContext())
            using (new WithInterception(queryCount))
            {
                roleProvider.FindUsersInRole(usersContext, testRoleX.Name, "");

                Assert.AreEqual(2, queryCount.TotalExecutedCount,
                    "The query count for CustomRoleProvider::FindUsersInRole exceeded the expected number.");

                queryCount.Reset();

                roleProvider.FindUsersInRole(usersContext, testRoleX.Name, testUserA.UserName);

                Assert.AreEqual(2, queryCount.TotalExecutedCount,
                    "The query count for CustomRoleProvider::FindUsersInRole exceeded the expected number.");
            }
        }
    }
}
