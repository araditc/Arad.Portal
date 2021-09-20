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
        Task<RepositoryOperationResult> AddMenu(MenuDTO dto);
        Task<PagedItems<MenuLinkModel>> AllShopMenuList(string domainId);
        Task<RepositoryOperationResult> DeleteMenu(string menuId);
        RepositoryOperationResult<MenuDTO> FetchMenu(string menuId);
    }
}
