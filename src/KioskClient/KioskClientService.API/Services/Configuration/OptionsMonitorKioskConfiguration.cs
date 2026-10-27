using KioskClientService.API.Services.Encryption;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace KioskClientService.API.Services.Configuration;

public class OptionsMonitorKioskConfiguration : BaseKioskConfiguration, IOptionsMonitorKioskConfiguration, IKioskConfiguration
{
	public OptionsMonitorKioskConfiguration(ILogger<OptionsMonitorKioskConfiguration> logger, IOptionsMonitor<KioskConfigurationSettings> optionsMonitorKioskConfigurationSettings, IEncryptionService encryptionService)
		: base(logger, optionsMonitorKioskConfigurationSettings, encryptionService)
	{
	}
}
