namespace Redbox.Rental.Model.KioskProduct;

public interface ISubscriptionProduct : IKioskProduct
{
	string SubscriptionId { get; set; }

	SubscriptionType SubscriptionType { get; set; }

	decimal Price { get; set; }

	decimal TaxRate { get; set; }

	int CompareTo(ISubscriptionProduct subscriptionProduct);
}
