using System;
using System.Collections.Generic;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Rental.Model.KioskProduct;

namespace Redbox.Rental.UI.Views;

public class PromoOfferBrowseInfo : IViewAnalyticsName
{
	public string ViewAnalyticsName { get; set; }

	public string OfferTitleText { get; set; }

	public List<ITitleProduct> applicableProducts { get; set; }

	public Action CancelAction { get; set; }

	public Action<ITitleProduct> ContinueAction { get; set; }

	public Action TimeoutAction { get; set; }

	public Func<decimal, decimal> DiscountedPriceCalc { get; set; }

	public bool AllowMnp { get; set; }

	public bool IsPurchase { get; set; }
}
