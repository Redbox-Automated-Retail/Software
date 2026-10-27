namespace Redbox.Rental.Model.Analytics;

public class SwipeViewData : AnalyticsData
{
	public string SwipeViewType { get; set; }

	public string TitleText { get; set; }

	public string MessageText { get; set; }

	public string BackButtonText { get; set; }

	public string ChipNotEnabledText { get; set; }

	public SwipeViewData()
	{
		base.DataType = "Swipe";
	}
}
