using Redbox.Core;
using Redbox.KioskDataServices.Proxy.ComponentModel;
using Redbox.KioskEngine.ComponentModel;

namespace Redbox.KioskDataServices.Proxy;

public class KioskSessionJsonAnalyticsService : IKioskSessionJsonAnalyticsService
{
	public static KioskSessionJsonAnalyticsService Instance => Singleton<KioskSessionJsonAnalyticsService>.Instance;

	private KioskSessionJsonAnalyticsService()
	{
		IQueueService service = ServiceLocator.Instance.GetService<IQueueService>();
		if (service != null)
		{
			service.RegisterMessageProcessorService(new AddKioskSessionJsonMessageProcessorService());
		}
		else
		{
			LogHelper.Instance.Log("Error in KioskSessionAnalyticsServices.RegisterWithQueueService.  Unable to find QueueService.");
		}
	}
}
