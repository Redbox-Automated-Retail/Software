using KioskClientService.API.Services.Encryption;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace KioskClientService.API.Services.Configuration;

public class OptionsSnapshotKioskConfiguration : BaseKioskConfiguration, IOptionsSnapshotKioskConfiguration, IKioskConfiguration
{
	public OptionsSnapshotKioskConfiguration(ILogger<OptionsSnapshotKioskConfiguration> logger, IOptionsSnapshot<KioskConfigurationSettings> optionsSnapshotKioskConfigurationSettings, IEncryptionService encryptionService)
		: base(logger, optionsSnapshotKioskConfigurationSettings, encryptionService)
	{
	}
}
