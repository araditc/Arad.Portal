using Arad.Portal.DataLayer.Entities.General.User;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arad.Portal.DataLayer.Repositories.General.User.Mongo
{
    //???
    //this part should be added userContext and userRepository
    //should be removed and extension methods should be added
    public  class UserExtensions
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public UserExtensions(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }
    }
}
