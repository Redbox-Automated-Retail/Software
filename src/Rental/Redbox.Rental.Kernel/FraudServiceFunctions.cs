using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Rental.Model;

namespace Redbox.Rental.Kernel;

public class FraudServiceFunctions
{
	[KernelFunction(Name = "kernel.send_fraud_clean_statistic")]
	internal static void SendFraudCleanStatistic(string count)
	{
		ServiceLocator.Instance.GetService<IFraudService>()?.AddToFraudStatistics(count);
	}

	[KernelFunction(Name = "kernel.clean_fraud_images")]
	internal static void CleanFraudImages()
	{
		ServiceLocator.Instance.GetService<IFraudService>()?.CleanImageFolder();
	}
}
