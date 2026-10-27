using Redbox.KioskEngine.ComponentModel;
using Redbox.Rental.Model.KioskProduct;

namespace Redbox.Rental.UI.Views;

public class MultiNightPricingPopupInfo : IViewAnalyticsName
{
	public ITitleProduct TitleProduct { get; set; }

	public string ShownFromView { get; set; }

	public string ViewAnalyticsName { get; set; }
}
