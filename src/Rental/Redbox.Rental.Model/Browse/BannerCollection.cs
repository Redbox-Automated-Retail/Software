using System.Collections.Generic;

namespace Redbox.Rental.Model.Browse;

public class BannerCollection : List<Banner>
{
	private static BannerCollection _bannerCollection;

	public static BannerCollection Instance
	{
		get
		{
			if (_bannerCollection == null)
			{
				_bannerCollection = new BannerCollection
				{
					new Banner
					{
						BannerType = BannerType.Bluray,
						BannerStyleName = "banner_format_background",
						TextResourceName = "blu_ray"
					},
					new Banner
					{
						BannerType = BannerType.DVD,
						BannerStyleName = "banner_format_background",
						TextResourceName = "dvd"
					},
					new Banner
					{
						BannerType = BannerType._4KUHD,
						BannerStyleName = "banner_format_background",
						TextResourceName = "_4k_uhd"
					},
					new Banner
					{
						BannerType = BannerType.ComingSoon,
						BannerStyleName = "banner_coming_soon_background",
						TextResourceName = "coming_label",
						BannerSize = BannerSize.Large
					},
					new Banner
					{
						BannerType = BannerType.Purchase,
						BannerStyleName = "banner_purchase_background",
						TextResourceName = "mini_cart_purchase_text",
						BannerSize = BannerSize.Mini
					},
					new Banner
					{
						BannerType = BannerType.MultipleDisc,
						BannerStyleName = "banner_multiple_disc_background",
						TextResourceName = "disc_vend",
						BannerSize = BannerSize.Mini
					},
					new Banner
					{
						BannerType = BannerType.DigitalCode,
						BannerStyleName = "banner_format_background",
						TextResourceName = "digital_code_banner"
					},
					new Banner
					{
						BannerType = BannerType.Subscription,
						BannerStyleName = "banner_subscription_background",
						TextResourceName = "mini_cart_subscription_text",
						BannerSize = BannerSize.Mini
					},
					new Banner
					{
						BannerType = BannerType.Plan,
						BannerStyleName = "banner_plan_background",
						TextResourceName = "mini_cart_plan_text",
						BannerSize = BannerSize.Mini
					}
				};
			}
			return _bannerCollection;
		}
	}
}
