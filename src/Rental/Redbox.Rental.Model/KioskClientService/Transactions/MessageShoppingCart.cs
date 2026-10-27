using System.Collections.Generic;

namespace Redbox.Rental.Model.KioskClientService.Transactions;

public class MessageShoppingCart
{
	public List<LineItemGroup> Groups { get; set; } = new List<LineItemGroup>();

	public List<Discount> Discounts { get; set; } = new List<Discount>();

	public ServiceFee ServiceFee { get; set; } = new ServiceFee();
}
