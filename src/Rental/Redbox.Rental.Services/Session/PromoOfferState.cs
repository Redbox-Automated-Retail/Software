using System.Collections.Generic;
using Redbox.Rental.Model.Browse;
using Redbox.Rental.Model.KioskProduct;
using Redbox.Rental.Model.Promotion;
using Redbox.Rental.Model.Session;

namespace Redbox.Rental.Services.Session;

public class PromoOfferState : IPromoOfferState
{
	public int BrowsePageNumber { get; set; }

	public ITitleProduct SelectedProduct { get; set; }

	public CustomerOffer SelectedOffer { get; set; }

	public bool WasOfferPresented { get; set; }

	public bool WasOfferAccepted { get; set; }

	public bool WasPerksSignupCompleted { get; set; }

	public List<IKioskProduct> BrowseProductsShown { get; set; }

	public List<IBrowseItemModel> TTSBrowseItemModelsShown { get; set; }

	public List<IDiscOfferCode> DiscOfferCodes { get; set; }
}
