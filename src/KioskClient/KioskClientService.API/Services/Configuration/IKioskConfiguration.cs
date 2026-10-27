namespace KioskClientService.API.Services.Configuration;

public interface IKioskConfiguration
{
	long ConfigurationVersion { get; }

	CampaignConfiguration Campaigns { get; }

	AssetsConfiguration Assets { get; }

	KioskClientAds KioskClientAds { get; }

	KioskClientDefault KioskClientDefault { get; }

	KioskClientProxyApi KioskClientProxyApi { get; }

	KioskClientSubscription KioskClientSubscriptions { get; }

	DeviceServiceConfiguration DeviceService { get; }

	BrowseConfiguration Browse { get; }

	CMSConfiguration CMS { get; }

	ProfileDataConfiguration ProfileData { get; }

	MarketingGeneralConfiguration MarketingGeneral { get; }

	MarketingAdConfiguration MarketingAd { get; }

	MarketingReturnConfiguration MarketingReturn { get; }

	ABTestingConfiguration ABTesting { get; }

	KioskHealthConfiguration KioskHealth { get; }

	PricingConfiguration Pricing { get; }

	TestData TestData { get; }

	void Log(ConfigLogParameters loggingParameters = null);
}
