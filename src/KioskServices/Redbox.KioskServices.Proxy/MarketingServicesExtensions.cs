using Redbox.Core;
using Redbox.Rental.Model.Personalization;

namespace Redbox.KioskServices.Proxy;

public static class MarketingServicesExtensions
{
	public static void AddMarketingServices(this ServiceLocator locator)
	{
		locator.AddService(typeof(IMarketingServices), new MarketingServices());
	}
}
