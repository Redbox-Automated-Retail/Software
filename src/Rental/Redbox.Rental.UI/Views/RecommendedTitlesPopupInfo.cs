using Redbox.Rental.Model.Personalization;

namespace Redbox.Rental.UI.Views;

public class RecommendedTitlesPopupInfo
{
	public IRecommendedTitles RecommendedTitles { get; set; }

	public int NumberOfTitlesToShow { get; set; }

	public string ShownFromView { get; set; }
}
