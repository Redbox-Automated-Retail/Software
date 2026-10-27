namespace Redbox.Rental.Services.Pricing;

public interface IPricingData
{
	IPricingDataInstance GetActiveInstance(bool force = false);

	void LoadStaged();
}
