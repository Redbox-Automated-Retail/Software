using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Redbox.Rental.Model.KioskProduct;
using Redbox.Rental.Model.ShoppingCart;

namespace Redbox.Rental.Model.Analytics;

public class ProductData : AnalyticsData
{
	public string ViewName { get; set; }

	public long? ProductId { get; set; }

	public long? TitleRollupProductGroupId { get; set; }

	public int? ProductTypeId { get; set; }

	public string TivoQueryId { get; set; }

	[JsonConverter(typeof(StringEnumConverter))]
	public TitleFamily? TitleFamily { get; set; }

	[JsonConverter(typeof(StringEnumConverter))]
	public TitleType? TitleType { get; set; }

	public string SubscriptionId { get; set; }

	[JsonConverter(typeof(StringEnumConverter))]
	public SubscriptionType? SubscriptionType { get; set; }

	public ProductData()
	{
		base.DataType = "Product";
	}

	public static ProductData ToProductData(long productGroupId)
	{
		return new ProductData
		{
			TitleRollupProductGroupId = productGroupId
		};
	}

	public static ProductData ToProductData(ITitleRollupProduct titleRollupProduct)
	{
		if (titleRollupProduct == null)
		{
			return null;
		}
		return new ProductData
		{
			ProductId = titleRollupProduct.ProductId,
			TitleRollupProductGroupId = titleRollupProduct.TitleRollupProductGroupId,
			ProductTypeId = titleRollupProduct.ProductTypeId,
			TitleFamily = titleRollupProduct.TitleFamily
		};
	}

	public static ProductData ToProductData(ITitleProduct titleProduct)
	{
		if (titleProduct == null)
		{
			return null;
		}
		return new ProductData
		{
			ProductId = titleProduct.ProductId,
			TitleRollupProductGroupId = titleProduct.TitleRollupProductGroupId,
			ProductTypeId = titleProduct.ProductTypeId,
			TitleFamily = titleProduct.TitleFamily,
			TitleType = titleProduct.TitleType,
			TivoQueryId = titleProduct.TivoQueryId
		};
	}

	public static ProductData ToProductData(ISubscriptionProduct subscriptionProduct)
	{
		ProductData result = null;
		if (subscriptionProduct != null)
		{
			result = new ProductData
			{
				SubscriptionId = subscriptionProduct.SubscriptionId,
				SubscriptionType = subscriptionProduct.SubscriptionType
			};
		}
		return result;
	}

	public static ProductData ToProductData(IBaseRentalShoppingCartItem rentalShoppingCartItem)
	{
		IRentalShoppingCartTitleItem rentalShoppingCartTitleItem = rentalShoppingCartItem as IRentalShoppingCartTitleItem;
		IRentalShoppingCartSubscriptionItem rentalShoppingCartSubscriptionItem = rentalShoppingCartItem as IRentalShoppingCartSubscriptionItem;
		if (rentalShoppingCartTitleItem != null)
		{
			return new ProductData
			{
				ProductId = rentalShoppingCartTitleItem.ProductId,
				TitleFamily = rentalShoppingCartTitleItem.TitleFamily,
				TitleType = rentalShoppingCartTitleItem.TitleType
			};
		}
		if (rentalShoppingCartSubscriptionItem != null)
		{
			return new ProductData
			{
				SubscriptionId = rentalShoppingCartSubscriptionItem.SubscriptionProduct.SubscriptionId,
				SubscriptionType = rentalShoppingCartSubscriptionItem.SubscriptionProduct.SubscriptionType
			};
		}
		return null;
	}
}
