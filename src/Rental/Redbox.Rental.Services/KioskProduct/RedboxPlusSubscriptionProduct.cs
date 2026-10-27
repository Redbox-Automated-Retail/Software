using System.Collections.Generic;
using System.Windows;
using Redbox.Rental.Model.KioskProduct;
using Redbox.Rental.Model.Profile;

namespace Redbox.Rental.Services.KioskProduct;

public class RedboxPlusSubscriptionProduct : SubscriptionProduct, IRedboxPlusSubscriptionProduct, ISubscriptionProduct, IKioskProduct
{
	public bool IsPromotional { get; set; }

	public IList<IRedboxPlusOfferPeriod> OfferPeriods { get; private set; }

	public Thickness MainStackPanelMargin { get; set; }

	public string TitleText { get; set; }

	public string SubtitleText { get; set; }

	public string LegalText { get; set; }

	public string MyBagProductDescription { get; set; }

	public string Benefit1Header { get; set; }

	public string Benefit1Detail { get; set; }

	public string Benefit2Header { get; set; }

	public string Benefit2Detail { get; set; }

	public string Benefit3Header { get; set; }

	public string Benefit3Detail { get; set; }

	public bool ShowTopBoxArtDisplay { get; set; }

	public bool ShowBottomBoxArtDisplay { get; set; }

	public string AcceptanceTerms { get; set; }

	public RedboxPlusSubscriptionProduct(Store store)
		: base(store)
	{
		base.SubscriptionType = SubscriptionType.RedboxPlus;
		OfferPeriods = new List<IRedboxPlusOfferPeriod>();
	}
}
