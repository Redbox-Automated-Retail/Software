using System.Collections.Generic;

namespace Redbox.KioskServices.Proxy.ComponentModel.Loyalty;

public class ShoppingCart
{
	public List<LineItemGroup> Groups { get; set; }

	public List<Discount> Discounts { get; set; }

	public ServiceFee ServiceFee { get; set; }

	public ShoppingCart()
	{
		Groups = new List<LineItemGroup>();
		Discounts = new List<Discount>();
		ServiceFee = new ServiceFee();
	}
}
