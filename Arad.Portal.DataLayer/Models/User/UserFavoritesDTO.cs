using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Arad.Portal.DataLayer.Entities.General.User;

namespace Arad.Portal.DataLayer.Models.User
{
    public class UserFavoritesDTO : UserFavorites
    {
        public string ImagePath { get; set; }

        public bool NoImage { get; set; }


        public string Name { get; set; }
    }
}
