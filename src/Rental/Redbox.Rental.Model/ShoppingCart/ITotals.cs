using System.Collections.Generic;

namespace Redbox.Rental.Model.ShoppingCart;

public interface ITotals
{
	decimal SubTotal { get; }

	decimal TaxAmount { get; }

	decimal DiscountAmountUsed { get; }

	decimal ServiceFee { get; }

	decimal GrandTotal { get; }

	List<ISubTotalGroup> SubTotalGroups { get; }

	void UpdateSubTotalGroups();
}
