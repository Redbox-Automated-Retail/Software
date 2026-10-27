namespace Redbox.Rental.Model.Browse;

public class ProductFlag
{
	public class FlagStyleNameConstants
	{
		public const string GoldFlag = "gold_flag_style";

		public const string GrayFlag = "gray_flag_style";

		public const string RedFlag = "red_flag_style";

		public const string BlueFlag = "blue_flag_style";

		public const string OrangeFlag = "orange_flag_style";

		public const string RubineFlag = "rubine_flag_style";

		public const string WarmPurpleFlag = "warm_purple_flag_style";

		public const string CharcoalGreyFlag = "charcoal_grey_flag_style";

		public const string MarigoldFlag = "marigold_flag_style";

		public const string DarkishPinkFlag = "darkish_pink_flag_style";

		public const string OffYellowFlag = "off_yellow_flag_style";
	}

	public class FlagKeyNameConstants
	{
		public const string OutOfStockKey = "flag_out_of_stock";

		public const string ComingSoonKey = "flag_coming_soon";

		public const string NewKey = "flag_new";

		public const string LastChanceKey = "flag_last_chance";

		public const string BackAgainKey = "flag_back_again";

		public const string AwardWinnerKey = "flag_award_winner";

		public const string ClassicsKey = "flag_classics";

		public const string FlashDealKey = "flag_flash_deal";

		public const string PointsKey = "flag_points";

		public const string ExcludedKey = "flag_excluded";

		public const string AwardNomineeKey = "flag_award_nominee";

		public const string WishListKey = "flag_wish_list";

		public const string SonySweepstakesKey = "flag_sony_sweepstakes";
	}

	public ProductFlags Flag { get; set; }

	public string FlagStyleName { get; set; }

	public string TextKeyName { get; set; }
}
