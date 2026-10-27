using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media.Imaging;

namespace Redbox.Rental.UI.Models;

public class TitleDetailsWatchOptionsPopupViewModel
{
	public Action OnWatchOptionsSeeFullDetailsButtonClicked;

	public Action OnWatchOptionsCloseButtonClicked;

	public string Header { get; set; }

	public string Description { get; set; }

	public string Starring { get; set; }

	public string Rating { get; set; }

	public string RunningTime { get; set; }

	public string ReleaseYear { get; set; }

	public double BorderHeight { get; set; }

	public Thickness RatingBorderThickness { get; set; }

	public Thickness RunningTimeBorderThickness { get; set; }

	public Thickness ReleaseYearBorderThickness { get; set; }

	public Visibility RatingVisibility { get; set; }

	public Visibility RunningTimeVisibility { get; set; }

	public Visibility ReleaseYearVisibility { get; set; }

	public Visibility CCSupported { get; set; }

	public Visibility MoreLikeThisVisibility { get; set; }

	public BitmapImage QRCodeImage { get; set; }

	public DisplayProductModel DisplayProductModel { get; set; }

	public List<DisplayProductMoreLikeThisModel> DisplayProductMoreLikeThisModels { get; set; }

	public void ProcessOnWatchOptionsSeeFullDetailsButtonClicked()
	{
		if (OnWatchOptionsSeeFullDetailsButtonClicked != null)
		{
			OnWatchOptionsSeeFullDetailsButtonClicked();
		}
	}

	public void ProcessOnWatchOptionsCloseButtonClicked()
	{
		if (OnWatchOptionsCloseButtonClicked != null)
		{
			OnWatchOptionsCloseButtonClicked();
		}
	}
}
