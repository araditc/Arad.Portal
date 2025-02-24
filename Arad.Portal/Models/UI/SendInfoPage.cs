using Arad.Portal.DataLayer.Models.Shared.User;

using System.Collections.Generic;

namespace Arad.Portal.Models.UI;

public class SendInfoPage
{
    public List<Address> Addresses { get; init; }
    public string TotalCost { get; init; }

    public string CurrencySymbol { get; init; }

    public string UserCartId { get; init; }
}