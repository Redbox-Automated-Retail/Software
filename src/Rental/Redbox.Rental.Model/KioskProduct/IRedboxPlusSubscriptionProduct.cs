using System.Windows;

namespace Redbox.Rental.Model.KioskProduct;

public interface IRedboxPlusSubscriptionProduct : ISubscriptionProduct, IKioskProduct
{
	bool IsPromotional { get; set; }

	string TitleText { get; set; }

	string SubtitleText { get; set; }

	string LegalText { get; set; }

	string MyBagProductDescription { get; set; }

	Thickness MainStackPanelMargin { get; set; }

	string Benefit1Header { get; set; }

	string Benefit1Detail { get; set; }

	string Benefit2Header { get; set; }

	string Benefit2Detail { get; set; }

	string Benefit3Header { get; set; }

	string Benefit3Detail { get; set; }

	bool ShowTopBoxArtDisplay { get; set; }

	bool ShowBottomBoxArtDisplay { get; set; }

	string AcceptanceTerms { get; set; }
}
