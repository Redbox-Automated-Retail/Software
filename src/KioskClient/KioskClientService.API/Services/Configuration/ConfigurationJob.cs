using System.Threading.Tasks;
using Coravel.Invocable;
using KioskClientService.API.Services.ProductGroup;
using KioskClientService.API.Services.Subscription;
using Microsoft.Extensions.Logging;

namespace KioskClientService.API.Services.Configuration;

public class ConfigurationJob : IConfigurationJob, IInvocable
{
	private ILogger<ConfigurationJob> _logger;

	private ISubscriptionService _subscriptionService;

	private IProductGroupSortOrderService _productGroupSortOrderService;

	private IKioskConfiguration _kioskConfiguration;

	private static long _lastConfigurationVersion;

	public ConfigurationJob(ILogger<ConfigurationJob> logger, IOptionsSnapshotKioskConfiguration optionsSnapshotKioskConfiguration, ISubscriptionService subscriptionService, IProductGroupSortOrderService productGroupSortOrderService)
	{
		_kioskConfiguration = optionsSnapshotKioskConfiguration;
		_logger = logger;
		_subscriptionService = subscriptionService;
		_productGroupSortOrderService = productGroupSortOrderService;
	}

	public async Task Invoke()
	{
		if (_lastConfigurationVersion == 0L)
		{
			_lastConfigurationVersion = _kioskConfiguration.ConfigurationVersion;
			_logger.LogInfoWithSource($"Initializing {GetType().Name} configuration version to {_lastConfigurationVersion}", "Invoke");
			return;
		}
		if (_lastConfigurationVersion == _kioskConfiguration.ConfigurationVersion)
		{
			_logger.LogInfoWithSource($"Configuration version has not changed.  verion: {_lastConfigurationVersion}", "Invoke");
			return;
		}
		_logger.LogInformation($"Configuration version changed from {_lastConfigurationVersion} to {_kioskConfiguration.ConfigurationVersion}");
		_lastConfigurationVersion = _kioskConfiguration.ConfigurationVersion;
		_kioskConfiguration.Log();
		await (_productGroupSortOrderService?.RefreshIfNeeded());
		await (_subscriptionService?.RefreshIfNeeded());
		_logger.LogInfoWithSource("Finished", "Invoke");
	}
}
