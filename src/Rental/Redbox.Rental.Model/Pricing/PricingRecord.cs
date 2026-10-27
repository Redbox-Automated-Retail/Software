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
				decimal? num = DefaultInitialNightPrice - (decimal?)InitialNight;
				return (num.GetValueOrDefault() > 0m) & num.HasValue;
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
		int num = TitleFamily.CompareTo(pricingRecord.TitleFamily);
		if (num == 0)
		{
			num = TitleType.CompareTo(pricingRecord.TitleType);
		}
		if (num == 0)
		{
			num = InitialNight.CompareTo(pricingRecord.InitialNight);
		}
		if (num == 0)
		{
			num = ExtraNight.CompareTo(pricingRecord.ExtraNight);
		}
		if (num == 0)
		{
			num = ExpirationPrice.CompareTo(pricingRecord.ExpirationPrice);
		}
		if (num == 0)
		{
			num = NonReturn.CompareTo(pricingRecord.NonReturn);
		}
		if (num == 0)
		{
			num = NonReturnDays.CompareTo(pricingRecord.NonReturnDays);
		}
		if (num == 0)
		{
			num = PromoValue.CompareTo(pricingRecord.PromoValue);
		}
		if (num == 0)
		{
			num = Purchase.CompareTo(pricingRecord.Purchase);
		}
		if (num == 0)
		{
			num = (ProductPricingId ?? (-999)).CompareTo(pricingRecord.ProductPricingId ?? (-999));
		}
		if (num == 0)
		{
			num = (InitialDays ?? (-999)).CompareTo(pricingRecord.InitialDays ?? (-999));
		}
		if (num == 0)
		{
			num = (DefaultInitialNightPrice ?? (-999m)).CompareTo(pricingRecord.DefaultInitialNightPrice ?? (-999m));
		}
		if (num == 0)
		{
			num = (DefaultExtraNightPrice ?? (-999m)).CompareTo(pricingRecord.DefaultExtraNightPrice ?? (-999m));
		}
		if (num == 0)
		{
			num = (PriceSetId ?? (-999)).CompareTo(pricingRecord.PriceSetId ?? (-999));
		}
		return num;
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
		Dictionary<string, object> obj = new Dictionary<string, object>
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
		obj.Add("price_set_id", "1");
		return obj;
	}
}
