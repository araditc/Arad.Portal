using Arad.Portal.DataLayer.Contracts.General.Permission;
using Arad.Portal.DataLayer.Contracts.General.Role;
using Arad.Portal.DataLayer.Entities;
using Arad.Portal.DataLayer.Entities.General.User;
using Arad.Portal.DataLayer.Models.User;
using Arad.Portal.DataLayer.Repositories.General.Permission.Mongo;
using Arad.Portal.DataLayer.Repositories.General.Role.Mongo;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using Newtonsoft.Json;
using Arad.Portal.DataLayer.Entities.General.City;
using Arad.Portal.DataLayer.Entities.General.State;
using Arad.Portal.DataLayer.Entities.General.County;
using Arad.Portal.DataLayer.Entities.General.District;
using Arad.Portal.DataLayer.Entities.General.Permission;
using Arad.Portal.DataLayer.Models.Role;
using Arad.Portal.DataLayer.Entities.General.Role;

namespace Arad.Portal.UI.Shop.Dashboard.Helpers
{
    public static class ExtensionMethods
    {
        public static void UseSeedDatabase(this IApplicationBuilder app, string applicationPath)
        {
            using var userScope = app.ApplicationServices.CreateScope();
            var userManager =
                (UserManager<ApplicationUser>)userScope.ServiceProvider
                    .GetService(typeof(UserManager<ApplicationUser>));

            if (userManager != null && !userManager.Users.Any())
            {
                var user = new ApplicationUser()
                {
                    Id = Guid.NewGuid().ToString(),
                    UserName = "SuperAdmin",
                    Claims = new List<IdentityUserClaim<string>>()
                    {
                        new IdentityUserClaim<string>()
                        {
                            ClaimType = ClaimTypes.GivenName,
                            ClaimValue = "ادمین شماره یک"
                        },
                        //new IdentityUserClaim<string>()
                        //{
                        //    ClaimType = "IsSystemAccount",
                        //    ClaimValue = true.ToString()
                        //},
                        //new IdentityUserClaim<string>()
                        //{
                        //    ClaimType = "IsActive",
                        //    ClaimValue = true.ToString()
                        //}
                    },
                    PhoneNumber = "989309910790",
                    IsActive = true,
                    IsSystemAccount = true,
                    Profile = new Profile()
                    {
                        Gender = Gender.Women,
                        FatherName = "نام پدر",
                        FirstName = "ادمین",
                        LastName = "شماره یک",
                        BirthDate = new DateTime(1987, 8, 26).ToUniversalTime(),
                        NationalId = "1292086734",
                        CompanyName = "Arad",
                        UserType = UserType.Admin
                    },
                    Addresses = new List<Address>(),
                    CreationDate = DateTime.UtcNow
                };

                userManager.CreateAsync(user, "Sa@12345").Wait();
            }

            using var permissionScope = app.ApplicationServices.CreateScope();
            var permissionRepository =
                (PermissionRepository)permissionScope.ServiceProvider.GetService(typeof(IPermissionRepository));

            if (!permissionRepository.HasAny())
            {
                using StreamReader r = new StreamReader(Path.Combine(applicationPath, "SeedData", "permissions.json"));
                string json = r.ReadToEnd();
                List<Permission> items = JsonConvert.DeserializeObject<List<Permission>>(json);

                if (items.Any())
                {
                    permissionRepository.InsertMany(items).Wait();
                }
            }

            using var stateScope = app.ApplicationServices.CreateScope();
            var roleRepository =
                (RoleRepository)stateScope.ServiceProvider.GetService(typeof(IRoleRepository));

            if (!roleRepository.States.AsQueryable().Any())
            {
                using StreamReader r = new StreamReader(Path.Combine(applicationPath, "SeedData", "States.json"));
                string json = r.ReadToEnd();
                List<State> states = JsonConvert.DeserializeObject<List<State>>(json);

                if (states.Any())
                {
                    roleRepository.States.InsertMany(states);
                }
            }

            if (!roleRepository.Counties.AsQueryable().Any())
            {
                using StreamReader r = new StreamReader(Path.Combine(applicationPath, "SeedData", "States.json"));
                string json = r.ReadToEnd();
                List<County> counties = JsonConvert.DeserializeObject<List<County>>(json);

                if (counties.Any())
                {
                    roleRepository.Counties.InsertMany(counties);
                }
            }

            if (!roleRepository.Districts.AsQueryable().Any())
            {
                using StreamReader r = new StreamReader(Path.Combine(applicationPath, "SeedData", "States.json"));
                string json = r.ReadToEnd();
                List<District> districts = JsonConvert.DeserializeObject<List<District>>(json);

                if (districts.Any())
                {
                    roleRepository.Districts.InsertMany(districts);
                }
            }

            if (!roleRepository.Cities.AsQueryable().Any())
            {
                using StreamReader r = new StreamReader(Path.Combine(applicationPath, "SeedData", "States.json"));
                string json = r.ReadToEnd();
                List<City> cities = JsonConvert.DeserializeObject<List<City>>(json);

                if (cities.Any())
                {
                    roleRepository.Cities.InsertMany(cities);
                }
            }

            if (!roleRepository.HasAny())
            {
                var role = new Role()
                {
                    RoleId = Guid.NewGuid().ToString(),
                    RoleName = "سوپر ادمین",
                    CreationDate = DateTime.UtcNow,
                    IsActive = true,
                    PermissionIds = permissionRepository.GetAllPermissionIds()
                };
                var role2 = new Role()
                {
                    RoleId = Guid.NewGuid().ToString(),
                    RoleName = "مدیریت کاربران",
                    CreationDate = DateTime.UtcNow,
                    IsActive = true
                };
                List<Role> items = new List<Role>();
                items.Add(role);
                items.Add(role2);

                if (items.Any())
                {
                    roleRepository.InsertMany(items).Wait();
                }
            }
        }
    }
}
