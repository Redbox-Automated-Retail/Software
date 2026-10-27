using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Xml;
using Redbox.Controls;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.ComponentModel.KioskServices;
using Redbox.KioskEngine.ComponentModel.TrackData;
using Redbox.KioskEngine.Environment;
using Redbox.Rental.Model;
using Redbox.Rental.Model.Analytics;
using Redbox.Rental.Model.DataService;
using Redbox.Rental.Model.Health;
using Redbox.Rental.Model.KioskClientService;
using Redbox.Rental.Model.KioskClientService.Session;
using Redbox.Rental.Model.KioskProduct;
using Redbox.Rental.Model.Personalization;
using Redbox.Rental.Model.Pricing;
using Redbox.Rental.Model.Session;
using Redbox.Rental.Model.ShoppingCart;
using Redbox.Rental.Services.Configuration;
using Redbox.Rental.Services.Controllers;
using Redbox.Rental.Services.KioskProduct;
using Redbox.Rental.Services.Session;
using Redbox.Rental.UI.Views;

namespace Redbox.Rental.Services;

public static class OrchestrationService
{
	private static Action _idleTimerExpireSessionCallback;

	private static Action _startNewRentalSessionCallback;

	private static Action _startIdleTimerCallback;

	private static Action _localizationReloadCallback;

	private static Action _finishVendCallback;

	private static Action _prepositionVendCallback;

	private static Action _prepositionVendAbortCallback;

	private static Func<bool> _getVendJobCompleteCallback;

	public static bool TTSEnabled => ServiceLocator.Instance.GetService<ITextToSpeechService>()?.TTSEnabled ?? false;

	private static IViewService ViewService => ServiceLocator.Instance.GetService<IViewService>();

	private static IAnalyticsService AnalyticsService => ServiceLocator.Instance.GetService<IAnalyticsService>();

	public static bool IsRentalSessionActive()
	{
		IViewService service = ServiceLocator.Instance.GetService<IViewService>();
		if (!PersonalizationFunctions.IsSignedIn)
		{
			if (service?.Peek() != service?.GetStartOverViewName())
			{
				return service?.Peek() != "maintenance_view";
			}
			return false;
		}
		return true;
	}

	public static bool IsPersonalizationSessionSignedIn()
	{
		return PersonalizationFunctions.IsSignedIn;
	}

	public static void CloseShoppingCartAnalytics(Guid sessionId, bool isSessionAbandoned)
	{
		IRentalShoppingCartService service = ServiceLocator.Instance.GetService<IRentalShoppingCartService>();
		IRentalShoppingCart rentalShoppingCart = service?.GetRentalShoppingCart(sessionId);
		if (rentalShoppingCart != null)
		{
			service.SendRentalShoppingCartToAnalytics(rentalShoppingCart);
			CloseAnalytics(sessionId, isSessionAbandoned);
		}
	}

	public static void CloseAnalytics(Guid sessionId, bool isSessionAbandoned)
	{
		AnalyticsService?.End(sessionId.ToString(), isSessionAbandoned);
	}

	public static void RegisterIdleTimerExpireSessionCallback(Action idleTimerExpireSessionCallback)
	{
		_idleTimerExpireSessionCallback = idleTimerExpireSessionCallback;
	}

	public static void RegisterStartNewRentalSessionCallback(Action startNewRentalSessionCallback)
	{
		_startNewRentalSessionCallback = startNewRentalSessionCallback;
	}

	public static void RegisterStartIdleTimerCallback(Action startIdleTimerCallback)
	{
		_startIdleTimerCallback = startIdleTimerCallback;
	}

	public static void RegisterLocalizationReloadCallback(Action localizationReloadCallback)
	{
		_localizationReloadCallback = localizationReloadCallback;
	}

	public static void RegisterFinishVendCallback(Action finishVendCallback)
	{
		_finishVendCallback = finishVendCallback;
	}

	public static void InvokeFinishVendCallback()
	{
		_finishVendCallback?.Invoke();
	}

	public static void RegisterPrepositionVendCallback(Action prepositionVendCallback)
	{
		_prepositionVendCallback = prepositionVendCallback;
	}

	public static void InvokePrepositionVendCallback()
	{
		_prepositionVendCallback?.Invoke();
	}

	public static void RegisterPrepositionVendAbortCallback(Action prepositionVendAbortCallback)
	{
		_prepositionVendAbortCallback = prepositionVendAbortCallback;
	}

	public static void InvokePrepositionVendAbortCallback()
	{
		_prepositionVendAbortCallback?.Invoke();
	}

	public static void RegisterIsVendJobCompletedCallback(Func<bool> getVendJobCompleteCallback)
	{
		_getVendJobCompleteCallback = getVendJobCompleteCallback;
	}

	public static bool InvokeIsVendJobCompletedCallback()
	{
		if (_getVendJobCompleteCallback != null)
		{
			return _getVendJobCompleteCallback();
		}
		return true;
	}

	public static void RegisterViewDefinitions()
	{
		ServiceLocator.Instance.GetService<IViewService>().ClearCache(clearResourceViewsOnly: false);
		WPFViewFrame.CacheWPFViewFrames("Redbox.Rental.Services.dll");
		WPFViewFrame.CacheWPFViewFrames("Redbox.FMA.UI.dll");
	}

	public static void InvokeIdleTimerExpireSessionCallback()
	{
		if (_idleTimerExpireSessionCallback != null)
		{
			_idleTimerExpireSessionCallback();
		}
	}

	public static void InvokeStartNewRentalSessionCallback()
	{
		if (_startNewRentalSessionCallback != null)
		{
			_startNewRentalSessionCallback();
		}
	}

	public static void InvokeStartIdleTimerCallback()
	{
		if (_startIdleTimerCallback != null)
		{
			_startIdleTimerCallback();
		}
	}

	public static void InvokeLocalizationReloadCallback()
	{
		if (_localizationReloadCallback != null)
		{
			_localizationReloadCallback();
		}
	}

	public static int GetIdleTimerTimeout()
	{
		int num = 40000;
		if (Redbox.Rental.Services.Configuration.Configuration.Instance != null && Redbox.Rental.Services.Configuration.Configuration.Instance.EarlyIdEnabled && Redbox.Rental.Services.Configuration.Configuration.Instance.SignInTimeout > 0 && Redbox.Rental.Services.Session.Session.Instance?.PersonalizationSession?.IsLoggedIn == true)
		{
			num = Redbox.Rental.Services.Configuration.Configuration.Instance.SignInTimeout;
		}
		ITextToSpeechService service = ServiceLocator.Instance.GetService<ITextToSpeechService>();
		IViewService service2 = ServiceLocator.Instance.GetService<IViewService>();
		if (service != null && service2 != null)
		{
			num = TTSGetTimeout((service2.Peek() == "help_document_view") ? "help" : "long", num);
		}
		return num;
	}

	public static int TTSGetTimeout(string timeoutType, int defaultTimeout)
	{
		ITextToSpeechService service = ServiceLocator.Instance.GetService<ITextToSpeechService>();
		if (service != null && service.TTSEnabled)
		{
			if (Redbox.Rental.Services.Configuration.Configuration.Instance.EarlyIdEnabled && Redbox.Rental.Services.Configuration.Configuration.Instance.SignInTimeout > 0)
			{
				IPersonalizationSession personalizationSession = Redbox.Rental.Services.Session.Session.Instance.PersonalizationSession;
				if (personalizationSession != null && personalizationSession.IsLoggedIn)
				{
					defaultTimeout = Redbox.Rental.Services.Configuration.Configuration.Instance.SignInADATimeout;
					goto IL_005f;
				}
			}
			defaultTimeout = service.GetTimeout(timeoutType, defaultTimeout);
		}
		goto IL_005f;
		IL_005f:
		return defaultTimeout;
	}

	public static void ResetApplicationIdleTimerTimeout()
	{
		IIdleTimerService service = ServiceLocator.Instance.GetService<IIdleTimerService>();
		if (service != null)
		{
			IIdleTimer idleTimer = service.GetIdleTimer("idle_timer");
			if (idleTimer != null && idleTimer.Timeout.GetValueOrDefault() != GetIdleTimerTimeout())
			{
				idleTimer.Timeout = GetIdleTimerTimeout();
			}
		}
	}

	public static void SetThemeAnimationState()
	{
		IThemeService service = ServiceLocator.Instance.GetService<IThemeService>();
		IConfiguration service2 = ServiceLocator.Instance.GetService<IConfiguration>();
		IViewService service3 = ServiceLocator.Instance.GetService<IViewService>();
		if (service?.CurrentTheme != null && service2 != null)
		{
			service.CurrentTheme.IsAnimated = service2.ButtonAnimationEnabled;
			RoundedButton.DefaultIsAnimated = service2.ButtonAnimationEnabled;
			RoundedButton.UITaskScheduler = service3?.UITaskScheduler;
		}
	}

	public static BitmapImage GetProductBitmapImage(IKioskProduct kioskProduct)
	{
		BitmapImage result = null;
		if (kioskProduct != null)
		{
			switch (kioskProduct.KioskProductType)
			{
			case KioskProductType.None:
				result = Redbox.Rental.UI.Views.Constants.Images.MovieMissingBoxArt;
				break;
			case KioskProductType.Title:
			case KioskProductType.MDV:
			case KioskProductType.TitleRollup:
				if (kioskProduct is ITitleProduct titleProduct)
				{
					if (DataServiceRegisterBitmapImage(titleProduct.ImageFile, titleProduct.ImageFile))
					{
						result = ServiceLocator.Instance.GetService<IBitmapCacheService>().GetBitmapImage(titleProduct.ImageFile);
					}
					else if (titleProduct.TitleFamily == TitleFamily.Movies)
					{
						result = Redbox.Rental.UI.Views.Constants.Images.MovieMissingBoxArt;
					}
					else if (titleProduct.TitleFamily == TitleFamily.Games)
					{
						result = Redbox.Rental.UI.Views.Constants.Images.GameMissingBoxArt;
					}
				}
				break;
			case KioskProductType.Subscription:
				if (kioskProduct is IRedboxPlusSubscriptionProduct)
				{
					result = Redbox.Rental.UI.Views.Constants.Images.RedboxPlusBoxArt;
				}
				break;
			default:
				result = Redbox.Rental.UI.Views.Constants.Images.MovieMissingBoxArt;
				break;
			}
		}
		return result;
	}

	public static void ComingSoonKioskOptIn()
	{
		string s = ServiceLocator.Instance.GetService<IMachineSettingsStore>()?.GetValue<string>("Store", "ID");
		IMarketingServices service = ServiceLocator.Instance.GetService<IMarketingServices>();
		List<IOptInItem> list = ServiceLocator.Instance.GetService<IRentalShoppingCartService>()?.CurrentRentalShoppingCart?.ComingSoonOptInInfo.OptInItems;
		if (list != null && list.Count > 0)
		{
			service.KioskOptIn(long.Parse(s), list, null);
		}
	}

	public static bool DataServiceRegisterImage(string name, string cacheName)
	{
		IBitmapCacheService service = ServiceLocator.Instance.GetService<IBitmapCacheService>();
		if (service.GetImage(name) != null)
		{
			return true;
		}
		byte[] image = ServiceLocator.Instance.GetService<IDataService>().GetImage(cacheName);
		if (image == null)
		{
			LogHelper.Instance.Log("OrchestrationService.RegisterImageFromCache was not able to read any data for name: {0} cacheName {1}", name, cacheName);
			return false;
		}
		return service.RegisterImage(name, image) != null;
	}

	public static bool DataServiceRegisterBitmapImage(string name, string cacheName)
	{
		IBitmapCacheService service = ServiceLocator.Instance.GetService<IBitmapCacheService>();
		if (service.GetBitmapImage(name) != null)
		{
			return true;
		}
		byte[] image = ServiceLocator.Instance.GetService<IDataService>().GetImage(cacheName);
		if (image == null)
		{
			LogHelper.Instance.Log("OrchestrationService.DataServiceRegisterBitmapImage was not able to read any data for name: {0} cacheName {1}", name, cacheName);
			return false;
		}
		return service.RegisterBitmapImage(name, image) != null;
	}

	public static void RegisterPricingService(Action pricingSetChangeCallback)
	{
		IPricingService service = ServiceLocator.Instance.GetService<IPricingService>();
		if (service == null)
		{
			return;
		}
		service.ClearOnPricingSetChange();
		service.OnPricingSetChange += delegate
		{
			LogHelper.Instance.Log(">  Price change detected; force web broker to re-register");
			if (pricingSetChangeCallback != null)
			{
				pricingSetChangeCallback();
			}
		};
	}

	public static void ReloadPricingService()
	{
		ServiceLocator.Instance.GetService<IPricingService>()?.Reload();
	}

	public static List<IKioskProduct> RentalGetCarouselProducts(int maxCarouselItems)
	{
		LogHelper.Instance.Log("RentalGetCarouselProducts");
		try
		{
			ErrorList carouselProducts = ServiceLocator.Instance.GetService<IDataService>().GetCarouselProducts(maxCarouselItems, out var products);
			if (carouselProducts.Count > 0)
			{
				carouselProducts.ToLogHelper();
				return null;
			}
			if (products == null)
			{
				LogHelper.Instance.Log("Error: OrchestrationService.RentalGetCarouselProducts return null");
				return null;
			}
			return products;
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("An exception was raised in OrchestrationService.RentalGetCarouselProducts", e);
			return null;
		}
	}

	public static bool RentalRefreshIfNeeded()
	{
		LogHelper.Instance.Log("OrchestrationService.RentalRefreshIfNeeded");
		try
		{
			LogHelper.Instance.Log("OrchestrationService.RentalRefreshIfNeeded - DataService");
			bool result = ServiceLocator.Instance.GetService<IDataService>().RefreshIfNeeded();
			LogHelper.Instance.Log("OrchestrationService.RentalRefreshIfNeeded - PricingService");
			ServiceLocator.Instance.GetService<IPricingService>().UpdatePricingInstance();
			ServiceLocator.Instance.GetService<IProductGroupService>()?.StartSession();
			return result;
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("An exception was raised in OrchestrationService.RentalRefreshIfNeeded", e);
			return false;
		}
	}

	public static void CreateStartViewAnalytics(string sessionStartTriggerActionName, string sessionType)
	{
		ISessionStartTriggerAction sessionStartTriggerAction = SessionStartTriggerActionList.Instance.FindByName(sessionStartTriggerActionName);
		if (sessionStartTriggerAction != null)
		{
			StartUserInteraction(sessionStartTriggerAction, Enum<AnalyticsSessionType>.ParseIgnoringCase(sessionType, AnalyticsSessionType.BrowseVend));
		}
	}

	public static string GetCulture()
	{
		return ServiceLocator.Instance.GetService<IResourceBundleService>()?.Filter["culture"] ?? null;
	}

	public static void SetCulture(string culture)
	{
		ServiceLocator.Instance.GetService<IResourceBundleService>().Filter["culture"] = culture;
		ServiceLocator.Instance.GetService<IAnalyticsService>()?.AddCurrentCulture(culture);
		ResetCurrentView();
	}

	public static void SetCurrentUICulture()
	{
		string text = ServiceLocator.Instance.GetService<IResourceBundleService>()?.Filter["culture"] ?? null;
		if (text != null)
		{
			string name = Thread.CurrentThread.CurrentUICulture.Name;
			if (!text.Equals(name))
			{
				Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo(text);
			}
		}
	}

	public static void ViewShow(string viewName)
	{
		ViewShow(viewName, showSafe: false, null);
	}

	private static void ViewShow(string viewName, bool showSafe, object parameter)
	{
		IViewService service = ServiceLocator.Instance.GetService<IViewService>();
		if (!showSafe || (service != null && service.IsSafeToChangeView()))
		{
			service?.Push(viewName, parameter);
			service?.Show();
		}
		else
		{
			LogHelper.Instance.Log("ViewShowSafe " + viewName + " denied.");
		}
	}

	public static void ViewShowSafe(string viewName, object parameter)
	{
		ViewShow(viewName, showSafe: true, parameter);
	}

	public static void ViewShowSafe(string viewName)
	{
		ViewShowSafe(viewName, null);
	}

	public static void ViewShow(string viewName, object parameter)
	{
		IViewService service = ServiceLocator.Instance.GetService<IViewService>();
		service?.Push(viewName, parameter);
		service?.Show();
	}

	public static void ShowCriticalErrorView(string viewName, ShowMessagePopupViewParameter parameter)
	{
		IViewService service = ServiceLocator.Instance.GetService<IViewService>();
		service?.PushCriticalErrorView(viewName, parameter);
		service?.Show();
	}

	public static string PeekView()
	{
		return ServiceLocator.Instance.GetService<IViewService>()?.Peek();
	}

	public static string PeekView(int index)
	{
		return ServiceLocator.Instance.GetService<IViewService>()?.Peek(index);
	}

	public static IViewFrameInstance PeekViewFrameByName(string viewName, out int index)
	{
		IViewService service = ServiceLocator.Instance.GetService<IViewService>();
		int index2 = -1;
		IViewFrameInstance result = service?.PeekViewFrameByName(viewName, out index2);
		index = index2;
		return result;
	}

	public static void ViewGoBack(object context, bool popOnly)
	{
		ServiceLocator.Instance.GetService<IViewService>()?.GoBack(context, popOnly);
	}

	public static void ViewGoBackSafe(object context, bool popOnly)
	{
		ServiceLocator.Instance.GetService<IViewService>()?.GoBackSafe(context, popOnly);
	}

	public static void UnwindToSafe(string unwindToViewName, bool discard, object context)
	{
		ServiceLocator.Instance.GetService<IViewService>()?.UnwindToSafe(unwindToViewName, discard, context);
	}

	public static void ResetCurrentView()
	{
		IViewService service = ServiceLocator.Instance.GetService<IViewService>();
		service.ClearCache(clearResourceViewsOnly: true);
		service.Show();
	}

	public static bool IsCurrentViewValid(string expectedViewName, bool logError = true)
	{
		string text = PeekView();
		bool num = text == expectedViewName;
		if (!num & logError)
		{
			LogHelper.Instance.Log("Invalid view.  Expected " + expectedViewName + " but found " + text);
		}
		return num;
	}

	public static bool IsCultureSpanish()
	{
		return (ServiceLocator.Instance.GetService<IResourceBundleService>()?.Filter["culture"] ?? null) == "es-MX";
	}

	public static string GetDefaultCulture()
	{
		return ServiceLocator.Instance.GetService<IStoreManager>()?.GetDefaultCulture();
	}

	public static string GetOtherCulture()
	{
		return ServiceLocator.Instance.GetService<IStoreManager>()?.GetAlternateCulture();
	}

	public static bool GetCanSellUsedMovies()
	{
		return ServiceLocator.Instance.GetService<IStoreManager>()?.CanSellUsedMovies ?? false;
	}

	public static object GetResourceProperty(string resourceName, string propertyName)
	{
		return ServiceLocator.Instance.GetService<IResourceBundleService>()?.GetResource(resourceName)?[propertyName];
	}

	public static List<Dictionary<string, object>> GetResourceTable(string name)
	{
		XmlNode xml = ServiceLocator.Instance.GetService<IResourceBundleService>().GetXml(name);
		if (xml == null)
		{
			return null;
		}
		XmlNodeList xmlNodeList = xml.SelectNodes("row");
		if (xmlNodeList == null)
		{
			return null;
		}
		List<Dictionary<string, object>> list = new List<Dictionary<string, object>>();
		foreach (XmlNode item in xmlNodeList)
		{
			XmlNodeList xmlNodeList2 = item.SelectNodes("column");
			if (xmlNodeList2 == null)
			{
				continue;
			}
			Dictionary<string, object> dictionary = new Dictionary<string, object>();
			foreach (XmlNode item2 in xmlNodeList2)
			{
				string attributeValue = item2.GetAttributeValue<string>("name");
				object value = item2.GetNodeValue(string.Empty);
				string attributeValue2 = item2.GetAttributeValue<string>("type");
				if (!string.IsNullOrEmpty(attributeValue2))
				{
					Type type = attributeValue2.ToType();
					if (type != null)
					{
						value = ConversionHelper.ChangeType(value, type);
					}
				}
				dictionary[attributeValue] = value;
			}
			list.Add(dictionary);
		}
		return list;
	}

	public static SessionType AnalyticsSessionTypeToKioskClientServiceSessionType(AnalyticsSessionType analyticsSessionType)
	{
		SessionType result = SessionType.None;
		switch (analyticsSessionType)
		{
		case AnalyticsSessionType.BrowseVend:
			result = SessionType.Browse;
			break;
		case AnalyticsSessionType.OnlinePickup:
			result = SessionType.Pickup;
			break;
		case AnalyticsSessionType.Return:
			result = SessionType.Return;
			break;
		case AnalyticsSessionType.QuickReturn:
			result = SessionType.Return;
			break;
		}
		return result;
	}

	public static void StartUserInteraction(ISessionStartTriggerAction userTriggerAction, AnalyticsSessionType sessionType = AnalyticsSessionType.BrowseVend)
	{
		LogHelper.Instance.Log($"OrchestrationService.StartUserInteraction - Session Type: {sessionType},  user trigger action: {userTriggerAction}");
		if (Redbox.Rental.Services.Session.Session.Instance != null)
		{
			ISession instance = Redbox.Rental.Services.Session.Session.Instance;
			if (instance != null && !instance.HasReceivedUserInteraction)
			{
				ServiceLocator.Instance.GetService<IKioskClientService>()?.Session?.Start(AnalyticsSessionTypeToKioskClientServiceSessionType(sessionType));
			}
			Redbox.Rental.Services.Session.Session.Instance?.SessionStartTriggerActions?.Add(userTriggerAction);
		}
		IShoppingSessionService service = ServiceLocator.Instance.GetService<IShoppingSessionService>();
		if (!service.StartUserInteraction())
		{
			LogHelper.Instance.Log("OrchestrationService.StartUserInteraction - session already started");
			return;
		}
		IShoppingSession currentSession = service.GetCurrentSession();
		if (currentSession != null)
		{
			_ = currentSession.Id;
			AnalyticsConstants.SessionType sessionType2 = AnalyticsConstants.SessionType.None;
			switch (sessionType)
			{
			case AnalyticsSessionType.BrowseVend:
				sessionType2 = AnalyticsConstants.SessionType.Browse;
				break;
			case AnalyticsSessionType.OnlinePickup:
				sessionType2 = AnalyticsConstants.SessionType.Pickup;
				break;
			case AnalyticsSessionType.Return:
				sessionType2 = AnalyticsConstants.SessionType.Return;
				break;
			}
			IAnalyticsService service2 = ServiceLocator.Instance.GetService<IAnalyticsService>();
			service2?.Start(currentSession.Id.ToString(), sessionType2);
			service2?.AddView(ViewService?.GetCurrentViewAnalyticsName());
			service2?.AddQueuedViewEvents();
		}
		Redbox.Rental.Services.Session.Session.Instance.BrowseState.EnableBrowseMessagePopup = Redbox.Rental.Services.Configuration.Configuration.Instance.KioskSession.MarketingGeneral.EnableBrowseMessagePopup;
		Redbox.Rental.Services.Session.Session.Instance.BrowseState.BrowseMessagePopupDuration = Redbox.Rental.Services.Configuration.Configuration.Instance.KioskSession.MarketingGeneral.BrowseMessagePopupDuration;
		Redbox.Rental.Services.Session.Session.Instance.BrowseState.ShowBrowseMessagePopup = Redbox.Rental.Services.Configuration.Configuration.Instance.KioskSession.MarketingGeneral.EnableBrowseMessagePopup;
		RentalRefreshIfNeeded();
		IKioskClientService kioskClientService = ServiceLocator.Instance.GetService<IKioskClientService>();
		IConfiguration service3 = ServiceLocator.Instance.GetService<IConfiguration>();
		IViewService service4 = ServiceLocator.Instance.GetService<IViewService>();
		if (kioskClientService?.Kiosk?.HasDual == true && service3 != null && service3.KioskSession.Browse.ShowDualInStock && service4 != null)
		{
			Task.Run(delegate
			{
				kioskClientService?.Dual?.UpdateTitles();
			}).ContinueWith(delegate
			{
				BrowseViewController.UpdateTitles();
			}, CancellationToken.None, TaskContinuationOptions.None, service4.UITaskScheduler);
		}
	}

	public static void AnalyticsAddLoggedInCustomerProfileNumber(string customerProfileNumber)
	{
		AnalyticsService?.AddSessionProperty("LoggedInCustomerProfileNumber", customerProfileNumber);
	}

	public static void RemoveTimer(string timerName)
	{
		ServiceLocator.Instance.GetService<ITimerService>()?.RemoveTimer(timerName);
	}

	public static void SetMacro(string name, string value)
	{
		IMacroService service = ServiceLocator.Instance.GetService<IMacroService>();
		if (service != null)
		{
			service[name] = value;
		}
	}

	public static string GetMacro(string name)
	{
		return ServiceLocator.Instance.GetService<IMacroService>()?[name];
	}

	public static object[] ExecuteScript(string name)
	{
		List<object> list = new List<object>();
		IKernelService service = ServiceLocator.Instance.GetService<IKernelService>();
		if (service != null)
		{
			ReadOnlyCollection<object> readOnlyCollection = service.ExecuteScript(name);
			if (readOnlyCollection != null)
			{
				list.AddRange(readOnlyCollection);
			}
		}
		return list.ToArray();
	}

	public static void SuspendIdleTimer(string idleTimerName)
	{
		(ServiceLocator.Instance.GetService<IIdleTimerService>()?.GetIdleTimer(idleTimerName))?.Suspend();
	}

	public static void ResetIdleTimer(string idleTimerName)
	{
		(ServiceLocator.Instance.GetService<IIdleTimerService>()?.GetIdleTimer(idleTimerName))?.Reset();
	}

	public static void ResumeIdleTimer(string idleTimerName)
	{
		(ServiceLocator.Instance.GetService<IIdleTimerService>()?.GetIdleTimer(idleTimerName))?.Resume();
	}

	public static void StopIdleTimer(string idleTimerName)
	{
		(ServiceLocator.Instance.GetService<IIdleTimerService>()?.GetIdleTimer(idleTimerName))?.Stop();
	}

	public static void StartIdleTimer(string idleTimerName, bool reset = false)
	{
		IIdleTimer idleTimer = ServiceLocator.Instance.GetService<IIdleTimerService>()?.GetIdleTimer(idleTimerName);
		idleTimer?.Start();
		if (reset)
		{
			idleTimer?.Reset();
		}
	}

	public static IIdleTimer GetIdleTimer(string idleTimerName)
	{
		return ServiceLocator.Instance.GetService<IIdleTimerService>()?.GetIdleTimer(idleTimerName);
	}

	public static IIdleTimer CreateIdleTimer(string idleTimerName)
	{
		IIdleTimerService service = ServiceLocator.Instance.GetService<IIdleTimerService>();
		IIdleTimer idleTimer = service?.GetIdleTimer(idleTimerName);
		if (idleTimer == null)
		{
			idleTimer = service?.CreateIdleTimer(idleTimerName);
		}
		return idleTimer;
	}

	public static void StartIdleTimer(int timeOutTime, Action idleTimerRoutine, string idleTimerName = null)
	{
		IIdleTimer idleTimer = GetIdleTimer(idleTimerName);
		if (idleTimer == null)
		{
			idleTimer = CreateIdleTimer(idleTimerName);
		}
		idleTimer?.Initialize(timeOutTime, idleTimerRoutine);
		idleTimer?.Start();
	}

	public static void RemoveIdleTimer(string idleTimerName)
	{
		ServiceLocator.Instance.GetService<IIdleTimerService>()?.RemoveIdleTimer(idleTimerName);
	}

	public static void StartTimer(string name)
	{
		(ServiceLocator.Instance.GetService<ITimerService>()?.GetTimer(name))?.Start();
	}

	public static void StopTimer(string timerName)
	{
		(ServiceLocator.Instance.GetService<ITimerService>()?.GetTimer(timerName))?.Stop();
	}

	public static bool IsGiftCard()
	{
		ITrackData trackData = Redbox.Rental.Services.Session.Session.Instance?.TrackData;
		if (trackData != null)
		{
			return trackData.CardType == CardType.RedboxGiftCard;
		}
		return false;
	}

	public static bool DoesShoppingCartContainAllReservedBarcodes()
	{
		bool result = true;
		IRentalShoppingCart rentalShoppingCart = ServiceLocator.Instance.GetService<IRentalShoppingCartService>()?.CurrentRentalShoppingCart;
		if (rentalShoppingCart != null)
		{
			foreach (string eachBarcode in ReservationFunctions.GetReservedBarcodesToVend())
			{
				if (!rentalShoppingCart.TitleItems.Any((IRentalShoppingCartTitleItem x) => x.Barcode == eachBarcode))
				{
					result = false;
					LogHelper.Instance.Log("Error: reserved barcode " + eachBarcode + " not found in shopping cart.");
				}
			}
		}
		return result;
	}

	public static void ShoppingCartCommit(string sessionId, string reason, bool shouldEndAndSave)
	{
		if (Redbox.Rental.Services.Configuration.Configuration.Instance.ComingSoonOptInEnabled)
		{
			ComingSoonKioskOptIn();
		}
		IShoppingSession shoppingSession = ServiceLocator.Instance.GetService<IShoppingSessionService>()?.GetSession(sessionId);
		shoppingSession?.Commit(reason);
		if (shouldEndAndSave)
		{
			shoppingSession?.End();
		}
	}

	public static void ShoppingCartEndSession(string sessionId)
	{
		IShoppingSession session = ServiceLocator.Instance.GetService<IShoppingSessionService>().GetSession(sessionId);
		if (session != null)
		{
			if (session.Status == ShoppingSessionStatus.Active)
			{
				session.Abandon("Session was not properly ended; marking as abandoned.");
			}
			session.End();
		}
	}

	public static void SendHealthServicesMinutesInMaintenanceModeStatistics(int minutesInMaintenanceMode)
	{
		ServiceLocator.Instance.GetService<IHealthServices>()?.EnqueueStatistic("MinutesInMaintenance", minutesInMaintenanceMode.ToString());
	}
}
