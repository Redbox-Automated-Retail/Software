using System.Collections.Generic;
using System.Collections.ObjectModel;
using Redbox.Rental.UI.ControllersLogic;

namespace Redbox.Rental.UI.Models;

public interface IPromoListModel
{
	List<IPerksOfferListItem> StoredPromoCodes { get; set; }

	DynamicRoutedCommand NextPagePressedCommand { get; set; }

	DynamicRoutedCommand PreviousPagePressedCommand { get; set; }

	DynamicRoutedCommand PageNumberPressedCommand { get; set; }

	DynamicRoutedCommand AddOrRemovePromoCommand { get; set; }

	DynamicRoutedCommand DetailsCommand { get; set; }

	int NumberOfPages { get; set; }

	int PromosPerPage { get; set; }

	ObservableCollection<IPerksOfferListItem> CurrentPagePromos { get; set; }

	int CurrentPageNumber { get; set; }

	PromoListLogic PromoLogic { get; }
}
