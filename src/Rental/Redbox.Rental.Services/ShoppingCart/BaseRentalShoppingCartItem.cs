using Redbox.Rental.Model.ShoppingCart;

namespace Redbox.Rental.Services.ShoppingCart;

public class BaseRentalShoppingCartItem : IBaseRentalShoppingCartItem
{
	private decimal _price;

	private decimal _taxRate;

	public RentalShoppingCartItemSequence Sequence { get; set; }

	public RentalShoppingCartItemAction Action { get; set; }

	public decimal Quantity { get; set; }

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

	public decimal ExtendedPrice => Price * Quantity;

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
}
