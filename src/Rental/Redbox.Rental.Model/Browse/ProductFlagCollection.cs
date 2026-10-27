using System.Collections.Generic;

namespace Redbox.Rental.Model.Browse;

public class ProductFlagCollection : List<ProductFlag>
{
	private static ProductFlagCollection _instance;

	public static ProductFlagCollection Instance
	{
		get
		{
			if (_instance == null)
			{
				_instance = new ProductFlagCollection
				{
					new ProductFlag
					{
						Flag = ProductFlags.New,
						TextKeyName = "flag_new",
						FlagStyleName = "rubine_flag_style"
					},
					new ProductFlag
					{
						Flag = ProductFlags.AwardWinner,
						TextKeyName = "flag_award_winner",
						FlagStyleName = "warm_purple_flag_style"
					},
					new ProductFlag
					{
						Flag = ProductFlags.BackAgain,
						TextKeyName = "flag_back_again",
						FlagStyleName = "rubine_flag_style"
					},
					new ProductFlag
					{
						Flag = ProductFlags.Classics,
						TextKeyName = "flag_classics",
						FlagStyleName = "rubine_flag_style"
					},
					new ProductFlag
					{
						Flag = ProductFlags.ComingSoon,
						TextKeyName = "flag_coming_soon",
						FlagStyleName = "warm_purple_flag_style"
					},
					new ProductFlag
					{
						Flag = ProductFlags.LastChance,
						TextKeyName = "flag_last_chance",
						FlagStyleName = "darkish_pink_flag_style"
					},
					new ProductFlag
					{
						Flag = ProductFlags.OutOfStock,
						TextKeyName = "flag_out_of_stock",
						FlagStyleName = "gray_flag_style"
					},
					new ProductFlag
					{
						Flag = ProductFlags.FlashDeal,
						TextKeyName = "flag_flash_deal",
						FlagStyleName = "orange_flag_style"
					},
					new ProductFlag
					{
						Flag = ProductFlags.Points,
						TextKeyName = "flag_points",
						FlagStyleName = "rubine_flag_style"
					},
					new ProductFlag
					{
						Flag = ProductFlags.Excluded,
						TextKeyName = "flag_excluded",
						FlagStyleName = "gray_flag_style"
					},
					new ProductFlag
					{
						Flag = ProductFlags.AwardNominee,
						TextKeyName = "flag_award_nominee",
						FlagStyleName = "warm_purple_flag_style"
					},
					new ProductFlag
					{
						Flag = ProductFlags.WishList,
						TextKeyName = "flag_wish_list",
						FlagStyleName = "rubine_flag_style"
					},
					new ProductFlag
					{
						Flag = ProductFlags.SonySweepstakes,
						TextKeyName = "flag_sony_sweepstakes",
						FlagStyleName = "rubine_flag_style"
					}
				};
			}
			return _instance;
		}
	}
}
