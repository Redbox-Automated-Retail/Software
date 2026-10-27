using Redbox.Rental.Model.KioskProduct;
using Redbox.Rental.Model.Profile;

namespace Redbox.Rental.Services.KioskProduct;

public class SubscriptionProduct : KioskProduct, ISubscriptionProduct, IKioskProduct
{
	private decimal _price;

	private decimal _taxRate;

	public string SubscriptionId { get; set; }

	public SubscriptionType SubscriptionType { get; set; }

	public decimal Price
	{
		get
		{
			return GetPrice();
		}
		set
		{
			SetPrice(value);
		}
	}

	public decimal TaxRate
	{
		get
		{
			return GetTaxRate();
		}
		set
		{
			SetTaxRate(value);
		}
	}

	public SubscriptionProduct(Store store)
		: base(store)
	{
		base.KioskProductType = KioskProductType.Subscription;
	}

	protected virtual decimal GetPrice()
	{
		return _price;
	}

	protected virtual void SetPrice(decimal price)
	{
		_price = price;
	}

	protected virtual decimal GetTaxRate()
	{
		return _taxRate;
	}

	protected virtual void SetTaxRate(decimal taxRate)
	{
		_taxRate = taxRate;
	}

	public int CompareTo(ISubscriptionProduct subscriptionProduct)
	{
		int num = SubscriptionType.CompareTo(subscriptionProduct?.SubscriptionType);
		if (num == 0)
		{
			num = SubscriptionId.CompareTo(subscriptionProduct?.SubscriptionId);
		}
		return num;
	}
}
