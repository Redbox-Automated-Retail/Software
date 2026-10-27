using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using UpdateClientService.API.Services.IoT.Certificate.Security;

namespace UpdateClientService.API.Services.Configuration;

public class BaseKioskConfiguration : BaseConfiguration, IKioskConfiguration
{
	public Operations Operations { get; set; }

	public FileSets FileSets { get; set; }

	public KioskHealthConfiguration KioskHealth { get; set; }

	public KioskClientProxyApi KioskClientProxyApi { get; set; }

	public BaseKioskConfiguration(ILogger<BaseKioskConfiguration> logger, IOptionsMonitor<KioskConfigurationSettings> optionsMonitorKioskConfigurationSettings, IEncryptionService encryptionService)
		: base(logger, optionsMonitorKioskConfigurationSettings, encryptionService)
	{
	}

	public BaseKioskConfiguration(ILogger<BaseKioskConfiguration> logger, IOptions<KioskConfigurationSettings> optionsKioskConfigurationSettings, IEncryptionService encryptionService)
		: base(logger, optionsKioskConfigurationSettings, encryptionService)
	{
	}
}
