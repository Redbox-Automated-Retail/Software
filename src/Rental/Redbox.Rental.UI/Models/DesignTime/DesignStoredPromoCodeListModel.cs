using System.Collections.ObjectModel;
using System.Windows;

namespace Redbox.Rental.UI.Models.DesignTime;

public class DesignStoredPromoCodeListModel : DependencyObject
{
	private static DependencyPropertyKey CurrentPagePromosPropertyKey = DependencyProperty.RegisterReadOnly("CurrentPagePromos", typeof(ObservableCollection<IPerksOfferListItem>), typeof(DesignStoredPromoCodeListModel), new PropertyMetadata(new ObservableCollection<IPerksOfferListItem>()));

	public static readonly DependencyProperty CurrentPagePromosProperty = CurrentPagePromosPropertyKey.DependencyProperty;

	public ObservableCollection<IPerksOfferListItem> CurrentPagePromos
	{
		get
		{
			return base.Dispatcher.Invoke(() => (ObservableCollection<IPerksOfferListItem>)GetValue(CurrentPagePromosProperty));
		}
		private set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(CurrentPagePromosPropertyKey, value);
			});
		}
	}

	public bool IsAdaMode { get; set; }

	public DesignStoredPromoCodeListModel()
	{
		CurrentPagePromos = new ObservableCollection<IPerksOfferListItem>();
		CurrentPagePromos.Add(new StoredPromoCodeModel
		{
			PromoCode = "TESTCODE1",
			IsAdded = false,
			PromoCodeDescription = "Rent 1 disc, get a Free 1-night DVD rental",
			PromoCodeExpirationText = "EXPIRES TODAY",
			AddRemoveButtonCommand = new DynamicRoutedCommand()
		});
		CurrentPagePromos.Add(new StoredPromoCodeModel
		{
			PromoCode = "TESTBDAY",
			IsAdded = true,
			PromoCodeDescription = "1 free birthday rental",
			PromoCodeExpirationText = "TWO DAYS LEFT",
			AddRemoveButtonCommand = new DynamicRoutedCommand()
		});
		CurrentPagePromos.Add(new StoredPromoCodeModel
		{
			PromoCode = "TESTB1G1",
			IsAdded = false,
			PromoCodeDescription = "Rent 1 disc, get a Free 1-night DVD rental",
			PromoCodeExpirationText = "THREE DAYS LEFT",
			AddRemoveButtonCommand = new DynamicRoutedCommand()
		});
		IsAdaMode = true;
	}
}
