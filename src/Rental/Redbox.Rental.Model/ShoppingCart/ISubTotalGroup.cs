namespace Redbox.Rental.Model.ShoppingCart;

public interface ISubTotalGroup
{
	RentalShoppingCartItemAction Action { get; set; }

	decimal Amount { get; }

	decimal? DiscountedAmount { get; }

	decimal DiscountAmountUsed { get; }

	decimal TaxRate { get; }

	decimal TaxAmount { get; }

	decimal SubTotal { get; }

	decimal GrantTotal { get; }
}
