using System.Windows;
using Redbox.Rental.UI.Properties;

namespace Redbox.Rental.Services.KioskProduct;

public static class RedboxPlusSubscriptionConstructorHelper
{
	public static void PopulateHardCodedFields(RedboxPlusSubscriptionProduct redboxPlusSubscriptionProduct)
	{
		if (redboxPlusSubscriptionProduct == null)
		{
			return;
		}
		string subscriptionId = redboxPlusSubscriptionProduct.SubscriptionId;
		if (!(subscriptionId == "RBPLUS_OFFER_1"))
		{
			if (subscriptionId == "RBPLUS_OFFER_2")
			{
				redboxPlusSubscriptionProduct.Price = 19.99m;
				redboxPlusSubscriptionProduct.MainStackPanelMargin = new Thickness(0.0, 28.0, 0.0, 0.0);
				redboxPlusSubscriptionProduct.TitleText = Resources.rbplus_offer_2_title;
				redboxPlusSubscriptionProduct.LegalText = Resources.rbplus_offer_2_legal_text;
				redboxPlusSubscriptionProduct.SubtitleText = Resources.rbplus_offer_2_subtitle;
				redboxPlusSubscriptionProduct.MyBagProductDescription = Resources.rbplus_offer_2_my_bag_description;
				redboxPlusSubscriptionProduct.Benefit1Header = Resources.rbplus_offer_2_benefit_1_header_text;
				redboxPlusSubscriptionProduct.Benefit1Detail = Resources.rbplus_offer_2_benefit_1_detail_text;
				redboxPlusSubscriptionProduct.Benefit2Header = Resources.rbplus_offer_2_benefit_2_header_text;
				redboxPlusSubscriptionProduct.Benefit2Detail = Resources.rbplus_offer_2_benefit_2_detail_text;
				redboxPlusSubscriptionProduct.Benefit3Header = Resources.rbplus_offer_2_benefit_3_header_text;
				redboxPlusSubscriptionProduct.Benefit3Detail = Resources.rbplus_offer_2_benefit_3_detail_text;
				redboxPlusSubscriptionProduct.ShowBottomBoxArtDisplay = true;
				redboxPlusSubscriptionProduct.ShowTopBoxArtDisplay = false;
				redboxPlusSubscriptionProduct.AcceptanceTerms = Resources.rbplus_offer_2_acceptance_terms;
			}
		}
		else
		{
			redboxPlusSubscriptionProduct.Price = 9.99m;
			redboxPlusSubscriptionProduct.MainStackPanelMargin = new Thickness(0.0, 64.0, 0.0, 0.0);
			redboxPlusSubscriptionProduct.TitleText = Resources.rbplus_offer_1_title;
			redboxPlusSubscriptionProduct.LegalText = Resources.rbplus_offer_1_legal_text;
			redboxPlusSubscriptionProduct.SubtitleText = Resources.rbplus_offer_1_subtitle;
			redboxPlusSubscriptionProduct.MyBagProductDescription = Resources.rbplus_offer_1_my_bag_description;
			redboxPlusSubscriptionProduct.Benefit1Header = Resources.rbplus_offer_1_benefit_1_header_text;
			redboxPlusSubscriptionProduct.Benefit1Detail = Resources.rbplus_offer_1_benefit_1_detail_text;
			redboxPlusSubscriptionProduct.Benefit2Header = Resources.rbplus_offer_1_benefit_2_header_text;
			redboxPlusSubscriptionProduct.Benefit2Detail = Resources.rbplus_offer_1_benefit_2_detail_text;
			redboxPlusSubscriptionProduct.ShowBottomBoxArtDisplay = false;
			redboxPlusSubscriptionProduct.ShowTopBoxArtDisplay = true;
			redboxPlusSubscriptionProduct.AcceptanceTerms = Resources.rbplus_offer_1_acceptance_terms;
		}
	}
}
