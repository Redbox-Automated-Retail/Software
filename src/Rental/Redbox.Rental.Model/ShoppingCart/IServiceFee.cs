namespace Redbox.Rental.Model.ShoppingCart;

public interface IServiceFee
{
	decimal DefaultAmount { get; set; }

	decimal ActualAmount { get; set; }

	decimal TaxRate { get; set; }

	decimal TaxAmount { get; }
}
