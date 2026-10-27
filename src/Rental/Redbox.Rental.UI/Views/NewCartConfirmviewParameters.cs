using System;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Rental.Model.KioskProduct;
using Redbox.Rental.Model.Promotion;
using Redbox.Rental.Model.ShoppingCart;

namespace Redbox.Rental.UI.Views;

public class NewCartConfirmviewParameters : IViewAnalyticsName
{
	public string ViewAnalyticsName { get; set; }

	public IRentalShoppingCart NewCart { get; set; }

	public ITitleProduct promoProduct { get; set; }

	public Action CancelAction { get; set; }

	public Action ContinueAction { get; set; }

	public Action TimeoutAction { get; set; }

	public Action RemoveOfferAction { get; set; }

	public CustomerOffer SelectedOffer { get; set; }
}
