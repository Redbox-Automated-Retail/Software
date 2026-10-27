namespace Redbox.Rental.Model.Analytics;

public class MultiNightPopupData : AnalyticsData
{
	public long ProductId { get; set; }

	public decimal InitialNightPrice { get; set; }

	public bool HasMultiNightPrice { get; set; }

	public decimal? MultiNightPrice { get; set; }

	public int? MultipNightInitialDays { get; set; }

	public bool ShowBuyButton { get; set; }

	public decimal? PurchasePrice { get; set; }

	public MultiNightPopupData()
	{
		base.DataType = "MultiNightPopup";
	}
}
