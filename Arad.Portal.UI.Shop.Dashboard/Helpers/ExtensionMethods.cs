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

            if (userManager != null && userManager.Users.Any())
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
                        new IdentityUserClaim<string>()
                        {
                            ClaimType = "IsSystemAccount",
                            ClaimValue = true.ToString()
                        },
                        new IdentityUserClaim<string>()
                        {
                            ClaimType = "IsActive",
                            ClaimValue = true.ToString()
                        }
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
                    CreationDate = DateTime.UtcNow,
                    Modifications = new List<Modification>()
                };

                userManager.CreateAsync(user, "Sa@12345").Wait();
            }

           

            using var stateScope = app.ApplicationServices.CreateScope();
            var stateRepository =
                (RoleRepository)stateScope.ServiceProvider.GetService(typeof(IRoleRepository));

            if (!stateRepository.States.AsQueryable().Any())
            {
                using StreamReader r = new StreamReader(Path.Combine(applicationPath, "SeedData", "States.json"));
                string json = r.ReadToEnd();
                List<State> states = JsonConvert.DeserializeObject<List<State>>(json);

                if (states.Any())
                {
                    stateRepository.States.InsertMany(states);
                }
            }

            if (!stateRepository.Counties.AsQueryable().Any())
            {
                using StreamReader r = new StreamReader(Path.Combine(applicationPath, "SeedData", "States.json"));
                string json = r.ReadToEnd();
                List<County> counties = JsonConvert.DeserializeObject<List<County>>(json);

                if (counties.Any())
                {
                    stateRepository.Counties.InsertMany(counties);
                }
            }

            if (!stateRepository.Districts.AsQueryable().Any())
            {
                using StreamReader r = new StreamReader(Path.Combine(applicationPath, "SeedData", "States.json"));
                string json = r.ReadToEnd();
                List<District> districts = JsonConvert.DeserializeObject<List<District>>(json);

                if (districts.Any())
                {
                    stateRepository.Districts.InsertMany(districts);
                }
            }

            if (!stateRepository.Cities.AsQueryable().Any())
            {
                using StreamReader r = new StreamReader(Path.Combine(applicationPath, "SeedData", "States.json"));
                string json = r.ReadToEnd();
                List<City> cities = JsonConvert.DeserializeObject<List<City>>(json);

                if (cities.Any())
                {
                    stateRepository.Cities.InsertMany(cities);
                }
            }
        }
    }
}
