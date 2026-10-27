namespace Redbox.Rental.Model.KioskClientService.Transactions;

public class CartMessage : MessageBase
{
	public string CustomerProfileNumber { get; set; }

	public MessageShoppingCart ShoppingCart { get; set; }

	public CartMessage()
	{
		ShoppingCart = new MessageShoppingCart();
	}
}
