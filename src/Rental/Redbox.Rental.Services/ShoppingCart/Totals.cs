using System.Collections.Generic;
using System.Linq;
using Redbox.Rental.Model.ShoppingCart;

namespace Redbox.Rental.Services.ShoppingCart;

public class Totals : ITotals
{
	private IRentalShoppingCart _rentalShoppingCart;

	public List<ISubTotalGroup> _subTotalGroups = new List<ISubTotalGroup>();

	public decimal SubTotal => _subTotalGroups.Sum((ISubTotalGroup x) => x.SubTotal);

	public decimal TaxAmount => _subTotalGroups.Sum((ISubTotalGroup x) => x.TaxAmount) + (_rentalShoppingCart?.ServiceFee?.TaxAmount).GetValueOrDefault();

	public decimal DiscountAmountUsed => _subTotalGroups.Sum((ISubTotalGroup x) => x.DiscountAmountUsed);

	public decimal ServiceFee => (_rentalShoppingCart?.ServiceFee?.ActualAmount).GetValueOrDefault();

	public decimal GrandTotal => SubTotal + ServiceFee + TaxAmount;

	public List<ISubTotalGroup> SubTotalGroups => _subTotalGroups;

	public Totals(IRentalShoppingCart rentalShoppingCart)
	{
		_rentalShoppingCart = rentalShoppingCart;
		UpdateSubTotalGroups();
	}

	public void UpdateSubTotalGroups()
	{
		if (_rentalShoppingCart == null)
		{
			return;
		}
		foreach (RentalShoppingCartItemAction eachAction in _rentalShoppingCart.Items.Select((IBaseRentalShoppingCartItem x) => x.Action).Distinct())
		{
			if (!_subTotalGroups.Any((ISubTotalGroup x) => x.Action == eachAction))
			{
				_subTotalGroups.Add(new SubTotalGroup(_rentalShoppingCart)
				{
					Action = eachAction
				});
			}
		}
		List<ISubTotalGroup> list = new List<ISubTotalGroup>();
		foreach (ISubTotalGroup item in _subTotalGroups.Where((ISubTotalGroup x) => !_rentalShoppingCart.Items.Any((IBaseRentalShoppingCartItem y) => y.Action == x.Action)))
		{
			list.Add(item);
		}
		foreach (ISubTotalGroup item2 in list)
		{
			_subTotalGroups.Remove(item2);
		}
	}
}
