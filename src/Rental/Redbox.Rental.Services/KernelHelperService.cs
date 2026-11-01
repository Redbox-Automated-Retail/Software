using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Rental.Model;

namespace Redbox.Rental.Services;

public class KernelHelperService : IKernelHelperService
{
	public void EnqueueStatistic(string name, string value)
	{
		IKernelService kernelService = ServiceLocator.Instance.GetService<IKernelService>();
		if (kernelService == null)
		{
			LogHelper.Instance.LogError("KernelHelper.HealthService_EnqueueStatistic", "IKernelService was not found");
		}
		else
		{
			kernelService.ExecuteChunk($"kernel.healthsvcs_enqueuestatistic('{name}','{value}')");
		}
	}
}
