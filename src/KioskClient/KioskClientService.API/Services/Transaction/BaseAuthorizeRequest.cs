using System;

namespace KioskClientService.API.Services.Transaction;

public class BaseAuthorizeRequest : MessageBase
{
	public DateTime TransactionDate { get; set; }

	public CreditCard CreditCard { get; set; }

	public ShoppingCart ShoppingCart { get; set; }

	public string CustomerProfileNumber { get; set; }

	public override void ScrubData()
	{
		base.ScrubData();
		CreditCard?.ScrubData();
	}
}
