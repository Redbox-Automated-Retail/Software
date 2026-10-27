using System.Collections.Generic;
using Redbox.Rental.Model.Browse;
using Redbox.Rental.Model.KioskProduct;
using Redbox.Rental.Model.Promotion;

namespace Redbox.Rental.Model.Session;

public interface IPromoOfferState
{
	int BrowsePageNumber { get; set; }

	CustomerOffer SelectedOffer { get; set; }

	ITitleProduct SelectedProduct { get; set; }

	bool WasOfferPresented { get; set; }

	bool WasOfferAccepted { get; set; }

	bool WasPerksSignupCompleted { get; set; }

	List<IKioskProduct> BrowseProductsShown { get; set; }

	List<IBrowseItemModel> TTSBrowseItemModelsShown { get; set; }

	List<IDiscOfferCode> DiscOfferCodes { get; set; }
}
