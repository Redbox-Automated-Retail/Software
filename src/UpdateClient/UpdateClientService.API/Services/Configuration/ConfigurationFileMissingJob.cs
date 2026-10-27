using System.Threading.Tasks;
using Coravel.Invocable;
using Microsoft.Extensions.Logging;
using Redbox.NetCore.Logging.Extensions;

namespace UpdateClientService.API.Services.Configuration;

public class ConfigurationFileMissingJob : IConfigurationFileMissingJob, IInvocable
{
	private IConfigurationService _configurationService;

	private readonly IKioskConfiguration _kioskConfiguration;

	private ILogger<ConfigurationFileMissingJob> _logger;

	public ConfigurationFileMissingJob(ILogger<ConfigurationFileMissingJob> logger, IConfigurationService configurationService, IOptionsSnapshotKioskConfiguration kioskConfiguration)
	{
		_configurationService = configurationService;
		_kioskConfiguration = kioskConfiguration;
		_logger = logger;
	}

	public async Task Invoke()
	{
		if (_kioskConfiguration.ConfigurationVersion == 0L)
		{
			_logger.LogWarningWithSource("KioskConfiguration is not current.  Getting current configuration version...", "Invoke", "/sln/src/UpdateClientService.API/Services/Configuration/ConfigurationFileMissingJob.cs");
			await _configurationService.GetKioskConfigurationSettingChanges(null);
		}
	}
}
