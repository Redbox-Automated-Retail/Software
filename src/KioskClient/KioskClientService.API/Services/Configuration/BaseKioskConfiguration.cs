using KioskClientService.API.Services.Encryption;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace KioskClientService.API.Services.Configuration;

public class BaseKioskConfiguration : BaseConfiguration, IKioskConfiguration
{
	public CampaignConfiguration Campaigns { get; set; }

	public AssetsConfiguration Assets { get; set; }

	public KioskClientAds KioskClientAds { get; set; }

	public KioskClientDefault KioskClientDefault { get; set; }

	public KioskClientProxyApi KioskClientProxyApi { get; set; }

	public KioskClientSubscription KioskClientSubscriptions { get; set; }

	public DeviceServiceConfiguration DeviceService { get; set; }

	public BrowseConfiguration Browse { get; set; }

	public CMSConfiguration CMS { get; set; }

	public ProfileDataConfiguration ProfileData { get; set; }

	public MarketingGeneralConfiguration MarketingGeneral { get; set; }

	public MarketingAdConfiguration MarketingAd { get; set; }

	public MarketingReturnConfiguration MarketingReturn { get; set; }

	public ABTestingConfiguration ABTesting { get; set; }

	public KioskHealthConfiguration KioskHealth { get; set; }

	public PricingConfiguration Pricing { get; set; }

	public TestData TestData { get; set; }

	public BaseKioskConfiguration(ILogger<BaseKioskConfiguration> logger, IOptionsMonitor<KioskConfigurationSettings> optionsMonitorKioskConfigurationSettings, IEncryptionService encryptionService)
		: base(logger, optionsMonitorKioskConfigurationSettings, encryptionService)
	{
	}

	public BaseKioskConfiguration(ILogger<BaseKioskConfiguration> logger, IOptions<KioskConfigurationSettings> optionsKioskConfigurationSettings, IEncryptionService encryptionService)
		: base(logger, optionsKioskConfigurationSettings, encryptionService)
	{
	}
}
