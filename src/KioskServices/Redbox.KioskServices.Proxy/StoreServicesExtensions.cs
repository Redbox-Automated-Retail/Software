using Redbox.Core;
using Redbox.KioskServices.Proxy.ComponentModel;

namespace Redbox.KioskServices.Proxy;

public static class StoreServicesExtensions
{
	public static void AddStoreServices(this ServiceLocator locator)
	{
		locator.AddService(typeof(IStoreServices), new StoreServices());
	}
}
