using System;
using System.Windows;

namespace Redbox.Rental.UI.Models;

public class PerksOfferListItemModel : BaseModel<PerksOfferListItemModel>, IPerksOfferListItem
{
	private static DependencyProperty IsAdaModeProperty = DependencyProperty.Register("IsAdaMode", typeof(bool), typeof(PerksOfferListItemModel), new PropertyMetadata(false));

	public string StatusText { get; set; }

	public string OfferStatus { get; set; }

	public string OfferValueText { get; set; }

	public string OfferUnitsText { get; set; }

	public string Name { get; set; }

	public string Description { get; set; }

	public string TimeToComplete { get; set; }

	public string LegalInformation { get; set; }

	public int? CurrentValue { get; set; }

	public int? MaxValue { get; set; }

	public int? RemainderValue { get; set; }

	public DateTime? StartDate { get; set; }

	public DateTime? EndDate { get; set; }

	public string DetailsButtonText { get; set; }

	public DynamicRoutedCommand DetailsButtonCommand { get; set; }

	public string CongratsText { get; set; }

	public string YouEarnedText { get; set; }

	public string ForCompletingText { get; set; }

	public int AdaButtonNumber { get; set; }

	public bool IsAdaMode
	{
		get
		{
			return base.Dispatcher.Invoke(() => (bool)GetValue(IsAdaModeProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(IsAdaModeProperty, value);
			});
		}
	}

	public int NumberOfSpaces => 2;
}
