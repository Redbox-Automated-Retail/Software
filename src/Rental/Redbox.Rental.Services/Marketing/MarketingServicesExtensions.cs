using Redbox.Core;
using Redbox.Rental.Model.Personalization;

namespace Redbox.Rental.Services.Marketing;

public static class MarketingServicesExtensions
{
	public static void AddMarketingServices(this ServiceLocator locator)
	{
		locator.AddService(typeof(IMarketingServices), new MarketingServices());
	}
}
