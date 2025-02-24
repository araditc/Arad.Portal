namespace Arad.Portal.Models.UI;

public class PaymentModel
{
    public string Address { get; set; }
    public string UserCartId { get; set; }

    public string PspType { get; set; }

    public int PspTypeId { get; set; }
}