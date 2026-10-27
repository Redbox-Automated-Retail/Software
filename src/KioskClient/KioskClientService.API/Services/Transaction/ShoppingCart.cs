using System.Collections.Generic;

namespace KioskClientService.API.Services.Transaction;

public class ShoppingCart
{
	public List<LineItemGroup> Groups { get; set; }

	public List<Discount> Discounts { get; set; }

	public ShoppingCart()
	{
		Groups = new List<LineItemGroup>();
		Discounts = new List<Discount>();
	}
}
