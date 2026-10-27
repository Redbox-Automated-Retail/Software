using System;
using System.Collections.Generic;
using Redbox.Rental.Model;
using Redbox.Rental.Model.KioskProduct;
using Redbox.Rental.Model.ShoppingCart;

namespace Redbox.Rental.Services.Promotion;

public class PromotionRule : IPromotionRule
{
	public class Constants
	{
		public static string Cart = "cart";

		public static string Product = "product";

		public static string BrowseMinimumQuantity = "browse_min_qty";

		public static string ItemAction = "item_action";

		public static string AddSequence = "add_sequence";

		public static string AskOnlyOnce = "ask_only_once";

		public static string ProductType = "product_type";

		public static string ProductFamily = "product_family";

		public static string ProductIdList = "product_id_list";

		public static string ExcludeIdList = "exclude_id_list";

		public static string ProductTypeList = "product_type_list";

		public static string MinimumQuantity = "min_qty";

		public static string MaximumQuantity = "max_qty";

		public static string RedemptionCode = "redemption_code";

		public static string RedemptionRule = "redemption_rule";

		public static string RedemptionAmount = "redemption_amount";

		public static string SatisfyCondition = "satisfy_condition";

		public static string OfferCondition = "offer_condition";

		public static string OfferLanguage = "offer_language";

		public static string OfferAction = "offer_action";

		public static string OfferMessageVersion = "offer_message_version";

		public static string InCartPromoBodyMessage = "InCartPromoBodyMessage";

		public static string InCartPromoHeaderMessage = "InCartPromoHeaderMessage";

		public static string InCartPromoYesButton = "InCartPromoYesButton";

		public static string InCartPromoNoButton = "InCartPromoNoButton";

		public static string RedemptionRuleProductType = "product_type";

		public static string RedemptionRuleFixedAmount = "fixed_amount";

		public static string RedemptionRuleQuantityBasedAmount = "qty_based_amount";

		public static string RedemptionRulePostOfferFirstNight = "post_offer_first_night";

		public static string OfferActionBrowseFilter = "browse_filter";

		public static string OfferActionTitleMarketing = "title_marketing";
	}

	public string SatisfyCondition { get; set; }

	public string OfferCondition { get; set; }

	public RentalShoppingCartItemAction? Action { get; set; }

	public RentalShoppingCartItemSequence? Sequence { get; set; }

	public TitleType TitleType { get; set; }

	public TitleFamily TitleFamily { get; set; }

	public List<long> IncludeProducts { get; set; }

	public List<long> ExcludeProducts { get; set; }

	public List<TitleType> ProductTypes { get; set; }

	public int MinimumQuantity { get; set; }

	public int MaximumQuantity { get; set; }

	public bool? AskOnlyOnce { get; set; }

	public string RedemptionRule { get; set; }

	public string RedemptionAmount { get; set; }

	public string RedemptionCode { get; set; }

	public string OfferLanguage { get; set; }

	public string OfferMessageVersion { get; set; }

	public string OfferAction { get; set; }

	public string InCartPromoHeaderMessage { get; set; }

	public string InCartPromoBodyMessage { get; set; }

	public string InCartPromoYesButton { get; set; }

	public string InCartPromoNoButton { get; set; }

	public PromotionRule(Dictionary<string, object> rule)
	{
		if (rule.TryGetValue(Constants.AskOnlyOnce, out var value))
		{
			bool result = false;
			if (bool.TryParse(value.ToString(), out result))
			{
				AskOnlyOnce = result;
			}
		}
		if (rule.TryGetValue(Constants.InCartPromoHeaderMessage, out value))
		{
			InCartPromoHeaderMessage = value.ToString();
		}
		if (rule.TryGetValue(Constants.InCartPromoBodyMessage, out value))
		{
			InCartPromoBodyMessage = value.ToString();
		}
		if (rule.TryGetValue(Constants.InCartPromoYesButton, out value))
		{
			InCartPromoYesButton = value.ToString();
		}
		if (rule.TryGetValue(Constants.InCartPromoNoButton, out value))
		{
			InCartPromoNoButton = value.ToString();
		}
		if (rule.TryGetValue(Constants.OfferAction, out value))
		{
			OfferAction = value.ToString();
		}
		if (rule.TryGetValue(Constants.OfferLanguage, out value))
		{
			OfferLanguage = value.ToString();
		}
		if (rule.TryGetValue(Constants.OfferMessageVersion, out value))
		{
			OfferMessageVersion = value.ToString();
		}
		if (rule.TryGetValue(Constants.SatisfyCondition, out value))
		{
			SatisfyCondition = value.ToString();
		}
		if (rule.TryGetValue(Constants.OfferCondition, out value))
		{
			OfferCondition = value.ToString();
		}
		if (rule.TryGetValue(Constants.RedemptionCode, out value))
		{
			RedemptionCode = value.ToString();
		}
		if (rule.TryGetValue(Constants.RedemptionRule, out value))
		{
			RedemptionRule = value.ToString();
		}
		if (rule.TryGetValue(Constants.RedemptionAmount, out value))
		{
			RedemptionAmount = value.ToString();
		}
		if (rule.TryGetValue(Constants.ItemAction, out value) && Enum.TryParse<RentalShoppingCartItemAction>(value.ToString(), out var result2))
		{
			Action = result2;
		}
		if (rule.TryGetValue(Constants.AddSequence, out value) && Enum.TryParse<RentalShoppingCartItemSequence>(value.ToString(), out var result3))
		{
			Sequence = result3;
		}
		TitleType result4 = TitleType.All;
		if (rule.TryGetValue(Constants.ProductType, out value))
		{
			Enum.TryParse<TitleType>(value.ToString(), out result4);
		}
		TitleType = result4;
		TitleFamily result5 = TitleFamily.All;
		if (rule.TryGetValue(Constants.ProductFamily, out value))
		{
			Enum.TryParse<TitleFamily>(value.ToString(), out result5);
		}
		TitleFamily = result5;
		if (rule.TryGetValue(Constants.ProductIdList, out value))
		{
			string text = value?.ToString();
			if (text != null)
			{
				List<string> list = new List<string>(text.Split(','));
				IncludeProducts = new List<long>();
				list.ForEach(delegate(string x)
				{
					long result8 = 0L;
					if (long.TryParse(x, out result8))
					{
						IncludeProducts.Add(result8);
					}
				});
			}
		}
		if (rule.TryGetValue(Constants.ExcludeIdList, out value))
		{
			string text2 = value?.ToString();
			if (text2 != null)
			{
				List<string> list2 = new List<string>(text2.Split(','));
				ExcludeProducts = new List<long>();
				list2.ForEach(delegate(string x)
				{
					long result8 = 0L;
					if (long.TryParse(x, out result8))
					{
						ExcludeProducts.Add(result8);
					}
				});
			}
		}
		if (rule.TryGetValue(Constants.ProductTypeList, out value))
		{
			string text3 = value?.ToString();
			if (!string.IsNullOrEmpty(text3))
			{
				string[] array = text3.Split(',');
				for (int num = 0; num < array.Length; num++)
				{
					if (Enum.TryParse<TitleType>(array[num], out result4))
					{
						if (ProductTypes == null)
						{
							ProductTypes = new List<TitleType>();
						}
						ProductTypes.Add(result4);
					}
				}
			}
		}
		MinimumQuantity = -1;
		if (rule.TryGetValue(Constants.MinimumQuantity, out value))
		{
			int result6 = 0;
			if (int.TryParse(value.ToString(), out result6))
			{
				MinimumQuantity = result6;
			}
		}
		MaximumQuantity = 100000;
		if (rule.TryGetValue(Constants.MaximumQuantity, out value))
		{
			int result7 = 0;
			if (int.TryParse(value.ToString(), out result7))
			{
				MaximumQuantity = result7;
			}
		}
	}
}
