namespace Redbox.Rental.Model.Browse;

public class Banner
{
	public class BannerStyleNameConstants
	{
		public const string ComingSoon = "banner_coming_soon_background";

		public const string Purchase = "banner_purchase_background";

		public const string MultipleDisc = "banner_multiple_disc_background";

		public const string Format = "banner_format_background";

		public const string Subscription = "banner_subscription_background";

		public const string Plan = "banner_plan_background";
	}

	public class TextKeyConstants
	{
		public const string Bluray = "blu_ray";

		public const string ComingSoon = "coming_label";

		public const string Purchase = "mini_cart_purchase_text";

		public const string DigitalCode = "digital_code_banner";

		public const string Dvd = "dvd";

		public const string _4k_uhd = "_4k_uhd";

		public const string MultipleDiscs = "disc_vend";

		public const string Subscription = "mini_cart_subscription_text";

		public const string Plan = "mini_cart_plan_text";
	}

	public BannerType BannerType { get; set; }

	public BannerSize BannerSize { get; set; }

	public string BannerStyleName { get; set; }

	public string TextResourceName { get; set; }
}
