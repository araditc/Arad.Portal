﻿using Arad.Portal.DataLayer.Entities.General.User;
using Arad.Portal.DataLayer.Models.Menu;
using Arad.Portal.DataLayer.Models.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arad.Portal.DataLayer.Contracts.General.Menu
{
    public interface IMenuRepository
    {
        Task<Result> AddMenu(MenuDTO dto);
        Task<Result> EditMenu(MenuDTO dto);
        /// <summary>
        /// if langId is null or empty then content will show in default languageId of domain
        /// </summary>
        /// <param name="domainId"></param>
        /// <param name="langId"></param>
        /// <returns></returns>
        List<StoreMenuVM> StoreList(string domainId, string langId, bool isDesign);
        Task<List<SelectListModel>> AllActiveMenues(string domainId, string langId);

        List<SelectListModel> MenuesOfSelectedDomain(string domainId, string langId);
        Task<PagedItems<MenuDTO>> AdminList(string queryString, ApplicationUser user);
         Task<Result> DeleteMenu(string menuId);
        Task<Result> RestoreMenu(string menuId);
        Result<MenuDTO> FetchMenu(string menuId);
         List<SelectListModel> GetAllMenuType();
         StoreMenuVM GetByCode(long menuCode);

        void InsertMany(List<Entities.General.Menu.Menu> menus);
    }
}
