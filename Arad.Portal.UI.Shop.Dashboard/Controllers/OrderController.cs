using Arad.Portal.DataLayer.Contracts.General.Domain;
using Arad.Portal.DataLayer.Contracts.Shop.Transaction;
using Arad.Portal.DataLayer.Entities.General.User;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Arad.Portal.UI.Shop.Dashboard.Controllers
{
    [Authorize(Policy = "Role")]
    public class OrderController : Controller
    {

        private readonly ITransactionRepository _transactionRepository;
        private readonly IDomainRepository _domainRepository;
        private readonly UserManager<ApplicationUser> _userManager;
        public OrderController(ITransactionRepository transactionRepository, IDomainRepository domainRepository, UserManager<ApplicationUser> userManager)
        {
            _transactionRepository = transactionRepository;
            _domainRepository = domainRepository;
            _userManager = userManager;
        }

        //public async Task<IActionResult> List()
        //{
            
        //}
    }
}
