using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DeviceService.ComponentModel;
using Redbox.Core;
using Redbox.HardwareServices.Proxy.ComponentModel;
using Redbox.KioskEngine.ComponentModel;
using ITimer = Redbox.KioskEngine.ComponentModel.ITimer;
using Redbox.KioskEngine.ComponentModel.TrackData;
using Redbox.KioskEngine.Environment;
using Redbox.REDS.Framework;
using Redbox.Rental.Model;
using Redbox.Rental.Model.DataService;
using Redbox.Rental.Model.KioskClientService;
using Redbox.Rental.Model.KioskHealth;
using Redbox.Rental.Model.KioskProduct;
using Redbox.Rental.Model.Profile;
using Redbox.Rental.Model.Session;
using Redbox.Rental.Model.ShoppingCart;
using Redbox.Rental.Services.Configuration;
using Redbox.Rental.Services.Controllers;
using Redbox.Rental.Services.Session;

namespace Redbox.Rental.Services;

public class RedboxRentalService : IRedboxRental
{
	public class Constants
	{
		public class Timers
		{
			public const string ReservationExpireTimer = "reservation-expire-timer";
		}

		public const string DisconnectedCardReaderTimer = "disconnected_usb_card_reader_timer";
	}

	private IViewService ViewService => ServiceLocator.Instance.GetService<IViewService>();

	private bool IsRentalApplicationRunning => ServiceLocator.Instance.GetService<IResourceBundleService>()?.IsRentalRunning ?? false;

	public Redbox.KioskEngine.ComponentModel.ErrorList Initialize()
	{
		Redbox.KioskEngine.ComponentModel.ErrorList errorList = new Redbox.KioskEngine.ComponentModel.ErrorList();
		try
		{
			InitializeDesktop();
		}
		catch (Exception e)
		{
			errorList.Add(Redbox.KioskEngine.ComponentModel.Error.NewError("E008", "Exception in EngineApplication.InitializeRedboxRental", e));
		}
		return errorList;
	}

	public bool IsRentalSessionActive()
	{
		return OrchestrationService.IsRentalSessionActive();
	}

	public void SetupTheRentalApplication()
	{
		HardwareHelper.TurnOnAirExchangerFanIfConfigured(ServiceLocator.Instance.GetService<IConfiguration>()?.ForcedAirExchangerConfigured ?? true, ServiceLocator.Instance.GetService<IResourceBundleService>()?.CurrentBundleName);
		ServiceLocator.Instance.GetService<IRentalHealthService>()?.KioskFunctionCheck();
		BrowseViewController.CacheProductImages();
		CheckReaderValidation();
	}

	private void CheckReaderValidation()
	{
		IDeviceServiceClientHelper deviceServiceClientHelper = ServiceLocator.Instance.GetService<IDeviceServiceClientHelper>();
		deviceServiceClientHelper?.CheckDeviceActivation();
		deviceServiceClientHelper?.CheckDeviceStatus();
	}

	public void StartTheRentalApplication()
	{
		IApplicationState applicationState = ServiceLocator.Instance.GetService<IApplicationState>();
		if (applicationState == null || !applicationState.IsInMaintenanceMode)
		{
			OrchestrationService.SendHealthServicesMinutesInMaintenanceModeStatistics(0);
		}
		CheckForTamperedCardReader();
	}

	public void CloseOpenSessionAndStartNewRentalSession(string newSessionContext, bool clearAudioDeviceConnected = true)
	{
		IRentalShoppingCart rentalShoppingCart = ServiceLocator.Instance.GetService<IRentalShoppingCartService>()?.CurrentRentalShoppingCart;
		if (rentalShoppingCart != null)
		{
			OrchestrationService.CloseShoppingCartAnalytics(rentalShoppingCart.ShoppingSessionId, isSessionAbandoned: true);
			rentalShoppingCart.Abandon("User chose to abandon session.");
			ApplicationState.Instance.IsADAAccessible = false;
			ITextToSpeechService textToSpeechService = ServiceLocator.Instance.GetService<ITextToSpeechService>();
			if ((textToSpeechService?.TTSEnabled ?? false) & clearAudioDeviceConnected)
			{
				textToSpeechService.ClearAudioDeviceConnected();
			}
		}
		ICardReadAttemptCollection cardReadAttemptCollection = Redbox.Rental.Services.Session.Session.Instance?.CardReadAttemptCollection ?? null;
		if (cardReadAttemptCollection != null && cardReadAttemptCollection.SupportsTechnicalFallbackTracking)
		{
			ServiceLocator.Instance.GetService<IEMVReaderHealth>()?.SessionWasInTechnicalFallback(cardReadAttemptCollection.IsInTrackableTechnicalFallback);
		}
		Redbox.Rental.Services.Session.Session.Instance?.ClearTrackData();
		ITrackDataService trackDataService = ServiceLocator.Instance.GetService<ITrackDataService>();
		trackDataService?.StopAllSessionCardReads(rentalShoppingCart?.ShoppingSessionId ?? default(Guid), CancelReason.EndingSession);
		if (trackDataService is IDeviceServiceTrackDataService)
		{
			ServiceLocator.Instance.GetService<IDeviceServiceClientHelper>()?.ReportAuthorizeResult(authorizeSuccessful: false);
		}
		ServiceLocator.Instance.GetService<IKioskClientService>()?.Session?.End();
		StartNewRentalSession(newSessionContext);
	}

	private void StartNewRentalSession(string context)
	{
		IViewService viewService = ViewService;
		if (viewService == null || !viewService.IsSafeToChangeView())
		{
			LogHelper.Instance.Log("Unable to start new session. view cannot be changed. Current view: " + ViewService?.Peek());
			ShowMaintenanceModeIfNeeded();
			return;
		}
		bool startSession = true;
		IEnvironmentNotificationService environmentNotificationService = ServiceLocator.Instance.GetService<IEnvironmentNotificationService>();
		if (environmentNotificationService != null && !environmentNotificationService.HasValidCardReader)
		{
			Redbox.Rental.Services.Session.Session.Instance = new Redbox.Rental.Services.Session.Session();
			if (CanEnterMaintenanceMode())
			{
				startSession = false;
				EnterMaintenanceModeBecauseOfDisconnectedCardReader();
			}
		}
		if (startSession)
		{
			LogHelper.Instance.Log("Starting new session. context: " + context);
			while ((ViewService?.ViewStackCount ?? 0) > 0)
			{
				ViewService.Pop();
			}
			OrchestrationService.StartTimer("reservation-expire-timer");
			OrchestrationService.StopIdleTimer("idle_timer");
			OrchestrationService.SetThemeAnimationState();
			Redbox.Rental.Services.Session.Session.Instance = new Redbox.Rental.Services.Session.Session();
			ServiceLocator.Instance.GetService<IKioskClientService>()?.Session?.Create(Redbox.Rental.Services.Session.Session.Instance.KioskSessionId);
			Redbox.Rental.Services.Configuration.Configuration.Instance.SessionReload();
			ServiceLocator.Instance.GetService<ITrackDataEncryptionService>()?.RefreshCertificate();
			OrchestrationService.ResetApplicationIdleTimerTimeout();
			ITextToSpeechService textToSpeechService = ServiceLocator.Instance.GetService<ITextToSpeechService>();
			if ((textToSpeechService != null && textToSpeechService.TTSEnabled) || ApplicationState.Instance.IsADAAccessible)
			{
				ApplicationState.Instance.IsADAAccessible = true;
			}
			if (ViewService != null)
			{
				ViewService.LastHitActorName = null;
			}
			IBrowseState browseState = Redbox.Rental.Services.Session.Session.Instance?.BrowseState;
			if (browseState != null)
			{
				browseState.BrowseMode = BrowseViewFilter.All;
				browseState.Genre = null;
				browseState.Rating = null;
				browseState.Sort = null;
				browseState.IsShoppingCartFullMessagesOpen = false;
				browseState.ShoppingCartFullMessagesShown = false;
				browseState.TitleFamily = TitleFamily.Movies;
			}
			OrchestrationService.InvokeStartNewRentalSessionCallback();
			ServiceLocator.Instance.GetService<IKioskClientService>()?.Campaign?.LoadCampaign();
			OrchestrationService.ViewShow(ViewService.GetStartOverViewName());
		}
	}

	private void InitializeDesktop()
	{
		IMacroService macroService = ServiceLocator.Instance.GetService<IMacroService>();
		string defaultCulture = ServiceLocator.Instance.GetService<IStoreManager>().GetDefaultCulture();
		if (!string.IsNullOrEmpty(defaultCulture))
		{
			macroService["DefaultCulture"] = defaultCulture;
		}
		ITextToSpeechService textToSpeechService = ServiceLocator.Instance.GetService<ITextToSpeechService>();
		if (textToSpeechService != null)
		{
			textToSpeechService.ClearOnAudioDeviceConnectionChanged();
			textToSpeechService.OnAudioDeviceConnectionChanged += HandleTextToSpeechAudioDeviceConnectionChanged;
			RegisterTTSWithRentalShoppingCartService();
		}
		IThemeService themeService = ServiceLocator.Instance.GetService<IThemeService>();
		if (themeService != null)
		{
			themeService.RegisterThemedControls("Redbox.Rental.UI.exe");
			themeService.RegisterThemedControls("Redbox.ControlPanel.UI.exe");
			themeService.RegisterThemedControls("Redbox.FMA.UI.dll");
			ITheme theme = themeService.Themes.Find((ITheme x) => x.Name == "Redbox Classic");
			if (theme != null)
			{
				theme.IsDefault = true;
				LogHelper.Instance.Log($"Set default theme to {theme.Name}", LogEntryType.Info);
				themeService.CurrentTheme = theme;
				LogHelper.Instance.Log($"Set current theme to {theme.Name}", LogEntryType.Info);
			}
			else
			{
				LogHelper.Instance.Log("Unable to set default theme to Redbox Classic", LogEntryType.Info);
			}
		}
		RegisterEnvironmentServiceCallbacks();
		RegisterApplicationStateCallbacks();
		RegisterDeviceServiceCanShutDownHandler();
		ServiceLocator.Instance.GetService<IKioskClientService>()?.Application?.Start();
	}

	private void CheckForTamperedCardReader()
	{
		IDeviceServiceClientHelper deviceServiceClientHelper = ServiceLocator.Instance.GetService<IDeviceServiceClientHelper>();
		if (deviceServiceClientHelper != null && deviceServiceClientHelper.IsCardReaderTampered)
		{
			ApplicationState.Instance.AddMaintenanceModeReason(MaintenanceModeSource.CardReader, "0099CDRR");
		}
	}

	private void RegisterDeviceServiceCanShutDownHandler()
	{
		ServiceLocator.Instance.GetService<IDeviceServiceClientHelper>()?.RegisterOnDeviceServiceCanShutDownHandler(delegate
		{
			bool canShutDown = true;
			bool hasUserInteraction = Redbox.Rental.Services.Session.Session.Instance?.HasReceivedUserInteraction ?? false;
			if (IsRentalApplicationRunning & hasUserInteraction)
			{
				canShutDown = false;
			}
			LogHelper.Instance.Log($"Device Service can shut down check: {canShutDown}   Session start trigger action: {Redbox.Rental.Services.Session.Session.Instance?.SessionStartTriggerAction}");
			return canShutDown;
		});
	}

	private void RegisterApplicationStateCallbacks()
	{
		ApplicationState.Instance.OnAddMaintenanceModeReasonHandler += HandleOnAddMaintenanceModeReason;
		ApplicationState.Instance.OnRemoveMaintenanceModeReasonHandler += HandleOnRemoveMaintenanceModeReason;
	}

	private void HandleOnRemoveMaintenanceModeReason(IMaintenanceModeReason removedMaintenanceModeReason, List<IMaintenanceModeReason> remainingMaintenanceModeReasons)
	{
		if (remainingMaintenanceModeReasons == null || remainingMaintenanceModeReasons.Count == 0)
		{
			if (!IsRentalApplicationRunning)
			{
				return;
			}
			IViewService viewService = ViewService;
			if (viewService != null && viewService.IsMaintenanceViewShowing())
			{
				IDeviceServiceClientHelper deviceServiceClientHelper = ServiceLocator.Instance.GetService<IDeviceServiceClientHelper>();
				if (deviceServiceClientHelper == null || !deviceServiceClientHelper.IsCardReaderTampered)
				{
					ViewService?.UnwindTo(ViewService.GetStartOverViewName(), discard: true, "clear maintenance view");
					ServiceLocator.Instance.GetService<IRedboxRental>()?.CloseOpenSessionAndStartNewRentalSession("Exit Maintenance Mode Code " + removedMaintenanceModeReason?.Code);
				}
			}
		}
		else
		{
			_ = ViewService?.IsMaintenanceViewShowing() ?? false;
		}
	}

	private void HandleOnAddMaintenanceModeReason(IMaintenanceModeReason maintenanceModeReason)
	{
		if (IsRentalApplicationRunning)
		{
			IViewService viewService = ViewService;
			if (viewService != null && !viewService.IsMaintenanceViewShowing())
			{
				if (ViewService?.UITaskScheduler != null)
				{
					Task.Factory.StartNew(delegate
					{
						OrchestrationService.ViewShow(ViewService.GetMaintenanceViewName(), maintenanceModeReason?.Code);
					}, CancellationToken.None, TaskCreationOptions.None, ViewService.UITaskScheduler);
				}
			}
			else
			{
				LogHelper.Instance.Log("Not showing maintenance view because it is already showing.");
			}
		}
		else
		{
			IResourceBundleService resourceBundleService = ServiceLocator.Instance.GetService<IResourceBundleService>();
			IManifestInfo manifestInfo = null;
			resourceBundleService?.GetManifest(out manifestInfo);
			LogHelper.Instance.Log("Not showing maintenance view because the rental application is not running.  Current application: " + manifestInfo?.ProductName);
		}
	}

	private void ShowMaintenanceModeIfNeeded()
	{
		List<IMaintenanceModeReason> maintenanceModeReasons = ApplicationState.Instance.GetMaintenanceModeReasons();
		IMaintenanceModeReason maintenanceModeReason = ((maintenanceModeReasons != null && maintenanceModeReasons.Any()) ? maintenanceModeReasons.Last() : null);
		if (maintenanceModeReason != null)
		{
			IViewService viewService = ServiceLocator.Instance.GetService<IViewService>();
			if (viewService != null && !viewService.IsMaintenanceViewShowing())
			{
				HandleOnAddMaintenanceModeReason(maintenanceModeReason);
			}
		}
	}

	private void RegisterEnvironmentServiceCallbacks()
	{
		IEnvironmentNotificationService environmentNotificationService = ServiceLocator.Instance.GetService<IEnvironmentNotificationService>();
		if (environmentNotificationService != null)
		{
			environmentNotificationService.RegisterCardReaderConnectedCallback(HandleCardReaderConnected);
			environmentNotificationService.RegisterCardReaderDisconnectedCallback(HandleCardReaderDisconnected);
			environmentNotificationService.RegisterCorruptDbCallback(HandleCorruptDb);
			environmentNotificationService.RegisterTamperedCardReaderCallback(HandleTamperedCardReader);
		}
	}

	private void HandleTamperedCardReader()
	{
		LogHelper.Instance.Log("Tampered Card Reader detected, processing.");
		ApplicationState.Instance.AddMaintenanceModeReason(MaintenanceModeSource.CardReader, "0099CDRR");
	}

	private void HandleCorruptDb()
	{
		LogHelper.Instance.Log("Corrupt Db detected, processing.");
		ApplicationState.Instance.AddMaintenanceModeReason(MaintenanceModeSource.Database, "SOFT-FILE01");
	}

	private void HandleCardReaderDisconnected(ITrackDataService previousTrackDataService)
	{
		string text = null;
		if (previousTrackDataService is IDeviceServiceTrackDataService)
		{
			text = "The Device Service card reader was disconnected.";
			ServiceLocator.Instance.GetService<IEMVReaderHealth>()?.TrackCardReaderDisconnects();
		}
		else if (previousTrackDataService is IClassicTrackDataService)
		{
			text = "The USB card reader was disconnected.";
		}
		if (CanEnterPleaseWaitViewBecauseOfDisconnectedCardReader())
		{
			CheckoutFunctions.ShowPleaseWaitBecauseOfDisconnectedCardReader();
		}
		else if (CanEnterMaintenanceMode())
		{
			LogHelper.Instance.Log(text + " Attempting to enter maintenance mode.");
			EnterMaintenanceModeBecauseOfDisconnectedCardReader();
		}
		else
		{
			LogHelper.Instance.Log(text + " Unable to enter maintenance mode.");
		}
	}

	public void EnterMaintenanceModeBecauseOfDisconnectedCardReader()
	{
		AddCardReaderMaintenanceModeReason("0072CDRR");
	}

	public void EnterMaintenanceModeBecauseOfTamperedCardReader()
	{
		IDeviceServiceClientHelper deviceServiceClientHelper = ServiceLocator.Instance.GetService<IDeviceServiceClientHelper>();
		if (deviceServiceClientHelper != null && !deviceServiceClientHelper.IsCardReaderTampered)
		{
			deviceServiceClientHelper.IsCardReaderTampered = true;
			deviceServiceClientHelper.SendTamperedCardReaderKioskAlertMessage();
		}
		AddCardReaderMaintenanceModeReason("0099CDRR");
	}

	public void HandleUnremovedChipInCardReader()
	{
		ServiceLocator.Instance.GetService<IRedboxRental>()?.CloseOpenSessionAndStartNewRentalSession("Chip not removed from Card Reader");
		string text = DateTime.Now.ToString("yyyyMMddHHmm");
		Store store = ServiceLocator.Instance.GetService<IDataService>()?.GetStore();
		ServiceLocator.Instance.GetService<IHardwareAlertManagerService>()?.SendAlert("HardwareError", $"KioskID: {store?.StoreId} Credit Card Reader Error &#150; Card Not Removed from Credit Card Reader", text + " - Credit Card Reader Card Not Removed.", null, null, "CardNotRemovedFromReader, 0100CDRR");
		LogHelper.Instance.LogTo("CardReaderLog", "Card Reader chip card not removed , Alert Sent");
	}

	private ITimer CreateDisconnectedCardReaderTimer()
	{
		IEnvironmentNotificationService environmentNotificationService = ServiceLocator.Instance.GetService<IEnvironmentNotificationService>();
		int value = ((environmentNotificationService != null && environmentNotificationService.PreviousTrackDataServiceWasDeviceServiceTrackDataService) ? 90000 : 10000);
		ITimerService timerService = ServiceLocator.Instance.GetService<ITimerService>();
		ITimer timer = timerService?.GetTimer("disconnected_usb_card_reader_timer");
		if (timer == null)
		{
			timer = timerService?.CreateTimer("disconnected_usb_card_reader_timer", value, value, delegate
			{
				(timerService?.GetTimer("disconnected_usb_card_reader_timer"))?.Stop();
				LogHelper.Instance.Log("> disconnected_usb_card_reader_timer executing");
				List<IMaintenanceModeReason> maintenanceModeReasons = ServiceLocator.Instance.GetService<IApplicationState>()?.GetMaintenanceModeReasons();
				if (maintenanceModeReasons == null || maintenanceModeReasons.Any((IMaintenanceModeReason x) => x.Source == MaintenanceModeSource.CardReader))
				{
					string text = DateTime.Now.ToString("yyyyMMddHHmm");
					Store store = ServiceLocator.Instance.GetService<IDataService>()?.GetStore();
					ServiceLocator.Instance.GetService<IHardwareAlertManagerService>()?.SendAlert("HardwareError", $"KioskID: {store?.StoreId} Credit Card Reader Communications Error &#150; Machine Down", text + " - Machine in maintenance mode.", null, null, "mm:USBCardReaderOffline, 0072CDRR");
					LogHelper.Instance.LogTo("CardReaderLog", "Card Reader Disconnected, Alert Sent");
				}
				else
				{
					LogHelper.Instance.LogTo("CardReaderLog", "Card Reader Disconnected, Replug Event Occured Within Threshold, Alert Not Sent");
				}
			});
		}
		return timer;
	}

	private void HandleCardReaderConnected()
	{
		if (ViewService?.UITaskScheduler == null)
		{
			return;
		}
		Task.Factory.StartNew(delegate
		{
			if (IsRentalApplicationRunning)
			{
				ISession instance = Redbox.Rental.Services.Session.Session.Instance;
				if (instance != null && instance.WaitingForCardReaderConnection)
				{
					LogHelper.Instance.Log("Done waiting for card reader connection.  Popping please_wait_view");
					CheckoutFunctions.PopPleaseWaitViewBecauseCardReaderReconnected();
					return;
				}
			}
			if (ServiceLocator.Instance?.GetService<IConfiguration>()?.ZeroTouchEnabled == true && OrchestrationService.PeekView() == "start_view")
			{
				LogHelper.Instance.Log("Done waiting for card reader connection.  Starting zero touch card read on start_view");
				StartViewController.OnCardReaderConnected();
			}
			else
			{
				ApplicationState.Instance.RemoveMaintenanceModeReason(MaintenanceModeSource.CardReader, "0072CDRR");
			}
		}, CancellationToken.None, TaskCreationOptions.None, ViewService.UITaskScheduler).Wait();
	}

	private bool CanEnterPleaseWaitViewBecauseOfDisconnectedCardReader()
	{
		bool result = false;
		if (IsRentalApplicationRunning && ViewService.Peek() == "swipe_view")
		{
			result = true;
		}
		return result;
	}

	private bool CanEnterMaintenanceMode()
	{
		bool canEnter = true;
		string text = "No reason not to enter maintenance mode";
		if (IsRentalApplicationRunning)
		{
			IEnvironmentNotificationService service = ServiceLocator.Instance.GetService<IEnvironmentNotificationService>();
			if (service != null && !service.IsTrackDataServiceRegistered)
			{
				bool onSwipeView = ViewService?.Peek() == "swipe_view";
				if (service.PreviousTrackDataServiceWasDeviceServiceTrackDataService)
				{
					IDeviceServiceClientHelper deviceServiceClientHelper = ServiceLocator.Instance.GetService<IDeviceServiceClientHelper>();
					if (deviceServiceClientHelper?.DeviceServiceShutDownInfo != null)
					{
						text = $"Device Service was shut down for reason {deviceServiceClientHelper?.DeviceServiceShutDownInfo?.ShutDownReason}";
						if (deviceServiceClientHelper?.DeviceServiceShutDownInfo?.ShutDownReason == ShutDownReason.SoftwareUpdate && !onSwipeView)
						{
							canEnter = false;
							text += " and current view is not swipe_view";
						}
					}
					else
					{
						text = "Device Service card reader not connected";
						if (!onSwipeView)
						{
							canEnter = false;
							text += " and current view is not swipe_view";
						}
					}
				}
				else if (service.PreviousTrackDataServiceWasClassicTrackDataService)
				{
					bool lastCardReadSucceeded = Redbox.Rental.Services.Session.Session.Instance?.LastCardReadAttemptSuccess ?? false;
					canEnter = !lastCardReadSucceeded;
					text = $"ClassicTrackDataService last card read attempt success = {lastCardReadSucceeded}";
				}
			}
		}
		else
		{
			canEnter = false;
			IManifestInfo manifestInfo = null;
			ServiceLocator.Instance.GetService<IResourceBundleService>()?.GetManifest(out manifestInfo);
			text = "Rental Application is not running.  Current application: " + manifestInfo?.ProductName;
		}
		LogHelper.Instance.Log($"CanEnterMaintenanceMode = {canEnter}   Reason: {text}");
		return canEnter;
	}

	private bool CanForceStartOfNewRentalSession()
	{
		bool result = true;
		IViewService viewService = ServiceLocator.Instance.GetService<IViewService>();
		if (viewService != null)
		{
			result = viewService.Peek() != viewService.GetMaintenanceViewName() && viewService.Peek() != "maintenance_sync_view" && !viewService.Stack.Any((IViewFrameInstance x) => x?.ViewFrame?.ViewName == "swipe_view") && Redbox.Rental.Services.Session.Session.Instance?.BrowseState?.InReturnFlow != true;
		}
		return result;
	}

	private void HandleTextToSpeechAudioDeviceConnectionChanged(ITextToSpeechService textToSpeachService, bool isConnected)
	{
		if (Redbox.Rental.Services.Session.Session.Instance != null && Redbox.Rental.Services.Session.Session.Instance.BrowseState != null)
		{
			ApplicationState.Instance.IsADAAccessible = isConnected;
		}
		if (isConnected)
		{
			ResourceBundleService.Instance.GetManifest(out var manifestInfo);
			_ = ResourceBundleService.Instance.Filter["view_name"];
			if (manifestInfo?.ProductName != "Rental Application")
			{
				LogHelper.Instance.Log($"TTS((> TextToSpeechService.AudioDeviceConnected Not in Rental Application. Currently {manifestInfo?.ProductName}");
			}
			else if (CanForceStartOfNewRentalSession())
			{
				LogHelper.Instance.Log("TTS((> Restarting session due to headphones being connected.");
				CloseOpenSessionAndStartNewRentalSession("TTS Audio Device connected", clearAudioDeviceConnected: false);
			}
		}
		else if (CanForceStartOfNewRentalSession())
		{
			CloseOpenSessionAndStartNewRentalSession("TTS Audio Device disconnected", clearAudioDeviceConnected: false);
		}
	}

	private void RegisterTTSWithRentalShoppingCartService()
	{
		ServiceLocator.Instance.GetService<IRentalShoppingCartService>().OnRentalShoppingCartAdded += delegate(IRentalShoppingCart rentalShoppingCart)
		{
			if (rentalShoppingCart != null)
			{
				rentalShoppingCart.OnChange += RespondToRentalShoppingCartChange;
				RespondToRentalShoppingCartChange(rentalShoppingCart);
			}
		};
	}

	private void RespondToRentalShoppingCartChange(IRentalShoppingCart rentalShoppingCart)
	{
		IDataService dataService = ServiceLocator.Instance.GetService(typeof(IDataService)) as IDataService;
		ITextToSpeechService textToSpeechService = ServiceLocator.Instance.GetService<ITextToSpeechService>();
		IMacroService macroService = ServiceLocator.Instance.GetService<IMacroService>();
		if (!textToSpeechService.TTSEnabled)
		{
			return;
		}
		IList<IRentalShoppingCartTitleItem> list = rentalShoppingCart.TitleItems;
		if (list == null)
		{
			list = new List<IRentalShoppingCartTitleItem>();
		}
		for (int i = 1; i <= rentalShoppingCart.MaximumCapacity; i++)
		{
			string text = null;
			IRentalShoppingCartTitleItem rentalShoppingCartTitleItem = ((i <= list.Count) ? list[i - 1] : null);
			if (rentalShoppingCartTitleItem != null)
			{
				dataService.GetProduct(rentalShoppingCartTitleItem.ProductId, excludeTitleRollup: true, out var kioskProduct);
				if (kioskProduct != null && kioskProduct is ITitleProduct)
				{
					text = ((ITitleProduct)kioskProduct).LongName;
				}
			}
			string macroName = $"TTS:CartTitle_{i}_Name";
			string macroValue = text;
			SetAndLogMacroValue(macroService, macroName, macroValue);
			macroName = $"TTS:CartTitle_{i}_Action";
			macroValue = ((rentalShoppingCartTitleItem != null) ? rentalShoppingCartTitleItem.Action.ToString() : string.Empty);
			SetAndLogMacroValue(macroService, macroName, macroValue);
		}
		macroService["TTS:NumberOfTitlesInCart"] = ((list.Count > 0) ? list.Count.ToString() : string.Empty);
		macroService["TTS:CartTitlePlurality"] = ((list.Count == 1) ? "Title" : "Titles");
		textToSpeechService.TTSRepeatSequence();
	}

	private void SetAndLogMacroValue(IMacroService macroService, string macroName, string macroValue)
	{
		LogHelper.Instance.Log(string.Format("{0} = {1}", macroName, string.IsNullOrEmpty(macroValue) ? "empty" : macroValue));
		macroService[macroName] = macroValue;
	}

	private void AddCardReaderMaintenanceModeReason(string reason)
	{
		ApplicationState.Instance.AddMaintenanceModeReason(MaintenanceModeSource.CardReader, reason);
		(ServiceLocator.Instance.GetService<IIdleTimerService>()?.GetIdleTimer("idle_timer"))?.Suspend();
		CreateDisconnectedCardReaderTimer()?.Start();
	}
}
