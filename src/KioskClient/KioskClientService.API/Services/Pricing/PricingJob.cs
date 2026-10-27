using System;
using System.Threading.Tasks;
using Coravel.Invocable;
using KioskClientService.API.Services.Configuration;
using Microsoft.Extensions.Logging;

namespace KioskClientService.API.Services.Pricing;

public class PricingJob : IInvocable
{
	public const int TimerIntervalMinutes = 5;

	private ILogger<PricingJob> _logger;

	private readonly IPricingService _pricingService;

	private readonly IKioskConfiguration _kioskConfiguration;

	public PricingJob(ILogger<PricingJob> logger, IPricingService pricingService, IOptionsSnapshotKioskConfiguration optionsSnapshotKioskConfiguration)
	{
		_logger = logger;
		_pricingService = pricingService;
		_kioskConfiguration = optionsSnapshotKioskConfiguration;
	}

	public async Task Invoke()
	{
		if (!_kioskConfiguration.KioskClientDefault.PricingActive)
		{
			_logger.LogInfoWithSource("Pricing is not active", "Invoke");
			return;
		}
		int num = new Random().Next(300000);
		_logger.LogInfoWithSource("Waiting random time of " + TimeSpan.FromMilliseconds(num).ToString("mm\\:ss") + " before calling RefreshIfNeeded", "Invoke");
		await Task.Delay(num);
		_logger.LogInfoWithSource("Invoking pricing Job to see if updates are needed", "Invoke");
		await _pricingService.RefreshIfNeeded();
		_logger.LogInfoWithSource("Finished", "Invoke");
	}
}
