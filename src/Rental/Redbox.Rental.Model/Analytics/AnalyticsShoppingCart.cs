using System;
using System.Collections.Generic;
using System.Linq;
using Redbox.Core;
using Redbox.Rental.Model.DataService;
using Redbox.Rental.Model.KioskProduct;
using Redbox.Rental.Model.ShoppingCart;

namespace Redbox.Rental.Model.Analytics;

public class AnalyticsShoppingCart
{
	public decimal SubTotal { get; set; }

	public decimal TaxAmount { get; set; }

	public decimal GrandTotal { get; set; }

	public List<AnalyticsShoppingCartItem> Items { get; } = new List<AnalyticsShoppingCartItem>();

	public List<AnalyticsShoppingCartSubscriptionItem> SubscriptionItems { get; } = new List<AnalyticsShoppingCartSubscriptionItem>();

	public List<AnalyticsDiscount> Discounts { get; } = new List<AnalyticsDiscount>();

	public static AnalyticsShoppingCart ToShoppingCart(IRentalShoppingCart sc)
	{
		AnalyticsShoppingCart cart = new AnalyticsShoppingCart
		{
			SubTotal = ((sc.Totals == null) ? 0m : sc.Totals.SubTotal),
			TaxAmount = ((sc.Totals == null) ? 0m : sc.Totals.TaxAmount),
			GrandTotal = ((sc.Totals == null) ? 0m : sc.Totals.GrandTotal)
		};
		(sc?.TitleItems?.Where((IRentalShoppingCartTitleItem x) => x?.LoyaltyPointsRecord?.ToBeRedeemed == true))?.ToList().ForEach(delegate(IRentalShoppingCartTitleItem x)
		{
			cart.Discounts.Add(new AnalyticsDiscount
			{
				ProductId = x.ProductId,
				DiscountType = DiscountType.Loyalty,
				RedemptionPoints = x.LoyaltyPointsRecord.PointsToRedeem,
				Amount = (x.PricingRecord?.InitialNight ?? 0m)
			});
		});
		if (sc.Promotion != null)
		{
			sc.Promotion.PromotionItems?.ToList().ForEach(delegate(IPromotionItem x)
			{
				cart.Discounts.Add(new AnalyticsDiscount
				{
					Amount = x.PromotionAmount,
					DiscountType = DiscountType.PromotionCode,
					PromotionCode = sc.Promotion.Name,
					PromotionCodeValue = sc.Promotion.OriginalPrice,
					PromotionIntentCode = sc.Promotion.Intent,
					ProductId = x.RentalShoppingCartItem.ProductId,
					ApplyOnlyToProduct = (sc.Promotion.IsValidatedPostVend ? new bool?(sc.Promotion.IsProductSpecific) : ((bool?)null))
				});
			});
		}
		try
		{
			IDataService service = ServiceLocator.Instance.GetService<IDataService>();
			if (service != null)
			{
				List<ITitleProduct> movieBrowseProducts = service.GetMovieBrowseProducts();
				List<ITitleRollupProduct> rollupOnly = (from x in movieBrowseProducts
					where x is ITitleRollupProduct
					select x as ITitleRollupProduct).ToList();
				sc.TitleItems.ToList().ForEach(delegate(IRentalShoppingCartTitleItem item)
				{
					ITitleRollupProduct titleRollupProduct = rollupOnly.Where((ITitleRollupProduct tr) => tr.KioskProducts.Contains(item.TitleProduct)).FirstOrDefault();
					if (titleRollupProduct != null)
					{
						int num = movieBrowseProducts.IndexOf(titleRollupProduct);
						item.RentalPosition = num + 1;
					}
					else
					{
						int num2 = movieBrowseProducts.IndexOf(item.TitleProduct);
						if (num2 > -1)
						{
							item.RentalPosition = num2 + 1;
						}
					}
				});
			}
		}
		catch (Exception e)
		{
			LogHelper.Instance.LogException("AnalyticsShoppingCart.ToShoppingCart - unhandled exception occurred", e);
		}
		sc.TitleItems.ToList().ForEach(delegate(IRentalShoppingCartTitleItem item)
		{
			cart.Items.Add(AnalyticsShoppingCartItem.ToShoppingCartItem(item));
		});
		sc.SubscriptionItems.ToList().ForEach(delegate(IRentalShoppingCartSubscriptionItem subscriptionItem)
		{
			cart.SubscriptionItems.Add(AnalyticsShoppingCartSubscriptionItem.ToAnalyticsShoppingCartSubscriptionItem(subscriptionItem));
		});
		return cart;
	}
}
