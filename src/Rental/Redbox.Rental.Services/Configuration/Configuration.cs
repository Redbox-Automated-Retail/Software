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
			IKioskClientService service = ServiceLocator.Instance.GetService<IKioskClientService>();
			if (service != null)
			{
				kioskConfiguration = service.Configuration?.GetCurrentKioskConfiguration(KioskClientServiceBaseUrl);
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
				IConfigurationService service = ServiceLocator.Instance.GetService<IConfigurationService>();
				_enabledGameTypes = new Dictionary<TitleType, bool>
				{
					{
						TitleType.Xbox360,
						GetGameConfig(service, "GameEnableXbox360")
					},
					{
						TitleType.XboxOne,
						GetGameConfig(service, "GameEnableXboxOne")
					},
					{
						TitleType.PS3,
						GetGameConfig(service, "GameEnablePS3")
					},
					{
						TitleType.PS4,
						GetGameConfig(service, "GameEnablePS4")
					},
					{
						TitleType.Wii,
						GetGameConfig(service, "GameEnableWii")
					},
					{
						TitleType.WiiU,
						GetGameConfig(service, "GameEnableWiiU")
					},
					{
						TitleType.NintendoSwitch,
						GetGameConfig(service, "GameEnableNintendoSwitch")
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
				IMachineSettingsStore service = ServiceLocator.Instance.GetService<IMachineSettingsStore>();
				_gameHasContentTypes = new Dictionary<TitleType, bool>
				{
					{
						TitleType.Xbox360,
						GetMachineSetting(service, "GameHasContentXbox360")
					},
					{
						TitleType.XboxOne,
						GetMachineSetting(service, "GameHasContentXboxOne")
					},
					{
						TitleType.PS3,
						GetMachineSetting(service, "GameHasContentPS3")
					},
					{
						TitleType.PS4,
						GetMachineSetting(service, "GameHasContentPS4")
					},
					{
						TitleType.Wii,
						GetMachineSetting(service, "GameHasContentWii")
					},
					{
						TitleType.WiiU,
						GetMachineSetting(service, "GameHasContentWiiU")
					},
					{
						TitleType.NintendoSwitch,
						GetMachineSetting(service, "GameHasContentNintendoSwitch")
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
		IConfigurationService service = ServiceLocator.Instance.GetService<IConfigurationService>();
		BurnInTimeout = ConfigGetValue(service, "system", "General", "BurnInTimeout", 1800000);
		BurnInViewDuration = ConfigGetValue(service, "system", "General", "BurnInViewDuration", 30000);
		SignInTimeout = ConfigGetValue(service, "system", "Personalization", "SignInTimeout", 20000);
		SignInADATimeout = ConfigGetValue(service, "system", "Personalization", "SignInADATimeout", 90000);
		EnablePricingEngine = ConfigGetValue(service, "system", "General", "EnablePricingEngine", defaultValue: true);
		KioskClientServiceBaseUrl = ConfigGetValue(service, "system", "KioskClientService", "KioskClientServiceBaseUrl", "http://localhost:9001");
		UpdateClientServiceBaseUrl = ConfigGetValue(service, "system", "UpdateClientService", "UpdateClientServiceBaseUrl", "http://localhost:6001");
		KioskEngineApiServiceBaseUrl = ConfigGetValue(service, "system", "KioskEngineApiService", "KioskEngineApiServiceBaseUrl", "http://localhost:9002");
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
		IConfigurationService service = ServiceLocator.Instance.GetService<IConfigurationService>();
		AnimatedDiscReturnEnabled = ConfigGetValue(service, "system", "General", "AnimatedDiscReturnEnabled", defaultValue: false);
		ComingSoonOptInEnabled = ConfigGetValue(service, "system", "General", "ComingSoonOptIn", defaultValue: true);
		MarketingOptInEnabled = ConfigGetValue(service, "system", "General", "MarketingOptInEnabled", defaultValue: true);
		RecommendationOnPickup = ConfigGetValue(service, "system", "General", "RecommendationOnPickup", defaultValue: true);
		ServiceFeeEnabled = ConfigGetValue(service, "system", "General", "ServiceFeeEnabled", defaultValue: false);
		BlurayUpsellEnabled = ConfigGetValue(service, "system", "General", "BluRayUpsellEnabled", defaultValue: false);
		_4KUpsellEnabled = ConfigGetValue(service, "system", "General", "4KUpsellEnabled", defaultValue: false);
		OfflineItemsLimit = ConfigGetValue(service, "system", "General", "OfflineItemsLimit", 3);
		OfflineGameRentalLimit = ConfigGetValue(service, "system", "General", "OfflineGameRentalLimit", 1);
		OfflineGamePurchaseLimit = ConfigGetValue(service, "system", "General", "OfflineGamePurchaseLimit", 2);
		KioskAnalyticsMessagePriority = ConfigGetValue(service, "system", "QueueService", "KioskAnalyticsMessagePriority", 0);
		UseTimerToInvokeCallbackEntries = ConfigGetValue(service, "system", "General", "UseTimerToInvokeCallbackEntries", defaultValue: true);
		GiftCardsEnabled = ConfigGetValue(service, "system", "General", "GiftCardsEnabled", defaultValue: true);
		TextToSpeechVolume = ConfigGetValue(service, "system", "General", "TextToSpeechVolume", 100);
		ButtonAnimationEnabled = ConfigGetValue(service, "system", "General", "ButtonAnimationEnabled", defaultValue: true);
		UseLowestPriceSortOrder = ConfigGetValue(service, "system", "General", "UserLowestPriceSortOrder", defaultValue: false);
		EarlyIdEnabled = ConfigGetValue(service, "system", "Personalization", "EarlyIDEnabled", defaultValue: false);
		LoyaltyEnabled = ConfigGetValue(service, "system", "Personalization", "LoyaltyEnabled", defaultValue: false);
		PerksPassEnabled = ConfigGetValue(service, "system", "Personalization", "PerksPassEnabled", defaultValue: false);
		_loyaltySignupAfterSwipeEnabled = ConfigGetValue(service, "system", "Personalization", "LoyaltySignUpAfterSwipeEnabled", defaultValue: false);
		string timeSpan = ConfigGetValue<string>(service, "system", "Personalization", "SignUpAfterSwipeStartEndTime", null);
		ParseTimeRange(timeSpan, out _loyaltySignupAfterSwipeStartTime, out _loyaltySignupAfterSwipeEndTime, "SignUpAfterSwipeStartEndTime");
		KeepHalImagesForNumberOfDays = ConfigGetValue(service, "system", "Fraud", "KeepHalImagesForNumberOfDays", 30);
		_priceFiltersOnBrowseABTestVersion = GetABTestValue(KioskSession.MarketingGeneral.PriceFiltersOnBrowse);
		_oosTitleDetailsABTestVersion = GetABTestValue(KioskSession.ABTesting.OutOfStockTitleDetails);
		AuthorizeAtPickup = ConfigGetValue(service, "system", "General", "AuthorizeAtPickup", defaultValue: false);
		StartViewDarkModeEnabled = ConfigGetValue(service, "system", "General", "StartViewDarkModeEnabled", defaultValue: false);
		StartViewDarkModeStartTime = ConfigGetValue(service, "system", "General", "StartViewDarkModeStartTime", "19:0:0");
		StartViewDarkModeEndTime = ConfigGetValue(service, "system", "General", "StartViewDarkModeEndTime", "7:0:0");
		KioskDataServicesUrl = ConfigGetValue<string>(service, "system", "General", "KioskDataServicesUrl", null);
		KioskDataServicesKey = ConfigGetValue<string>(service, "system", "General", "KioskDataServicesKey", null);
		SendJsonKioskAnalyticsToKioskDataService = ConfigGetValue(service, "system", "General", "SendJsonKioskAnalyticsToKioskDataService", defaultValue: true);
		ForcedAirExchangerConfigured = ConfigGetValue(service, "system", "General", "ForcedAirExchangerConfigured", defaultValue: true);
		FAQDocument = ConfigGetValue(service, "system", "Documents", "FAQDocument", "document-faqs");
		TermsDocument = ConfigGetValue(service, "system", "Documents", "TermsDocument", "document-terms");
		RedboxCardDocument = ConfigGetValue(service, "system", "Documents", "RedboxCardDocument", "document-terms-redbox-card");
		PrivacyDocument = ConfigGetValue(service, "system", "Documents", "PrivacyDocument", "document-privacy-policy");
		PerksDocument = ConfigGetValue(service, "system", "Documents", "PerksDocument", "document-perks");
		MobileFaqDocument = ConfigGetValue(service, "system", "Documents", "MobileFAQDocument", "document-mobile-faqs");
		CAPrivacyNoticeDocument = ConfigGetValue(service, "system", "Documents", "CAPrivacyNoticeDocument", string.Empty);
		DoNotSellDocument = ConfigGetValue(service, "system", "Documents", "DoNotSellDocument", string.Empty);
		SubscriptionsDocument = ConfigGetValue(service, "system", "Documents", "SubscriptionsDocument", string.Empty);
		RedboxPlusDocument = ConfigGetValue(service, "system", "Documents", "RedboxPlusDocument", string.Empty);
		UseMerchOrdersForThinning = ConfigGetValue(service, "system", "General", "UseMerchOrdersForThinning", defaultValue: false);
		KioskClientServiceGetMerchandizingOrdersTimeout = ConfigGetValue(service, "system", "General", "KioskClientServiceGetMerchandizingOrdersTimeout", 60000);
		string timeSpan2 = ConfigGetValue(service, "system", "General", "ThinJobTimeRange", "03:00:00-04:30:00");
		ParseTimeRange(timeSpan2, out _thinJobStartTime, out _thinJobEndTime, "ThinJobTimeRange");
		MerchOrderExpirationDays = ConfigGetValue(service, "system", "General", "MerchOrderExpirationDays", 7);
		ZeroTouchReadTimeout = ConfigGetValue(service, "system", "ZeroTouch", "ZeroTouchReadTimeout", 600000);
		ZeroTouchPickupEnabled = ConfigGetValue(service, "system", "ZeroTouch", "ZeroTouchPickupEnabled", defaultValue: true);
		ZeroTouchVasEnabled = ConfigGetValue(service, "system", "ZeroTouch", "ZeroTouchVasEnabled", defaultValue: false);
		ZeroTouchContactlessEnabled = ConfigGetValue(service, "system", "ZeroTouch", "ZeroTouchContactlessEnabled", defaultValue: true) && KioskSession.DeviceService.ContactlessEnabled;
		ZeroTouchEnabled = ConfigGetValue(service, "system", "ZeroTouch", "ZeroTouchEnabled", defaultValue: false) && ZeroTouchPickupEnabled;
		ZeroTouchGoogleTwoTap = ConfigGetValue(service, "system", "ZeroTouch", "ZeroTouchGoogleTwoTap", defaultValue: true);
		AuthorizeTimeout = ConfigGetValue(service, "system", "General", "AuthorizeTimeout", 60000);
		KioskClientServiceInventorySnapshotTimeout = ConfigGetValue(service, "kioskengine", "Jarvis", "KioskClientServiceInventorySnapshotTimeout", 60000);
		UseLegacyBrokerService = ConfigGetValue(service, "kioskengine", "Jarvis", "UseLegacyBrokerService", defaultValue: true);
		UseAWSBrokerService = ConfigGetValue(service, "kioskengine", "Jarvis", "UseAWSBrokerService", defaultValue: true);
		KioskClientServiceDiscActionTimeout = ConfigGetValue(service, "kioskengine", "Jarvis", "KioskClientServiceDiscActionTimeout", 5000);
		KioskClientServiceDiscFraudTimeout = ConfigGetValue(service, "kioskengine", "Jarvis", "KioskClientServiceDiscFraudTimeout", 5000);
		KioskClientServiceEstimateAccrualTimeout = ConfigGetValue(service, "kioskengine", "Jarvis", "KioskClientServiceEstimateAccrualTimeout", 5000);
		KioskClientServiceEstimateRedemptionTimeout = ConfigGetValue(service, "kioskengine", "Jarvis", "KioskClientServiceEstimateRedemptionTimeout", 5000);
		UseKCSPricing = ConfigGetValue(service, "kioskengine", "Jarvis", "UseKCSPricing", defaultValue: true);
		KioskClientServicePromoCodeValidationTimeout = ConfigGetValue(service, "kioskengine", "Jarvis", "KioskClientServicePromoCodeValidationTimeout", 5000);
		KioskClientServiceUpdateAccountTimeout = ConfigGetValue(service, "kioskengine", "Jarvis", "KioskClientServiceUpdateAccountTimeout", 3000);
		LoginLockoutMinutes = ConfigGetValue(service, "kioskengine", "FMACP", "LoginLockoutMinutes", 5);
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
			string[] array = timeSpan.Split('-');
			if (array.Count() == 2)
			{
				startTime = DateTime.ParseExact(array[0], "H:m:s", DateTimeFormatInfo.InvariantInfo).TimeOfDay;
				endTime = DateTime.ParseExact(array[1], "H:m:s", DateTimeFormatInfo.InvariantInfo).TimeOfDay;
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
