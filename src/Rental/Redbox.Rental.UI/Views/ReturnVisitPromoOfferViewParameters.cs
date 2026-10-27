using System;
using Redbox.Rental.Model.KioskClientService.Transactions;

namespace Redbox.Rental.UI.Views;

public class ReturnVisitPromoOfferViewParameters
{
	public string PromoCode { get; set; }

	public ReturnPromoType PromoType { get; set; }

	public decimal? Amount { get; set; }

	public DateTime ExpirationDate { get; set; }

	public Action CloseAction { get; set; }
}
