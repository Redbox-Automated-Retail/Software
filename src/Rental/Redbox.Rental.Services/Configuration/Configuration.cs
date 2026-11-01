using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Rental.Model;
using Redbox.Rental.Model.KioskClientService;
using Redbox.Rental.Model.KioskClientService.Configuration;
using Redbox.Rental.Model.KioskProduct;
using Redbox.Rental.Services.KioskClientService.Configuration;
using Redbox.Rental.Services.Session;

namespace Redbox.Rental.Services.Configuration;

public class Configuration : IConfiguration
{
	private Dictionary<TitleType, bool> _enabledGameTypes;

	private Dictionary<TitleType, bool> _gameHasContentTypes;

	private bool _loyaltySignupAfterSwipeEnabled;

	private TimeSpan _loyaltySignupAfterSwipeStartTime;

	private TimeSpan _loyaltySignupAfterSwipeEndTime;

	private string _priceFiltersOnBrowseABTestVersion;

	private string _oosTitleDetailsABTestVersion;

	private bool? _cachedIsDefaultCulture;

	private TimeSpan _thinJobStartTime;

	private TimeSpan _thinJobEndTime;

	private KioskConfiguration _lifetimeKioskConfiguration;

	private KioskConfiguration _sessionKioskConfiguration;

	public static IConfiguration Instance => Singleton<Configuration>.Instance;

	public KioskConfiguration Current
	{
		get
		{
			KioskConfiguration kioskConfiguration = null;
			IKioskClientService kioskClientService = ServiceLocator.Instance.GetService<IKioskClientService>();
			if (kioskClientService != null)
			{
				kioskConfiguration = kioskClientService.Configuration?.GetCurrentKioskConfiguration(KioskClientServiceBaseUrl);
				if (kioskConfiguration == null)
				{
					LogHelper.Instance.Log("Unable to get current KioskConfiguration from KioskClientService");
				}
			}
			else
			{
				kioskConfiguration = new KioskClientServiceConfiguration()?.GetCurrentKioskConfiguration(KioskClientServiceBaseUrl);
			}
			if (kioskConfiguration == null)
			{
				if (_lifetimeKioskConfiguration != null)
				{
					LogHelper.Instance.Log("Unable to get current KioskConfiguration.  Returning Lifetime values from app startup.");
					kioskConfiguration = _lifetimeKioskConfiguration;
				}
				else
				{
					LogHelper.Instance.Log("Unable to get current KioskConfiguration. Returning default values");
					kioskConfiguration = new KioskConfiguration();
				}
			}
			return kioskConfiguration;
		}
	}

	public KioskConfiguration KioskSession
	{
		get
		{
			if (_sessionKioskConfiguration == null)
			{
				return new KioskConfiguration();
			}
			return _sessionKioskConfiguration;
		}
	}

	public KioskConfiguration Lifetime
	{
		get
		{
			if (_lifetimeKioskConfiguration == null)
			{
				return new KioskConfiguration();
			}
			return _lifetimeKioskConfiguration;
		}
	}

	public Dictionary<TitleType, bool> EnabledGameTypes
	{
		get
		{
			if (_enabledGameTypes == null)
			{
				IConfigurationService configurationService = ServiceLocator.Instance.GetService<IConfigurationService>();
				_enabledGameTypes = new Dictionary<TitleType, bool>
				{
					{
						TitleType.Xbox360,
						GetGameConfig(configurationService, "GameEnableXbox360")
					},
					{
						TitleType.XboxOne,
						GetGameConfig(configurationService, "GameEnableXboxOne")
					},
					{
						TitleType.PS3,
						GetGameConfig(configurationService, "GameEnablePS3")
					},
					{
						TitleType.PS4,
						GetGameConfig(configurationService, "GameEnablePS4")
					},
					{
						TitleType.Wii,
						GetGameConfig(configurationService, "GameEnableWii")
					},
					{
						TitleType.WiiU,
						GetGameConfig(configurationService, "GameEnableWiiU")
					},
					{
						TitleType.NintendoSwitch,
						GetGameConfig(configurationService, "GameEnableNintendoSwitch")
					}
				};
			}
			return _enabledGameTypes;
		}
	}

	public Dictionary<TitleType, bool> GameHasContentTypes
	{
		get
		{
			if (_gameHasContentTypes == null)
			{
				IMachineSettingsStore machineSettingsStore = ServiceLocator.Instance.GetService<IMachineSettingsStore>();
				_gameHasContentTypes = new Dictionary<TitleType, bool>
				{
					{
						TitleType.Xbox360,
						GetMachineSetting(machineSettingsStore, "GameHasContentXbox360")
					},
					{
						TitleType.XboxOne,
						GetMachineSetting(machineSettingsStore, "GameHasContentXboxOne")
					},
					{
						TitleType.PS3,
						GetMachineSetting(machineSettingsStore, "GameHasContentPS3")
					},
					{
						TitleType.PS4,
						GetMachineSetting(machineSettingsStore, "GameHasContentPS4")
					},
					{
						TitleType.Wii,
						GetMachineSetting(machineSettingsStore, "GameHasContentWii")
					},
					{
						TitleType.WiiU,
						GetMachineSetting(machineSettingsStore, "GameHasContentWiiU")
					},
					{
						TitleType.NintendoSwitch,
						GetMachineSetting(machineSettingsStore, "GameHasContentNintendoSwitch")
					}
				};
			}
			return _gameHasContentTypes;
		}
	}

	public bool UseTimerToInvokeCallbackEntries { get; private set; }

	public bool GiftCardsEnabled { get; private set; }

	public bool AnimatedDiscReturnEnabled { get; private set; }

	public int BurnInTimeout { get; private set; }

	public int BurnInViewDuration { get; private set; }

	public bool EarlyIdEnabled { get; private set; }

	public bool ComingSoonOptInEnabled { get; private set; }

	public int TextToSpeechVolume { get; private set; }

	public bool MarketingOptInEnabled { get; private set; }

	public bool RecommendationOnPickup { get; private set; }

	public bool ShowOptinForNormalCheckout { get; private set; }

	public bool LoyaltyEnabled { get; private set; }

	public int SignInTimeout { get; private set; }

	public int SignInADATimeout { get; private set; }

	public bool EnablePricingEngine { get; private set; }

	public bool UseLowestPriceSortOrder { get; private set; }

	public bool ServiceFeeEnabled { get; private set; }

	public bool BlurayUpsellEnabled { get; private set; }

	public bool _4KUpsellEnabled { get; private set; }

	public int OfflineItemsLimit { get; private set; }

	public int OfflineGameRentalLimit { get; private set; }

	public int OfflineGamePurchaseLimit { get; private set; }

	public int KioskAnalyticsMessagePriority { get; private set; }

	public bool LoyaltySignUpAfterSwipeEnabled
	{
		get
		{
			if (_loyaltySignupAfterSwipeEnabled)
			{
				return IsCurrentTimeInRange(_loyaltySignupAfterSwipeStartTime, _loyaltySignupAfterSwipeEndTime);
			}
			return false;
		}
	}

	public int KeepHalImagesForNumberOfDays { get; private set; }

	public bool PerksPassEnabled { get; private set; }

	public string PriceFiltersOnBrowseABTestVersion => GetCultureBasedValue(_priceFiltersOnBrowseABTestVersion, "A");

	public string OutOfStockTitleDetailsABTestVersion => GetCultureBasedValue(_oosTitleDetailsABTestVersion, "A");

	public bool StartViewDarkModeEnabled { get; private set; }

	public string StartViewDarkModeStartTime { get; private set; }

	public string StartViewDarkModeEndTime { get; private set; }

	public string KioskClientServiceBaseUrl { get; private set; }

	public string UpdateClientServiceBaseUrl { get; private set; }

	public string KioskEngineApiServiceBaseUrl { get; private set; }

	public bool UseLegacyBrokerService { get; private set; }

	public bool UseAWSBrokerService { get; private set; }

	public bool AuthorizeAtPickup { get; private set; }

	public string KioskDataServicesUrl { get; private set; }

	// Base of the customer-facing website the title QR codes point to (Redbox used https://www.redbox.com).
	public string PublicWebsiteUrl { get; private set; }

	public string KioskDataServicesKey { get; private set; }

	public bool SendJsonKioskAnalyticsToKioskDataService { get; private set; }

	public bool ForcedAirExchangerConfigured { get; private set; }

	public string FAQDocument { get; private set; }

	public string TermsDocument { get; private set; }

	public string RedboxCardDocument { get; private set; }

	public string PrivacyDocument { get; private set; }

	public string PerksDocument { get; private set; }

	public string MobileFaqDocument { get; private set; }

	public string CAPrivacyNoticeDocument { get; private set; }

	public string DoNotSellDocument { get; private set; }

	public string SubscriptionsDocument { get; private set; }

	public string RedboxPlusDocument { get; private set; }

	public bool ZeroTouchEnabled { get; private set; }

	public bool ZeroTouchPickupEnabled { get; private set; }

	public bool ZeroTouchContactlessEnabled { get; private set; }

	public int ZeroTouchReadTimeout { get; private set; }

	public bool ZeroTouchVasEnabled { get; private set; }

	public bool ZeroTouchGoogleTwoTap { get; private set; }

	public bool UseMerchOrdersForThinning { get; private set; }

	public int KioskClientServiceGetMerchandizingOrdersTimeout { get; private set; }

	public TimeSpan ThinJobRangeStart => _thinJobStartTime;

	public TimeSpan ThinJobRangeEnd => _thinJobEndTime;

	public int MerchOrderExpirationDays { get; private set; }

	public int AuthorizeTimeout { get; private set; }

	public int KioskClientServiceInventorySnapshotTimeout { get; private set; }

	public int KioskClientServiceDiscActionTimeout { get; private set; }

	public int KioskClientServiceDiscFraudTimeout { get; private set; }

	public int KioskClientServiceEstimateAccrualTimeout { get; private set; }

	public int KioskClientServiceEstimateRedemptionTimeout { get; private set; }

	public int KioskClientServicePromoCodeValidationTimeout { get; private set; }

	public int KioskClientServiceUpdateAccountTimeout { get; private set; }

	public bool ButtonAnimationEnabled { get; private set; }

	public int LoginLockoutMinutes { get; private set; }

	public bool UseKCSPricing { get; private set; }

	private string GetCultureBasedValue(string defaultCultureValue, string alternateCultureValue)
	{
		if (!_cachedIsDefaultCulture.HasValue)
		{
			_cachedIsDefaultCulture = OrchestrationService.GetCulture() == OrchestrationService.GetDefaultCulture();
		}
		if (_cachedIsDefaultCulture.HasValue && !_cachedIsDefaultCulture.Value)
		{
			return alternateCultureValue;
		}
		return defaultCultureValue;
	}

	public void ResetCachedIsDefaultCulture()
	{
		_cachedIsDefaultCulture = null;
	}

	private Configuration()
	{
		Initialize();
	}

	private void Initialize()
	{
		ResetCachedIsDefaultCulture();
		IConfigurationService configurationService = ServiceLocator.Instance.GetService<IConfigurationService>();
		BurnInTimeout = ConfigGetValue(configurationService, "system", "General", "BurnInTimeout", 1800000);
		BurnInViewDuration = ConfigGetValue(configurationService, "system", "General", "BurnInViewDuration", 30000);
		SignInTimeout = ConfigGetValue(configurationService, "system", "Personalization", "SignInTimeout", 20000);
		SignInADATimeout = ConfigGetValue(configurationService, "system", "Personalization", "SignInADATimeout", 90000);
		EnablePricingEngine = ConfigGetValue(configurationService, "system", "General", "EnablePricingEngine", defaultValue: true);
		KioskClientServiceBaseUrl = ConfigGetValue(configurationService, "system", "KioskClientService", "KioskClientServiceBaseUrl", "http://localhost:9001");
		UpdateClientServiceBaseUrl = ConfigGetValue(configurationService, "system", "UpdateClientService", "UpdateClientServiceBaseUrl", "http://localhost:6001");
		KioskEngineApiServiceBaseUrl = ConfigGetValue(configurationService, "system", "KioskEngineApiService", "KioskEngineApiServiceBaseUrl", "http://localhost:9002");
		_lifetimeKioskConfiguration = new KioskClientServiceConfiguration()?.GetCurrentKioskConfiguration(KioskClientServiceBaseUrl);
		LogHelper.Instance.Log("Loaded static KioskConfiguration");
		_lifetimeKioskConfiguration?.Log();
		SessionReload();
	}

	public void SessionReload()
	{
		_sessionKioskConfiguration = ServiceLocator.Instance.GetService<IKioskClientService>()?.Configuration?.GetKioskSessionKioskConfiguration(KioskClientServiceBaseUrl);
		if (_sessionKioskConfiguration == null)
		{
			_sessionKioskConfiguration = _lifetimeKioskConfiguration;
		}
		else
		{
			LogHelper.Instance.Log($"Loaded KioskConfiguration for KioskSession id {Redbox.Rental.Services.Session.Session.Instance?.KioskSessionId}");
			_sessionKioskConfiguration?.Log();
		}
		ResetCachedIsDefaultCulture();
		IConfigurationService configurationService = ServiceLocator.Instance.GetService<IConfigurationService>();
		AnimatedDiscReturnEnabled = ConfigGetValue(configurationService, "system", "General", "AnimatedDiscReturnEnabled", defaultValue: false);
		ComingSoonOptInEnabled = ConfigGetValue(configurationService, "system", "General", "ComingSoonOptIn", defaultValue: true);
		MarketingOptInEnabled = ConfigGetValue(configurationService, "system", "General", "MarketingOptInEnabled", defaultValue: true);
		RecommendationOnPickup = ConfigGetValue(configurationService, "system", "General", "RecommendationOnPickup", defaultValue: true);
		ServiceFeeEnabled = ConfigGetValue(configurationService, "system", "General", "ServiceFeeEnabled", defaultValue: false);
		BlurayUpsellEnabled = ConfigGetValue(configurationService, "system", "General", "BluRayUpsellEnabled", defaultValue: false);
		_4KUpsellEnabled = ConfigGetValue(configurationService, "system", "General", "4KUpsellEnabled", defaultValue: false);
		OfflineItemsLimit = ConfigGetValue(configurationService, "system", "General", "OfflineItemsLimit", 3);
		OfflineGameRentalLimit = ConfigGetValue(configurationService, "system", "General", "OfflineGameRentalLimit", 1);
		OfflineGamePurchaseLimit = ConfigGetValue(configurationService, "system", "General", "OfflineGamePurchaseLimit", 2);
		KioskAnalyticsMessagePriority = ConfigGetValue(configurationService, "system", "QueueService", "KioskAnalyticsMessagePriority", 0);
		UseTimerToInvokeCallbackEntries = ConfigGetValue(configurationService, "system", "General", "UseTimerToInvokeCallbackEntries", defaultValue: true);
		GiftCardsEnabled = ConfigGetValue(configurationService, "system", "General", "GiftCardsEnabled", defaultValue: true);
		TextToSpeechVolume = ConfigGetValue(configurationService, "system", "General", "TextToSpeechVolume", 100);
		ButtonAnimationEnabled = ConfigGetValue(configurationService, "system", "General", "ButtonAnimationEnabled", defaultValue: true);
		UseLowestPriceSortOrder = ConfigGetValue(configurationService, "system", "General", "UserLowestPriceSortOrder", defaultValue: false);
		EarlyIdEnabled = ConfigGetValue(configurationService, "system", "Personalization", "EarlyIDEnabled", defaultValue: false);
		LoyaltyEnabled = ConfigGetValue(configurationService, "system", "Personalization", "LoyaltyEnabled", defaultValue: false);
		PerksPassEnabled = ConfigGetValue(configurationService, "system", "Personalization", "PerksPassEnabled", defaultValue: false);
		_loyaltySignupAfterSwipeEnabled = ConfigGetValue(configurationService, "system", "Personalization", "LoyaltySignUpAfterSwipeEnabled", defaultValue: false);
		string timeSpan = ConfigGetValue<string>(configurationService, "system", "Personalization", "SignUpAfterSwipeStartEndTime", null);
		ParseTimeRange(timeSpan, out _loyaltySignupAfterSwipeStartTime, out _loyaltySignupAfterSwipeEndTime, "SignUpAfterSwipeStartEndTime");
		KeepHalImagesForNumberOfDays = ConfigGetValue(configurationService, "system", "Fraud", "KeepHalImagesForNumberOfDays", 30);
		_priceFiltersOnBrowseABTestVersion = GetABTestValue(KioskSession.MarketingGeneral.PriceFiltersOnBrowse);
		_oosTitleDetailsABTestVersion = GetABTestValue(KioskSession.ABTesting.OutOfStockTitleDetails);
		AuthorizeAtPickup = ConfigGetValue(configurationService, "system", "General", "AuthorizeAtPickup", defaultValue: false);
		StartViewDarkModeEnabled = ConfigGetValue(configurationService, "system", "General", "StartViewDarkModeEnabled", defaultValue: false);
		StartViewDarkModeStartTime = ConfigGetValue(configurationService, "system", "General", "StartViewDarkModeStartTime", "19:0:0");
		StartViewDarkModeEndTime = ConfigGetValue(configurationService, "system", "General", "StartViewDarkModeEndTime", "7:0:0");
		KioskDataServicesUrl = ConfigGetValue<string>(configurationService, "system", "General", "KioskDataServicesUrl", null);
		PublicWebsiteUrl = ConfigGetValue(configurationService, "system", "General", "PublicWebsiteUrl", "https://www.kiosk-backend.invalid").TrimEnd('/');
		KioskDataServicesKey = ConfigGetValue<string>(configurationService, "system", "General", "KioskDataServicesKey", null);
		SendJsonKioskAnalyticsToKioskDataService = ConfigGetValue(configurationService, "system", "General", "SendJsonKioskAnalyticsToKioskDataService", defaultValue: true);
		ForcedAirExchangerConfigured = ConfigGetValue(configurationService, "system", "General", "ForcedAirExchangerConfigured", defaultValue: true);
		FAQDocument = ConfigGetValue(configurationService, "system", "Documents", "FAQDocument", "document-faqs");
		TermsDocument = ConfigGetValue(configurationService, "system", "Documents", "TermsDocument", "document-terms");
		RedboxCardDocument = ConfigGetValue(configurationService, "system", "Documents", "RedboxCardDocument", "document-terms-redbox-card");
		PrivacyDocument = ConfigGetValue(configurationService, "system", "Documents", "PrivacyDocument", "document-privacy-policy");
		PerksDocument = ConfigGetValue(configurationService, "system", "Documents", "PerksDocument", "document-perks");
		MobileFaqDocument = ConfigGetValue(configurationService, "system", "Documents", "MobileFAQDocument", "document-mobile-faqs");
		CAPrivacyNoticeDocument = ConfigGetValue(configurationService, "system", "Documents", "CAPrivacyNoticeDocument", string.Empty);
		DoNotSellDocument = ConfigGetValue(configurationService, "system", "Documents", "DoNotSellDocument", string.Empty);
		SubscriptionsDocument = ConfigGetValue(configurationService, "system", "Documents", "SubscriptionsDocument", string.Empty);
		RedboxPlusDocument = ConfigGetValue(configurationService, "system", "Documents", "RedboxPlusDocument", string.Empty);
		UseMerchOrdersForThinning = ConfigGetValue(configurationService, "system", "General", "UseMerchOrdersForThinning", defaultValue: false);
		KioskClientServiceGetMerchandizingOrdersTimeout = ConfigGetValue(configurationService, "system", "General", "KioskClientServiceGetMerchandizingOrdersTimeout", 60000);
		string timeSpan2 = ConfigGetValue(configurationService, "system", "General", "ThinJobTimeRange", "03:00:00-04:30:00");
		ParseTimeRange(timeSpan2, out _thinJobStartTime, out _thinJobEndTime, "ThinJobTimeRange");
		MerchOrderExpirationDays = ConfigGetValue(configurationService, "system", "General", "MerchOrderExpirationDays", 7);
		ZeroTouchReadTimeout = ConfigGetValue(configurationService, "system", "ZeroTouch", "ZeroTouchReadTimeout", 600000);
		ZeroTouchPickupEnabled = ConfigGetValue(configurationService, "system", "ZeroTouch", "ZeroTouchPickupEnabled", defaultValue: true);
		ZeroTouchVasEnabled = ConfigGetValue(configurationService, "system", "ZeroTouch", "ZeroTouchVasEnabled", defaultValue: false);
		ZeroTouchContactlessEnabled = ConfigGetValue(configurationService, "system", "ZeroTouch", "ZeroTouchContactlessEnabled", defaultValue: true) && KioskSession.DeviceService.ContactlessEnabled;
		ZeroTouchEnabled = ConfigGetValue(configurationService, "system", "ZeroTouch", "ZeroTouchEnabled", defaultValue: false) && ZeroTouchPickupEnabled;
		ZeroTouchGoogleTwoTap = ConfigGetValue(configurationService, "system", "ZeroTouch", "ZeroTouchGoogleTwoTap", defaultValue: true);
		AuthorizeTimeout = ConfigGetValue(configurationService, "system", "General", "AuthorizeTimeout", 60000);
		KioskClientServiceInventorySnapshotTimeout = ConfigGetValue(configurationService, "kioskengine", "Jarvis", "KioskClientServiceInventorySnapshotTimeout", 60000);
		UseLegacyBrokerService = ConfigGetValue(configurationService, "kioskengine", "Jarvis", "UseLegacyBrokerService", defaultValue: true);
		UseAWSBrokerService = ConfigGetValue(configurationService, "kioskengine", "Jarvis", "UseAWSBrokerService", defaultValue: true);
		KioskClientServiceDiscActionTimeout = ConfigGetValue(configurationService, "kioskengine", "Jarvis", "KioskClientServiceDiscActionTimeout", 5000);
		KioskClientServiceDiscFraudTimeout = ConfigGetValue(configurationService, "kioskengine", "Jarvis", "KioskClientServiceDiscFraudTimeout", 5000);
		KioskClientServiceEstimateAccrualTimeout = ConfigGetValue(configurationService, "kioskengine", "Jarvis", "KioskClientServiceEstimateAccrualTimeout", 5000);
		KioskClientServiceEstimateRedemptionTimeout = ConfigGetValue(configurationService, "kioskengine", "Jarvis", "KioskClientServiceEstimateRedemptionTimeout", 5000);
		UseKCSPricing = ConfigGetValue(configurationService, "kioskengine", "Jarvis", "UseKCSPricing", defaultValue: true);
		KioskClientServicePromoCodeValidationTimeout = ConfigGetValue(configurationService, "kioskengine", "Jarvis", "KioskClientServicePromoCodeValidationTimeout", 5000);
		KioskClientServiceUpdateAccountTimeout = ConfigGetValue(configurationService, "kioskengine", "Jarvis", "KioskClientServiceUpdateAccountTimeout", 3000);
		LoginLockoutMinutes = ConfigGetValue(configurationService, "kioskengine", "FMACP", "LoginLockoutMinutes", 5);
	}

	private bool GetMachineSetting(IMachineSettingsStore machineSettingStore, string key)
	{
		bool result = false;
		if (machineSettingStore != null)
		{
			bool.TryParse(machineSettingStore.GetValue("Store", key, "false"), out result);
		}
		return result;
	}

	private bool GetGameConfig(IConfigurationService configService, string settingName)
	{
		bool value = false;
		if (configService != null && !configService.TryGetValue<bool>("system", "Games", settingName, out value))
		{
			value = false;
		}
		return value;
	}

	private static T ConfigGetValue<T>(IConfigurationService configService, string configFile, string categoryName, string settingName, T defaultValue)
	{
		T value = defaultValue;
		if (configService != null && !configService.TryGetValue<T>(configFile, categoryName, settingName, out value))
		{
			value = defaultValue;
		}
		return value;
	}

	private string GetABTestValue(string configValue)
	{
		string result = configValue;
		if (string.IsNullOrEmpty(configValue) || configValue == "OFF")
		{
			result = "A";
		}
		return result;
	}

	private void ParseTimeRange(string timeSpan, out TimeSpan startTime, out TimeSpan endTime, string configSetting)
	{
		startTime = TimeSpan.MinValue;
		endTime = TimeSpan.MinValue;
		if (string.IsNullOrEmpty(timeSpan))
		{
			return;
		}
		try
		{
			string[] parts = timeSpan.Split('-');
			if (parts.Count() == 2)
			{
				startTime = DateTime.ParseExact(parts[0], "H:m:s", DateTimeFormatInfo.InvariantInfo).TimeOfDay;
				endTime = DateTime.ParseExact(parts[1], "H:m:s", DateTimeFormatInfo.InvariantInfo).TimeOfDay;
			}
			else
			{
				LogHelper.Instance.Log("Invalid time span format used for " + configSetting + ". Value read was " + timeSpan);
			}
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("Unable to parse time span for " + configSetting + ". Value read was " + timeSpan, e);
			startTime = TimeSpan.MinValue;
			endTime = TimeSpan.MinValue;
		}
	}

	private static bool IsCurrentTimeInRange(TimeSpan startTime, TimeSpan endTime)
	{
		TimeSpan timeOfDay = DateTime.Now.TimeOfDay;
		if (startTime < endTime)
		{
			if (timeOfDay >= startTime)
			{
				return timeOfDay < endTime;
			}
			return false;
		}
		if (!(timeOfDay >= startTime))
		{
			return timeOfDay < endTime;
		}
		return true;
	}
}
