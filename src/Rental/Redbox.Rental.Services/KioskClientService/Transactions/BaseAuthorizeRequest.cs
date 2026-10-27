using System;
using Redbox.Rental.Model.KioskClientService.Transactions;

namespace Redbox.Rental.Services.KioskClientService.Transactions;

public class BaseAuthorizeRequest : MessageBase
{
	public DateTime TransactionDate { get; set; }

	public CreditCard CreditCard { get; set; }

	public MessageShoppingCart ShoppingCart { get; set; }

	public string CustomerProfileNumber { get; set; }

	public override void ScrubData()
	{
		base.ScrubData();
		CreditCard?.ScrubData();
	}
}
