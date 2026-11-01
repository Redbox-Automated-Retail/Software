using System.Collections.Generic;
using Redbox.Core;
using Redbox.Rental.Model.KioskProduct;

namespace Redbox.Rental.Model.Pricing;

public class PricingRecord : IPricingRecord
{
	public class Constants
	{
		public const string PriceSetId = "price_set_id";

		public const string Family = "family";

		public const string Type = "type";

		public const string InitialNight = "initial_night";

		public const string ExtraNight = "extra_night";

		public const string ExpirationlPrice = "expiration_price";

		public const string NonReturn = "non_return";

		public const string NonReturnDays = "non_return_days";

		public const string PromoValue = "promo_value";

		public const string Purchase = "purchase";

		public const string ProductPricingId = "product_price_id";

		public const string InitialDays = "initial_days";

		public const string DefaultInitialNightPrice = "default_initial_night";

		public const string DefaultExtraNightPrice = "default_extra_night";
	}

	public TitleFamily TitleFamily { get; set; }

	public TitleType TitleType { get; set; }

	public decimal InitialNight { get; set; }

	public decimal ExtraNight { get; set; }

	public decimal ExpirationPrice { get; set; }

	public decimal NonReturn { get; set; }

	public decimal NonReturnDays { get; set; }

	public decimal PromoValue { get; set; }

	public long? ProductPricingId { get; set; }

	public int? InitialDays { get; set; }

	public decimal? DefaultInitialNightPrice { get; set; }

	public decimal? DefaultExtraNightPrice { get; set; }

	public int? PriceSetId { get; set; }

	public decimal Purchase { get; set; }

	public bool IsMultiNightPrice
	{
		get
		{
			if (InitialDays.HasValue)
			{
				return InitialDays > 1;
			}
			return false;
		}
	}

	public bool IsDeal
	{
		get
		{
			if (!IsMultiNightPrice && DefaultInitialNightPrice.HasValue)
			{
				decimal? discount = DefaultInitialNightPrice - (decimal?)InitialNight;
				return (discount.GetValueOrDefault() > 0m) & discount.HasValue;
			}
			return false;
		}
	}

	public bool IsEqual(IPricingRecord pricingRecord)
	{
		return CompareTo(pricingRecord) == 0;
	}

	public int CompareTo(IPricingRecord pricingRecord)
	{
		int comparison = TitleFamily.CompareTo(pricingRecord.TitleFamily);
		if (comparison == 0)
		{
			comparison = TitleType.CompareTo(pricingRecord.TitleType);
		}
		if (comparison == 0)
		{
			comparison = InitialNight.CompareTo(pricingRecord.InitialNight);
		}
		if (comparison == 0)
		{
			comparison = ExtraNight.CompareTo(pricingRecord.ExtraNight);
		}
		if (comparison == 0)
		{
			comparison = ExpirationPrice.CompareTo(pricingRecord.ExpirationPrice);
		}
		if (comparison == 0)
		{
			comparison = NonReturn.CompareTo(pricingRecord.NonReturn);
		}
		if (comparison == 0)
		{
			comparison = NonReturnDays.CompareTo(pricingRecord.NonReturnDays);
		}
		if (comparison == 0)
		{
			comparison = PromoValue.CompareTo(pricingRecord.PromoValue);
		}
		if (comparison == 0)
		{
			comparison = Purchase.CompareTo(pricingRecord.Purchase);
		}
		if (comparison == 0)
		{
			comparison = (ProductPricingId ?? (-999)).CompareTo(pricingRecord.ProductPricingId ?? (-999));
		}
		if (comparison == 0)
		{
			comparison = (InitialDays ?? (-999)).CompareTo(pricingRecord.InitialDays ?? (-999));
		}
		if (comparison == 0)
		{
			comparison = (DefaultInitialNightPrice ?? (-999m)).CompareTo(pricingRecord.DefaultInitialNightPrice ?? (-999m));
		}
		if (comparison == 0)
		{
			comparison = (DefaultExtraNightPrice ?? (-999m)).CompareTo(pricingRecord.DefaultExtraNightPrice ?? (-999m));
		}
		if (comparison == 0)
		{
			comparison = (PriceSetId ?? (-999)).CompareTo(pricingRecord.PriceSetId ?? (-999));
		}
		return comparison;
	}

	public IPricingRecord Clone()
	{
		return new PricingRecord
		{
			TitleFamily = TitleFamily,
			TitleType = TitleType,
			InitialNight = InitialNight,
			ExtraNight = ExtraNight,
			ExpirationPrice = ExpirationPrice,
			NonReturn = NonReturn,
			NonReturnDays = NonReturnDays,
			PromoValue = PromoValue,
			Purchase = Purchase,
			ProductPricingId = ProductPricingId,
			InitialDays = InitialDays,
			DefaultInitialNightPrice = DefaultInitialNightPrice,
			DefaultExtraNightPrice = DefaultExtraNightPrice,
			PriceSetId = PriceSetId
		};
	}

	public Dictionary<string, object> ToDictionary()
	{
		Dictionary<string, object> values = new Dictionary<string, object>
		{
			{
				"family",
				TitleFamily.ToString().ToLower()
			},
			{
				"type",
				TitleType.ToString().ToLower().Replace("bluray", "blu-ray")
					.Replace("all", "*")
			},
			{ "initial_night", InitialNight },
			{ "extra_night", ExtraNight },
			{ "expiration_price", ExpirationPrice },
			{ "non_return", NonReturn },
			{ "non_return_days", NonReturnDays },
			{ "promo_value", PromoValue },
			{ "purchase", Purchase },
			{ "product_price_id", ProductPricingId },
			{ "initial_days", InitialDays },
			{ "default_initial_night", DefaultInitialNightPrice },
			{ "default_extra_night", DefaultExtraNightPrice }
		};
		ServiceLocator.Instance.GetService<IPricingService>();
		values.Add("price_set_id", "1");
		return values;
	}
}
