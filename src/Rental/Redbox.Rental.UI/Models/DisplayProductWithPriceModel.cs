namespace Redbox.Rental.UI.Models;

public class DisplayProductWithPriceModel : DisplayProductModel
{
	public string PriceTypeText { get; set; }

	public string PriceText { get; set; }

	public string StrikethroughPriceText { get; set; }

	public string TitleText { get; set; }

	public double TitleTextWidth { get; set; }
}
