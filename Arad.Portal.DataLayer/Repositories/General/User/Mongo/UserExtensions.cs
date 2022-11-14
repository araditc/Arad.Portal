﻿using Arad.Portal.DataLayer.Entities.General.User;
using Arad.Portal.DataLayer.Models.Shared;
using Arad.Portal.DataLayer.Models.User;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arad.Portal.DataLayer.Repositories.General.User.Mongo
{
    
    public  class UserExtensions
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public UserExtensions(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public PagedItems<UserDTO> UserFilters(UserSearchParams search, string currentUserId)
        {
            var result = new PagedItems<UserDTO>();
            try
            {
                var users = _userManager.Users.Where(_ => !_.IsSystemAccount & _.Id != currentUserId);

                if (!string.IsNullOrEmpty(search.UserName))
                {
                    users = users.Where(_ => _.UserName.Contains(search.UserName.Trim()));
                }

                if (!string.IsNullOrWhiteSpace(search.Name))
                {
                    users = users.Where(c => c.Profile.FirstName.Contains(search.Name.Trim()));
                }

                if (!string.IsNullOrWhiteSpace(search.LastName))
                {
                    users = users.Where(_ => _.Profile.LastName.Contains(search.LastName.Trim()));
                }

                if (!string.IsNullOrWhiteSpace(search.PhoneNumber))
                {
                    users = users.Where(_ => _.PhoneNumber.Contains(search.PhoneNumber.Trim()));
                }

                if(search.IsActive != null)
                {
                    users = users.Where(_ => _.IsActive == search.IsActive);
                }
                if (search.StartRegisterDate != null)
                {
                   
                    users = users.Where(_ => _.CreationDate >= search.StartRegisterDate.Value.ToUniversalTime());
                }

                if (search.EndRegisterDate != null)
                {
                    
                    users = users.Where(_ => _.CreationDate < search.EndRegisterDate.Value.ToUniversalTime());
                }
                if (!string.IsNullOrWhiteSpace(search.UserRoleId))
                {
                    users = users.Where(c => c.UserRoleId == search.UserRoleId);
                }

                var count = users.Count();

                users = users.Skip((search.CurrentPage - 1) * search.PageSize)
                    .Take(search.PageSize);

                result = new PagedItems<UserDTO>()
                {
                    CurrentPage = search.CurrentPage,
                    ItemsCount = count,
                    PageSize = search.PageSize,
                    Items = users.Select(_ => new UserDTO()
                    {
                        UserId = _.Id.ToString(),
                        UserName = _.UserName,
                        UserProfile = _.Profile,
                        PhoneNumber =_.PhoneNumber,
                        IsActive = _.IsActive,
                        CreationDate = _.CreationDate,
                        IsDeleted = _.IsDeleted
                    }).ToList()
                };

            }
            catch (Exception ex)
            {
            }
            return result;
        }


        public ApplicationUser GetUsersByPhoneNumber(string phoneNumber)
        {
            try
            {
                var user = _userManager.Users
                    .Any(_ => _.PhoneNumber == phoneNumber && !_.IsDeleted) ? _userManager.Users
                    .FirstOrDefault(_ => _.PhoneNumber == phoneNumber && !_.IsDeleted) : null;

                return user;
            }
            catch (Exception e)
            {
                return null;
            }
        }

        public bool IsPhoneNumberUnique(string modelPhone, string userPhone)
        {
            bool result = false;
            try
            {
             
                var user = _userManager.Users.Where(u => u.PhoneNumber == modelPhone).ToList();

                var count = user.Count;

                if (count == 0)
                {
                    user = _userManager.Users.Where(_ => _.UserName == modelPhone).ToList();
                    count = user.Count;
                    if (user.Count == 0)
                    {
                        result = true;
                    }
                }

                if (count == 1)
                {
                    if (modelPhone == userPhone)
                    {
                        result = true;
                    }
                }
            }
            catch (Exception e)
            {
                result = false;
            }
            return result;
        }
    }
}
