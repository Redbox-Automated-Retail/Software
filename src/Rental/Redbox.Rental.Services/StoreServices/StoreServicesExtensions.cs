using Redbox.Core;
using Redbox.Rental.Model;

namespace Redbox.Rental.Services.StoreServices;

public static class StoreServicesExtensions
{
	public static void AddStoreServices(this ServiceLocator locator)
	{
		locator.AddService(typeof(IStoreServices), new StoreServices());
	}
}
