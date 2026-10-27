using System;

namespace Redbox.Rental.UI.Views;

public class PerksThanksInfo
{
	public enum ImageType
	{
		PerksStar,
		CheckMark
	}

	public string TitleText { get; set; }

	public string MessageText { get; set; }

	public string CommentText { get; set; }

	public Action<AnalyticsTypes> AnalyticsAction { get; set; }

	public Action ContinueAction { get; set; }

	public bool ShowPerksIcons { get; set; }

	public ImageType TopImage { get; set; }
}
