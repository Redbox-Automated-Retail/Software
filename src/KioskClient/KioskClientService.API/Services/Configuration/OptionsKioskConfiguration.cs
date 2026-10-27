using KioskClientService.API.Services.Encryption;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace KioskClientService.API.Services.Configuration;

public class OptionsKioskConfiguration : BaseKioskConfiguration, IOptionsKioskConfiguration, IKioskConfiguration
{
	public OptionsKioskConfiguration(ILogger<OptionsKioskConfiguration> logger, IOptions<KioskConfigurationSettings> optionsKioskConfigurationSettings, IEncryptionService encryptionService)
		: base(logger, optionsKioskConfigurationSettings, encryptionService)
	{
	}
}
