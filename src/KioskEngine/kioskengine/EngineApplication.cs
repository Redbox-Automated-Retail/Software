using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Timers;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Forms.Integration;
using System.Windows.Input;
using Microsoft.Win32;
using Redbox.BrokerServices.KernelExtension;
using Redbox.Core;
using Redbox.FraudServices.Proxy;
using Redbox.FraudServices.Proxy.ComponentModel;
using Redbox.GetOpts;
using Redbox.HardwareServices.KernelExtension;
using Redbox.HardwareServices.Proxy;
using Redbox.HardwareServices.Proxy.ComponentModel;
using Redbox.KioskDataServices.Proxy;
using Redbox.KioskDataServices.Proxy.ComponentModel;
using Redbox.KioskEngine.API;
using Redbox.KioskEngine.Bootstrap.Properties;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.ComponentModel.TrackData;
using Redbox.KioskEngine.Drawing;
using Redbox.KioskEngine.Environment;
using Redbox.KioskEngine.Environment.CardDataReader;
using Redbox.KioskEngine.IDE;
using Redbox.KioskEngine.Kernel;
using Redbox.REDS.Framework;
using Redbox.Rental.Data;
using Redbox.Rental.Kernel;
using Redbox.Rental.Model;
using Redbox.Rental.Model.Ads;
using Redbox.Rental.Model.Analytics;
using Redbox.Rental.Model.DataService;
using Redbox.Rental.Model.EngineApplication;
using Redbox.Rental.Model.Health;
using Redbox.Rental.Model.KioskClientService;
using Redbox.Rental.Model.KioskEvents;
using Redbox.Rental.Model.KioskHealth;
using Redbox.Rental.Model.Loyalty;
using Redbox.Rental.Model.Personalization;
using Redbox.Rental.Model.Pricing;
using Redbox.Rental.Model.Reservation;
using Redbox.Rental.Model.Session;
using Redbox.Rental.Model.UpdateClientService;
using Redbox.Rental.Services;
using Redbox.Rental.Services.Ads;
using Redbox.Rental.Services.Analytics;
using Redbox.Rental.Services.App;
using Redbox.Rental.Services.Authentication;
using Redbox.Rental.Services.Configuration;
using Redbox.Rental.Services.Health;
using Redbox.Rental.Services.KioskClientService;
using Redbox.Rental.Services.KioskEvents;
using Redbox.Rental.Services.KioskHealth;
using Redbox.Rental.Services.KioskProduct;
using Redbox.Rental.Services.Loyalty;
using Redbox.Rental.Services.Marketing;
using Redbox.Rental.Services.Personalization;
using Redbox.Rental.Services.Pricing;
using Redbox.Rental.Services.Reservations;
using Redbox.Rental.Services.Session;
using Redbox.Rental.Services.ShoppingCart;
using Redbox.Rental.Services.StoreServices;
using Redbox.Rental.Services.UpdateClientService;
using Redbox.UpdateServices.KernelExtension;
using WindowsInput;

namespace Redbox.KioskEngine.Bootstrap;

public class EngineApplication : ApplicationContext, IInputService, IFileCacheService, IEngineApplication
{
	public enum MapType : uint
	{
		MAPVK_VK_TO_VSC,
		MAPVK_VSC_TO_VK,
		MAPVK_VK_TO_CHAR,
		MAPVK_VSC_TO_VK_EX
	}

	internal struct NativeMessage
	{
		public IntPtr hWnd;

		public uint msg;

		public IntPtr wParam;

		public IntPtr lParam;

		public uint time;

		public System.Drawing.Point p;
	}

	private ElementHost? _elementHost;

	public string? _dataPath;

	internal System.Timers.Timer? _waitTimer;

	internal object _lockObject = new object();

	private DateTime m_idleEventLastLoggedDate = DateTime.Now;

	private long m_idleEventCounter;

	private TimeSpan m_idleLoggingTargetInterval = TimeSpan.FromSeconds(15.0);

	private bool checkMainFormStatus;

	private NamedLock? m_instanceLockWin7;

	private Mutex? m_instanceLockWin10;

	private CommandLine? m_commandLine;

	private readonly AutoResetEvent m_resetEvent = new AutoResetEvent(initialState: false);

	private static readonly Guid m_applicationGuidWin7 = new Guid("{FB5E48FF-4677-4cfa-8ED1-265114D7564C}");

	private static readonly Guid m_applicationGuidWin10 = new Guid("{2E14AC6E-F57C-4221-9BCA-D1FB6DD11FE0}");

	private readonly IDictionary<string, IdleEventHandler> m_idleHandlers = new Dictionary<string, IdleEventHandler>();

	private readonly IDictionary<string, KeyPressHandler> m_keyPressHandlers = new Dictionary<string, KeyPressHandler>();

	private readonly IDictionary<string, MouseClickHandler> m_mouseClickHandlers = new Dictionary<string, MouseClickHandler>();

	private readonly IDictionary<string, MouseClickHandler> m_mouseDoubleClickHandlers = new Dictionary<string, MouseClickHandler>();

	private readonly IDictionary<string, ActivatedEventHandler> m_activatedHandlers = new Dictionary<string, ActivatedEventHandler>();

	public static EngineApplication Instance => Singleton<EngineApplication>.Instance;

	public CommandLine CommandLine
	{
		get
		{
			if (m_commandLine == null)
			{
				m_commandLine = CommandLine.ParseTo(Options.Instance);
			}
			return m_commandLine;
		}
	}

	public string DataPath
	{
		get
		{
			if (!string.IsNullOrWhiteSpace(_dataPath))
			{
				return _dataPath!;
			}
			string text = string.Empty;
			if (Debugger.IsAttached)
			{
				text = Settings.Default.DevDataPath;
			}
			if (string.IsNullOrEmpty(text))
			{
				text = Path.Combine(RunningPath, "..\\data");
			}
			else if (!Path.IsPathRooted(text))
			{
				text = Path.Combine(RunningPath, text);
			}
			_dataPath = Path.GetFullPath(text);
			return _dataPath;
		}
	}

	public string RunningPath => Path.GetDirectoryName(typeof(EngineApplication).Assembly.Location)!;

	internal static bool IsApplicationIdle
	{
		get
		{
			NativeMessage msg;
			return !PeekMessage(out msg, IntPtr.Zero, 0u, 0u, 0u);
		}
	}

	public bool AppStarting { get; set; } = true;

	public void Reset()
	{
		LogHelper.Instance.Log("Reset input service.");
		ClearIdleHandlers();
		ClearActivatedHandlers();
		ClearKeyPressHandlers();
		ClearMouseClickHandlers();
		ClearMouseDoubleClickHandlers();
	}

	public void PerformSingleClick()
	{
		try
		{
			InputSimulator.SimulateLeftMouseDown(System.Windows.Forms.Cursor.Position.X, System.Windows.Forms.Cursor.Position.Y);
			InputSimulator.SimulateLeftMouseUp(System.Windows.Forms.Cursor.Position.X, System.Windows.Forms.Cursor.Position.Y);
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("An unhandled exception was raised in EngineApplication.PerformSingleClick.", e);
		}
	}

	public void PerformDoubleClick()
	{
		try
		{
			InputSimulator.SimulateLeftMouseDown(System.Windows.Forms.Cursor.Position.X, System.Windows.Forms.Cursor.Position.Y);
			InputSimulator.SimulateLeftMouseUp(System.Windows.Forms.Cursor.Position.X, System.Windows.Forms.Cursor.Position.Y);
			InputSimulator.SimulateLeftMouseDown(System.Windows.Forms.Cursor.Position.X, System.Windows.Forms.Cursor.Position.Y);
			InputSimulator.SimulateLeftMouseUp(System.Windows.Forms.Cursor.Position.X, System.Windows.Forms.Cursor.Position.Y);
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("An unhandled exception was raised in EngineApplication.PerformDoubleClick.", e);
		}
	}

	public void SimulateKeyStrokes(string input)
	{
		try
		{
			InputSimulator.SimulateTextEntry(input);
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("An unhandled exception was raised in EngineApplication.SimulateKeyStrokes.", e);
		}
	}

	public void RegisterIdleHandler(string name, IdleEventHandler handler)
	{
		if (!m_idleHandlers.ContainsKey(name))
		{
			m_idleHandlers[name] = handler;
		}
	}

	public void RemoveIdleHandler(string name)
	{
		m_idleHandlers.Remove(name);
	}

	public void ClearIdleHandlers()
	{
		m_idleHandlers.Clear();
	}

	public void RegisterActivatedHandler(string name, ActivatedEventHandler handler)
	{
		if (!m_activatedHandlers.ContainsKey(name))
		{
			m_activatedHandlers[name] = handler;
		}
	}

	public void RemoveActivatedHandler(string name)
	{
		m_activatedHandlers.Remove(name);
	}

	public void ClearActivatedHandlers()
	{
		m_activatedHandlers.Clear();
	}

	public void RegisterKeyPressHandler(string name, KeyPressHandler handler)
	{
		if (!m_keyPressHandlers.ContainsKey(name))
		{
			m_keyPressHandlers[name] = handler;
		}
	}

	public void RegisterMouseClickHandler(string name, MouseClickHandler handler)
	{
		if (!m_mouseClickHandlers.ContainsKey(name))
		{
			m_mouseClickHandlers[name] = handler;
		}
	}

	public void RemoveMouseClickHandler(string name)
	{
		m_mouseClickHandlers.Remove(name);
	}

	public void RegisterMouseDoubleClickHandler(string name, MouseClickHandler handler)
	{
		m_mouseDoubleClickHandlers.ContainsKey(name);
	}

	public void RemoveMouseDoubleClickHandler(string name)
	{
		m_mouseDoubleClickHandlers.Remove(name);
	}

	public void ClearKeyPressHandlers()
	{
		m_keyPressHandlers.Clear();
	}

	public void ClearMouseClickHandlers()
	{
		m_mouseClickHandlers.Clear();
	}

	public void ClearMouseDoubleClickHandlers()
	{
		m_mouseDoubleClickHandlers.Clear();
	}

	public void RemoveKeyPressHandler(string name)
	{
		m_keyPressHandlers.Remove(name);
	}

	public void Remove(string pattern)
	{
		try
		{
			string[] files = Directory.GetFiles(GetCachePath(), pattern);
			for (int i = 0; i < files.Length; i++)
			{
				File.Delete(files[i]);
			}
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("An unhandled exception was raised in IFileCacheService.Remove().", e);
		}
	}

	public void FlushCache()
	{
		LogHelper.Instance.Log("Flush file cache service.");
		try
		{
			string[] files = Directory.GetFiles(GetCachePath(), "*.*");
			foreach (string text in files)
			{
				LogHelper.Instance.Log("...Delete file '{0}'", text);
				File.Delete(text);
			}
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("An unhandled exception was raised in IFileCacheService.FlushCache().", e);
		}
	}

	public void SetFileContent(string fileName, byte[] data, bool overwrite)
	{
		string path = Path.Combine(GetCachePath(), fileName);
		if (!File.Exists(path) | overwrite)
		{
			File.WriteAllBytes(path, data);
		}
	}

	public void SetFileContent(string fileName, string data, bool overwrite)
	{
		string path = Path.Combine(GetCachePath(), fileName);
		if (!File.Exists(path) | overwrite)
		{
			File.WriteAllText(path, data);
		}
	}

	public string GetCachePath()
	{
		string text = Path.Combine(ServiceLocator.Instance.GetService<IMacroService>().ExpandProperties("${RunningPath}"), "..\\cache");
		if (!Directory.Exists(text))
		{
			Directory.CreateDirectory(text);
		}
		return text;
	}

	public byte[]? GetFileContent(string fileName)
	{
		string path = Path.Combine(GetCachePath(), fileName);
		if (File.Exists(path))
		{
			return File.ReadAllBytes(path);
		}
		return null;
	}

	public string? GetFileContentAsString(string fileName)
	{
		string path = Path.Combine(GetCachePath(), fileName);
		if (File.Exists(path))
		{
			return File.ReadAllText(path);
		}
		return null;
	}

	public void ShutDown()
	{
		System.Windows.Forms.Application.Exit();
	}

	public bool Run()
	{
		HostForm hostForm = new HostForm();
		base.MainForm = hostForm;
		base.MainForm.BackColor = RenderingService.Instance.BackgroundColor;
		BufferedGraphicsManager.Current.MaximumBuffer = base.MainForm.DisplayRectangle.Size;
		ServiceLocator.Instance.GetService<IEnvironmentNotificationService>().Register(base.MainForm.Handle);
		IRenderingService renderingService = ServiceLocator.Instance.GetService<IRenderingService>();
		ElementHost elementHost = new ElementHost
		{
			Parent = base.MainForm,
			Dock = DockStyle.Fill,
			Visible = false
		};
		elementHost.BackColor = Color.Red;
		_elementHost = elementHost;
		IScene scene = renderingService.CreateScene("Default", base.MainForm.ClientSize.Width, base.MainForm.ClientSize.Height, RenderingService.Instance.BackgroundColor, elementHost);
		scene.OnWPFHit += Scene_OnWPFHit;
		scene.OnWPFKeyDown += Scene_OnWPFKeyDown;
		scene.OnWPFKeyUp += Scene_OnWPFKeyUp;
		renderingService.ActiveScene = scene;
		Redbox.KioskEngine.ComponentModel.ErrorList errorList = new Redbox.KioskEngine.ComponentModel.ErrorList();
		try
		{
			if (System.Windows.Application.Current == null)
			{
				new System.Windows.Application();
			}
			Uri resourceLocator = new Uri("Redbox.Rental.UI;component/Assets/StyleDictionary.xaml", UriKind.Relative);
			System.Windows.Application.Current!.Resources.MergedDictionaries.Add(System.Windows.Application.LoadComponent(resourceLocator) as ResourceDictionary);
			Uri resourceLocator2 = new Uri("Redbox.ControlPanel.UI;component/Assets/StyleDictionary.xaml", UriKind.Relative);
			System.Windows.Application.Current.Resources.MergedDictionaries.Add(System.Windows.Application.LoadComponent(resourceLocator2) as ResourceDictionary);
			Uri resourceLocator3 = new Uri("Redbox.FMA.UI;component/Assets/StyleDictionary.xaml", UriKind.Relative);
			System.Windows.Application.Current.Resources.MergedDictionaries.Add(System.Windows.Application.LoadComponent(resourceLocator3) as ResourceDictionary);
		}
		catch (Exception e)
		{
			errorList.Add(Redbox.KioskEngine.ComponentModel.Error.NewError("E006", "Unable to load WPF resource dictionary.", e));
		}
		OrchestrationService.RegisterViewDefinitions();
		if (!ResourceBundleService.Instance.IsPendingBundleSwitch() && ResourceBundleService.Instance.DefaultBundleName == null)
		{
			errorList.AddRange(SelectBundle());
		}
		if (!errorList.ContainsError())
		{
			if (checkMainFormStatus)
			{
				return false;
			}
			base.MainForm.WindowState = FormWindowState.Minimized;
			base.MainForm.Show();
			base.MainForm.WindowState = FormWindowState.Normal;
			return true;
		}
		ErrorForm errorForm = new ErrorForm();
		errorForm.Errors = errorList;
		errorForm.ShowDialog();
		return false;
	}

	[DllImport("user32.dll")]
	public static extern bool GetKeyboardState(byte[] lpKeyState);

	[DllImport("user32.dll")]
	public static extern uint MapVirtualKey(uint uCode, MapType uMapType);

	[DllImport("user32.dll")]
	public static extern int ToUnicode(uint wVirtKey, uint wScanCode, byte[] lpKeyState, [Out][MarshalAs(UnmanagedType.LPWStr)] StringBuilder pwszBuff, int cchBuff, uint wFlags);

	[DllImport("user32.dll")]
	public static extern void DisableProcessWindowsGhosting();

	public static char? GetCharFromKey(Key key)
	{
		char? result = null;
		int virtualKey = KeyInterop.VirtualKeyFromKey(key);
		byte[] lpKeyState = new byte[256];
		GetKeyboardState(lpKeyState);
		uint wScanCode = MapVirtualKey((uint)virtualKey, MapType.MAPVK_VK_TO_VSC);
		StringBuilder stringBuilder = new StringBuilder(2);
		int charCount = ToUnicode((uint)virtualKey, wScanCode, lpKeyState, stringBuilder, stringBuilder.Capacity, 0u);
		// -1: a dead key, 0: no character.
		if (charCount != -1 && charCount != 0)
		{
			result = stringBuilder[0];
		}
		return result;
	}

	private void Scene_OnWPFKeyUp(object sender, System.Windows.Input.KeyEventArgs e)
	{
		char? charFromKey = GetCharFromKey(e.Key);
		if (charFromKey.HasValue)
		{
			KeyPressEventArgs e2 = new KeyPressEventArgs(charFromKey.Value);
			((HostForm)base.MainForm!).OnKeyPress(sender, e2);
		}
	}

	private void Scene_OnWPFKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
	{
		System.Windows.Forms.KeyEventArgs e2 = new System.Windows.Forms.KeyEventArgs((Keys)KeyInterop.VirtualKeyFromKey(e.Key));
		((HostForm)base.MainForm!).OnKeyDown(sender, e2);
	}

	private void Scene_OnWPFHit(IActor? actor)
	{
		ActivateMouseClickHandlers(new System.Windows.Forms.MouseEventArgs(MouseButtons.Left, 1, System.Windows.Forms.Cursor.Position.X, System.Windows.Forms.Cursor.Position.Y, 0));
		if (!base.MainForm!.Focused)
		{
			base.MainForm.Focus();
		}
	}

	public Redbox.KioskEngine.ComponentModel.ErrorList Initialize()
	{
		FirewallHelper.CreateFirewallExemptions();
		ConfigureDefaultMacros();
		IMacroService macroService = ServiceLocator.Instance.GetService<IMacroService>();
		LogHelper.Instance.Log("Engine start up.");
		LogHelper.Instance.Log(macroService.ExpandProperties("Engine: ${ProductName} ${ProductVersion}"));
		Redbox.KioskEngine.ComponentModel.ErrorList errorList = new Redbox.KioskEngine.ComponentModel.ErrorList();
		m_instanceLockWin7 = new NamedLock(m_applicationGuidWin7.ToString(), LockScope.Local);
		if (!m_instanceLockWin7.Exists())
		{
			return SingleInstanceError(macroService);
		}
		string name = string.Format(CultureInfo.InvariantCulture, "Global\\{0}", new object[1] { m_applicationGuidWin10 });
		new MutexSecurity().AddAccessRule(new MutexAccessRule(new SecurityIdentifier(WellKnownSidType.WorldSid, null), MutexRights.FullControl, AccessControlType.Allow));
		m_instanceLockWin10 = new Mutex(initiallyOwned: true, name, out var createdNew);
		if (!createdNew)
		{
			return SingleInstanceError(macroService);
		}
		if (CommandLine.Errors.Count > 0)
		{
			CommandLine.WriteUsage(includeHelp: false);
			while (CommandLine.Errors.Count > 0)
			{
				errorList.Add(Redbox.KioskEngine.ComponentModel.Error.NewError("E005", CommandLine.Errors.Pop(), string.Empty));
			}
			return errorList;
		}
		CommandHelper.Initialize();
		UserSettingsStore.Instance.RootPath = "SOFTWARE\\Redbox\\REDS\\Kiosk Engine";
		MachineSettingsStore.Instance.RootPath = "SOFTWARE\\Redbox\\REDS\\Kiosk Engine";
		AddService(typeof(IEngineApplication), this);
		errorList.AddRange(KernelExtensionService.Instance.Initialize(new List<Type>
		{
			typeof(BrokerServicesHost),
			typeof(HardwareServicesHost),
			typeof(UpdateServicesHost)
		}));
		errorList.AddRange(InitializeConfiguration());
		AddService(typeof(IObfuscationService), new ObfuscationHelper());
		AddService(typeof(IObjectCacheService), ObjectCacheService.Instance);
		AddService(typeof(IIdeService), IdeService.Instance);
		AddService(typeof(IInputService), this);
		AddService(typeof(IFileCacheService), this);
		AddService(typeof(IViewService), ViewService.Instance);
		AddService(typeof(ITimerService), TimerService.Instance);
		AddService(typeof(IIdleTimerService), IdleTimerService.Instance);
		AddService(typeof(ISoundService), SoundService.Instance);
		AddService(typeof(ITweenService), TweenService.Instance);
		AddService(typeof(ICallbackService), CallbackService.Instance);
		AddService(typeof(ISecurityService), SecurityService.Instance);
		AddService(typeof(IDataCacheService), DataCacheService.Instance);
		AddService(typeof(IUserSettingsStore), UserSettingsStore.Instance);
		AddService(typeof(IMachineSettingsStore), MachineSettingsStore.Instance);
		AddService(typeof(IPreferenceService), PreferenceService.Instance);
		AddService(typeof(IScreenSaverService), ScreenSaverService.Instance);
		AddService(typeof(IStyleSheetService), StyleSheetService.Instance);
		AddService(typeof(IKernelFunctionRegistryService), KernelFunctionRegistryService.Instance);
		AddService(typeof(IProductLookupCatalogService), new ProductLookupCatalogService());
		AddService(typeof(IThemeService), ThemeService.Instance);
		AddService(typeof(IFraudService), FraudService.Instance);
		AddService(typeof(IPromotionService), PromotionService.Instance);
		AddService(typeof(IConfiguration), Configuration.Instance);
		AddService(typeof(IDeviceServiceClientHelper), DeviceServiceClientHelperFactory.Create());
		AddService(typeof(IClassicTrackDataService), new ClassicTrackDataService());
		AddService(typeof(IDeviceServiceTrackDataService), new DeviceServiceTrackDataService());
		AddService(typeof(ITrackDataEncryptionService), new TrackDataEncryptionService());
		AddService(typeof(IAfterSwipeOfferService), AfterSwipeOfferService.Instance);
		AddService(typeof(IPerksSignupService), PerksSignupService.Instance);
		AddService(typeof(IReservationService), new ReservationService());
		AddService(typeof(IHttpService), new HttpService(LogHelper.Instance, new HttpClient()));
		AddService(typeof(IUpdateClientService), new UpdateClientService());
		AddService(typeof(IApiService), new APIService());
		AddService(typeof(IEngineApplicationService), new EngineApplicationService());
		IConfigurationService configurationService = ServiceLocator.Instance.GetService<IConfigurationService>();
		if (configurationService != null && (configurationService.TryGetValue<bool>("system", "General", "TextToSpeechEnabled", out var value) & value))
		{
			AddService(typeof(ITextToSpeechService), TextToSpeechService.Instance);
		}
		AnalyticsService instance = new AnalyticsService(LogHelper.Instance, configurationService, this);
		AddService(typeof(IAnalyticsService), instance);
		InitializePreferencePages();
		errorList.AddRange(SchedulerService.Instance.Initialize());
		errorList.AddRange(DataStoreService.Instance.Initialize());
		errorList.AddRange(FontCacheService.Instance.Initialize());
		errorList.AddRange(BitmapCacheService.Instance.Initialize());
		errorList.AddRange(RenderingService.Instance.Initialize());
		errorList.AddRange(KernelService.Instance.Initialize());
		errorList.AddRange(EnvironmentManager.Instance.Initialize());
#if NETFRAMEWORK
		errorList.AddRange(QueueService.Instance.Initialize(Path.Combine(RunningPath, "message.data"), new QueueServiceVistaDBData()));
#else
		// SQLite (message.sqlite), converted from message.data the first time.
		errorList.AddRange(QueueService.Instance.Initialize(Path.Combine(RunningPath, "message.data"), new QueueServiceSqliteData()));
#endif
		AddService(typeof(IKioskClientService), KioskClientService.Instance);
		ProductGroupService productGroupService = new ProductGroupService(LogHelper.Instance, KioskClientService.Instance);
		AddService(typeof(IProductGroupService), productGroupService);
		ServiceLocator.Instance.GetService<IConfiguration>();
		AddService(typeof(IKioskSessionJsonAnalyticsService), KioskSessionJsonAnalyticsService.Instance);
		AddService(typeof(IBarcodeTrackingService), BarcodeTrackingService.Instance);
		errorList.AddRange(ShoppingSessionService.Instance.Initialize(Path.Combine(RunningPath, "session.data")));
		errorList.AddRange(DebugService.Instance.Initialize());
		_ = ApplicationState.Instance.IsStandAlone;
		ServiceLocator.Instance.AddAuthenticationServices();
		ServiceLocator.Instance.AddStoreServices();
		AddService(typeof(IHealthServices), HealthService.Instance);
		ServiceLocator.Instance.AddMarketingServices();
		errorList.AddRange(KernelExtensionService.Instance.LoadAllExtensions());
		Redbox.KioskEngine.ComponentModel.ErrorList errorList2 = ResourceBundleService.Instance.Initialize(System.Windows.Forms.Application.ExecutablePath, Settings.Default.DevBundlePath);
		if (errorList2.ContainsError())
		{
			errorList.AddRange(errorList2);
		}
		AddService(typeof(ISyncUnknownProcessor), SyncUnknownProcessor.Instance);
		ServiceLocator.Instance.GetService<ISyncUnknownProcessor>()?.Initialize();
		AddService(typeof(ISyncEmptiesProcessor), SyncEmptiesProcessor.Instance);
		ServiceLocator.Instance.GetService<ISyncEmptiesProcessor>()?.Initialize();
		errorList.AddRange(InitializeRedboxRental());
		errorList2 = ResourceBundleService.Instance.LoadBundles();
		if (errorList2.ContainsError())
		{
			errorList.AddRange(errorList2);
		}
		string? text = Options.Instance.BundleFile;
		if (string.IsNullOrEmpty(text) && !string.IsNullOrEmpty(ResourceBundleService.Instance.DefaultBundleName))
		{
			text = ResourceBundleService.Instance.DefaultBundleName;
		}
		if (!string.IsNullOrEmpty(text))
		{
			try
			{
				BundleSpecifier bundleSpecifier = new BundleSpecifier(text);
				errorList.AddRange(ResourceBundleService.Instance.Activate(bundleSpecifier.Name, bundleSpecifier.Version.ToString()));
			}
			catch (Exception ex)
			{
				errorList.Add(Redbox.KioskEngine.ComponentModel.Error.NewError("B110", ex.Message, "Correct the specifier and try again."));
			}
		}
		else
		{
			LogHelper.Instance.Log("No Default Bundle specified in System.Config");
		}
		ServiceLocator.Instance.GetService<IHealthServices>()?.StartPingTimer();
		ServiceLocator.Instance.GetService<IApiService>()?.Initialize();
		return errorList;
	}

	private void AddService(Type serviceType, object instance)
	{
		LogHelper.Instance.Log("ServiceLocator adding service: " + serviceType?.Name);
		ServiceLocator.Instance.AddService(serviceType, instance);
	}

	public Redbox.KioskEngine.ComponentModel.ErrorList InitializeRedboxRental()
	{
		Redbox.KioskEngine.ComponentModel.ErrorList errorList = new Redbox.KioskEngine.ComponentModel.ErrorList();
		try
		{
			AddService(typeof(IRentalSessionService), new RentalSessionService());
			IKernelService kernelService = ServiceLocator.Instance.GetService<IKernelService>();
			IConfigurationService configurationService = ServiceLocator.Instance.GetService<IConfigurationService>();
			IKioskClientService kioskClientService = ServiceLocator.Instance.GetService<IKioskClientService>();
			AddService(instance: new ControlPanelService(), serviceType: typeof(IControlPanelService));
			AddService(instance: new FieldMaintenanceService(), serviceType: typeof(IFieldMaintenanceService));
			StoreManagerService storeManagerService = new StoreManagerService(Country.US, configurationService);
			AddService(typeof(IStoreManager), storeManagerService);
			kioskClientService?.Kiosk?.LoadNearbyKiosks();
			KioskEventService kioskEventService = new KioskEventService(storeManagerService);
			kioskEventService.Start();
			AddService(typeof(IKioskEventService), kioskEventService);
			AddService(instance: new RentalHealthService(), serviceType: typeof(IRentalHealthService));
			KernelHelperService kernelHelperService = new KernelHelperService();
			AddService(typeof(IKernelHelperService), kernelHelperService);
			InventoryData inventoryData = new InventoryData(kernelHelperService);
			inventoryData.Initialize();
			AddService(typeof(IInventoryData), inventoryData);
			AddService(instance: new InventoryService(inventoryData), serviceType: typeof(IInventoryService));
			RentalShoppingCartService.Instance.Initialize();
			RedboxRentalService redboxRentalService = new RedboxRentalService();
			AddService(typeof(IRedboxRental), redboxRentalService);
			errorList.AddRange(redboxRentalService.Initialize());
			DataService dataService = new DataService(kernelService, new DataServiceInstance(), new ProfileData(DataPath, "ProfileImages\\"), new CacheData(), new LocalData(), new ReservationData(DataPath), this);
			AddService(typeof(IDataService), dataService);
			dataService.Initialize();
			dataService.Start();
			dataService.RefreshIfNeeded();
			AddService(instance: new KioskClientServicePricing(), serviceType: typeof(IKioskClientServicePricing));
			PricingService pricingService = new PricingService(kernelService, storeManagerService);
			AddService(typeof(IPricingService), pricingService);
			pricingService.Start();
			ServiceLocator.Instance.GetService<IResourceBundleService>().RegisterAssemblyFunctions("Rental Kernel", typeof(RentalDataFunctions).Assembly);
			VistarAdService vistarAdService = new VistarAdService();
			AddService(typeof(IVistarAdService), vistarAdService);
			AdService instance = new AdService(vistarAdService);
			AddService(typeof(IAdService), instance);
			AddService(typeof(IPersonalizationService), new PersonalizationService());
			AddService(typeof(ILoyaltyService), new LoyaltyService());
			AddService(typeof(IPersistentDataCacheService), new PersistentDataCacheService());
			KioskHealthService kioskHealthService = new KioskHealthService(kioskEventService);
			AddService(typeof(IKioskHealthService), kioskHealthService);
			TouchScreenHealth touchScreenHealth = new TouchScreenHealth(kioskEventService);
			AddService(typeof(ITouchScreenHealth), touchScreenHealth);
			kioskHealthService.AddHealthItem(touchScreenHealth);
			RouterHealth routerHealth = new RouterHealth(kioskEventService);
			AddService(typeof(IRouterHealth), routerHealth);
			kioskHealthService.AddHealthItem(routerHealth);
			ArcusHealth arcusHealth = new ArcusHealth(kioskEventService);
			AddService(typeof(IArcusHealth), arcusHealth);
			kioskHealthService.AddHealthItem(arcusHealth);
			HardwareCorrectionStatsHealth healthItem = new HardwareCorrectionStatsHealth(kioskEventService);
			kioskHealthService.AddHealthItem(healthItem);
			ViewHealth viewHealth = new ViewHealth(kioskEventService);
			AddService(typeof(IViewHealth), viewHealth);
			kioskHealthService.AddHealthItem(viewHealth);
			CCReaderHealth cCReaderHealth = new CCReaderHealth(kioskEventService);
			AddService(typeof(ICCReaderHealth), cCReaderHealth);
			kioskHealthService.AddHealthItem(cCReaderHealth);
			EMVReaderHealth eMVReaderHealth = new EMVReaderHealth(kioskEventService);
			AddService(typeof(IEMVReaderHealth), eMVReaderHealth);
			kioskHealthService.AddHealthItem(eMVReaderHealth);
			AddService(typeof(IPingService), new PingService());
			kioskHealthService.Start();
		}
		catch (Exception e)
		{
			errorList.Add(Redbox.KioskEngine.ComponentModel.Error.NewError("E007", "Exception in EngineApplication.InitializeRedboxRental", e));
		}
		return errorList;
	}

	public void ThreadSafeHostUpdate(MethodInvoker invoker)
	{
		if (base.MainForm != null && base.MainForm.InvokeRequired)
		{
			base.MainForm.Invoke(invoker);
		}
		else
		{
			invoker();
		}
	}

	public bool CanRestart()
	{
		return EnvironmentFunctions.CanRestart();
	}

	public bool CanShutDown()
	{
		return EnvironmentFunctions.CanShutdown();
	}

	public void FlagForSafeShutdown()
	{
		EnvironmentFunctions.FlagForSafeShutdown();
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing)
		{
			if (m_instanceLockWin7 != null)
			{
				m_instanceLockWin7.Dispose();
			}
			if (m_instanceLockWin10 != null)
			{
				m_instanceLockWin10.Dispose();
			}
			CommandLine.Dispose();
		}
		base.Dispose(disposing);
	}

	protected override void OnMainFormClosed(object? sender, EventArgs e)
	{
		ITimerService timerService = ServiceLocator.Instance.GetService<ITimerService>();
		IIdleTimerService idleTimerService = ServiceLocator.Instance.GetService<IIdleTimerService>();
		ICallbackService callbackService = ServiceLocator.Instance.GetService<ICallbackService>();
		callbackService.Flush();
		IEnvironmentNotificationService notificationService = ServiceLocator.Instance.GetService<IEnvironmentNotificationService>();
		IQueueService queueService = ServiceLocator.Instance.GetService<IQueueService>();
		IKernelService kernelService = ServiceLocator.Instance.GetService<IKernelService>();
		IRenderingService renderingService = ServiceLocator.Instance.GetService<IRenderingService>();
		IFontCacheService fontCacheService = ServiceLocator.Instance.GetService<IFontCacheService>();
		IDataCacheService dataCacheService = ServiceLocator.Instance.GetService<IDataCacheService>();
		IBitmapCacheService bitmapCacheService = ServiceLocator.Instance.GetService<IBitmapCacheService>();
		IShoppingSessionService shoppingSessionService = ServiceLocator.Instance.GetService<IShoppingSessionService>();
		IDataStoreService dataStoreService = ServiceLocator.Instance.GetService<IDataStoreService>();
		ISchedulerService schedulerService = ServiceLocator.Instance.GetService<ISchedulerService>();
		ServiceLocator.Instance.GetService<IStyleSheetService>().Reset();
		idleTimerService?.Reset();
		timerService.Reset();
		schedulerService.Shutdown();
		notificationService.Unregister(base.MainForm!.Handle);
		notificationService.Reset();
		if (renderingService.ActiveScene != null)
		{
			renderingService.ActiveScene.Clear();
		}
		bitmapCacheService.DropCache();
		fontCacheService.DropCache();
		kernelService.Dispose();
		dataCacheService.Shutdown();
		callbackService.Reset();
		queueService.StopQueueWorker();
		shoppingSessionService.Dispose();
		queueService.Dispose();
		dataStoreService.CloseAllStores();
		ServiceLocator.Instance.GetService<IKioskHealthService>().Stop();
		ServiceLocator.Instance.GetService<IPricingService>().Stop();
		ServiceLocator.Instance.GetService<IKioskEventService>().Stop();
		ServiceLocator.Instance.GetService<IHealthServices>()?.StopPingTimer();
		ServiceLocator.Instance.GetService<IKernelExtensionService>()?.DeactivateAllExtensions();
		ServiceLocator.Instance.GetService<IConfigurationService>()?.Stop().ForEach(delegate(Redbox.KioskEngine.ComponentModel.Error error)
		{
			LogHelper.Instance.Log("Code: {0} Desc: {1} Details: {2}", error.Code, error.Description, error.Details);
		});
		ServiceLocator.Instance.GetService<IKioskClientService>()?.Application?.End();
		base.OnMainFormClosed(sender, e);
	}

	[DllImport("User32.dll")]
	internal static extern bool PeekMessage(out NativeMessage msg, IntPtr hWnd, uint messageFilterMin, uint messageFilterMax, uint flags);

	internal Redbox.KioskEngine.ComponentModel.ErrorList SelectBundle()
	{
		Redbox.KioskEngine.ComponentModel.ErrorList result = new Redbox.KioskEngine.ComponentModel.ErrorList();
		SelectBundleForm selectBundleForm = new SelectBundleForm
		{
			SearchPath = ResourceBundleService.Instance.SearchPath.Join(";")
		};
		selectBundleForm.ShowDialog();
		if (selectBundleForm.SelectedBundle != null)
		{
			if (ResourceBundleService.Instance.SetSwitchToBundle(selectBundleForm.SelectedBundle))
			{
				checkMainFormStatus = false;
			}
			ResourceBundleService.Instance.ShowDebugger = selectBundleForm.ShowDebugger;
			return result;
		}
		IMachineSettingsStore machineSettingsStore = ServiceLocator.Instance.GetService<IMachineSettingsStore>();
		if (machineSettingsStore != null && machineSettingsStore.GetValue<string>("Core", "DefaultBundle") != "NONE")
		{
			ResourceBundleService.Instance.ActivateDefaultBundle();
			checkMainFormStatus = false;
			return result;
		}
		checkMainFormStatus = true;
		m_keyPressHandlers.Clear();
		base.MainForm!.Close();
		return result;
	}

	internal void SwitchBundleIfNecessary()
	{
		IKernelExtensionService kernelExtensionService;
		if (ResourceBundleService.Instance.IsPendingBundleSwitch())
		{
			LogHelper.Instance.Log("Attempting bundle switch...");
			ServiceLocator.Instance.GetService<IPricingService>()?.ClearOnPricingSetChange();
			kernelExtensionService = ServiceLocator.Instance.GetService<IKernelExtensionService>();
			if (kernelExtensionService == null)
			{
				LogHelper.Instance.Log("Skippng bundle switch because kernelExtensionService is not initialized");
			}
			else
			{
				ThreadSafeHostUpdate(invoker);
			}
		}
		void invoker()
		{
			if (base.MainForm == null)
			{
				LogHelper.Instance.Log("Skippng bundle switch because MainForm is not initialized");
			}
			else
			{
				base.MainForm.TopMost = true;
				base.MainForm.TopMost = false;
				Redbox.KioskEngine.ComponentModel.ErrorList errorList = ResourceBundleService.Instance.Switch(out var previousBundle);
				if (!errorList.ContainsError())
				{
					kernelExtensionService.ActivateAllExtensions();
					ServiceLocator.Instance.GetService<IQueueService>()?.StartQueueWorker();
				}
				else
				{
					ErrorForm errorForm = new ErrorForm();
					errorForm.Errors = errorList;
					errorForm.ShowDialog();
					ResourceBundleService.Instance.SetSwitchToBundle(previousBundle);
				}
			}
		}
	}

	internal void ConfigureDefaultMacros()
	{
		MacroService macroService = new MacroService();
		AddService(typeof(IMacroService), macroService);
		macroService["EngineVersion"] = AssemblyInfoHelper.GetVersion(typeof(Program).Assembly);
		macroService["RunningPath"] = RunningPath;
		macroService["ProductName"] = "Redbox Kiosk Engine";
		macroService["ProductVersion"] = AssemblyInfoHelper.GetVersion(typeof(Program).Assembly);
	}

	internal void IdleLoop()
	{
		lock (_lockObject)
		{
			while (IsApplicationIdle && !_waitTimer!.Enabled)
			{
				using ExecutionTimer executionTimer = new ExecutionTimer();
				m_idleEventCounter++;
				TimeSpan timeSpan = DateTime.Now - m_idleEventLastLoggedDate;
				if (timeSpan > m_idleLoggingTargetInterval)
				{
					LogHelper.Instance.Log($"EngineApplication.IdleLoop: Target interval {m_idleLoggingTargetInterval}; Time since Last idle logged: {timeSpan};  Idle event count: {m_idleEventCounter}; invoke callbacks from IdleLoop: {!CallbackService.Instance.UseTimerToInvokeCallbacks}");
					CallbackService.Instance.LogStatistics();
					CallbackService.Instance.ResetStatistics();
					m_idleEventLastLoggedDate = DateTime.Now;
					m_idleEventCounter = 0L;
				}
				SwitchBundleIfNecessary();
				TimerService.Instance.UpdateTimers();
				TweenService.Instance.UpdateTweens();
				if (!CallbackService.Instance.UseTimerToInvokeCallbacks)
				{
					while (executionTimer.ElapsedMilliseconds < 10 && CallbackService.Instance.InvokeNextCallback())
					{
					}
				}
				RenderScene();
				_waitTimer.Start();
			}
		}
	}

	private void _waitTimer_Elapsed(object? sender, ElapsedEventArgs e)
	{
		_elementHost?.Child.Dispatcher.Invoke(delegate
		{
			IdleLoop();
		});
	}

	internal void OnIdle(object? sender, EventArgs e)
	{
		if (_waitTimer == null)
		{
			_waitTimer = new System.Timers.Timer(5.0);
			_waitTimer.Elapsed += _waitTimer_Elapsed;
			_waitTimer.AutoReset = false;
		}
		try
		{
			ActivateIdleHandlers();
		}
		catch (Exception e2)
		{
			LogHelper.Instance.Log("An unhandled exception was rasied in the Application.Idle handler.", e2);
		}
		IdleLoop();
	}

	internal bool RenderScene()
	{
		IRenderingService renderingService = ServiceLocator.Instance.GetService<IRenderingService>();
		if (renderingService == null)
		{
			return false;
		}
		if (renderingService.ActiveScene == null)
		{
			return false;
		}
		using (BufferedGraphics bufferedGraphics = BufferedGraphicsManager.Current.Allocate(base.MainForm!.CreateGraphics(), base.MainForm.DisplayRectangle))
		{
			renderingService.ActiveScene.Render(bufferedGraphics.Graphics, out var changed);
			if (!changed)
			{
				return false;
			}
			if (renderingService.ActiveScene.SceneRenderType == RenderType.WPF)
			{
				return true;
			}
			bufferedGraphics.Render();
		}
		return true;
	}

	internal void ActivateIdleHandlers()
	{
		if (m_idleHandlers.Count != 0)
		{
			// A copy: a handler may add or remove handlers.
			KeyValuePair<string, IdleEventHandler>[] snapshot = new KeyValuePair<string, IdleEventHandler>[m_idleHandlers.Count];
			m_idleHandlers.CopyTo(snapshot, 0);
			foreach (KeyValuePair<string, IdleEventHandler> keyValuePair in snapshot)
			{
				keyValuePair.Value();
			}
		}
	}

	internal void ActivateActivatedHandlers()
	{
		if (m_activatedHandlers.Count != 0)
		{
			// A copy: a handler may add or remove handlers.
			KeyValuePair<string, ActivatedEventHandler>[] snapshot = new KeyValuePair<string, ActivatedEventHandler>[m_activatedHandlers.Count];
			m_activatedHandlers.CopyTo(snapshot, 0);
			foreach (KeyValuePair<string, ActivatedEventHandler> keyValuePair in snapshot)
			{
				keyValuePair.Value();
			}
		}
	}

	internal void ActivateKeyPressHandlers(KeyPressEventArgsEx e)
	{
		if (m_keyPressHandlers.Count != 0)
		{
			string key = e.KeyChar.ToString();
			int keyChar = e.KeyChar;
			// A copy: a handler may add or remove handlers.
			KeyValuePair<string, KeyPressHandler>[] snapshot = new KeyValuePair<string, KeyPressHandler>[m_keyPressHandlers.Count];
			m_keyPressHandlers.CopyTo(snapshot, 0);
			foreach (KeyValuePair<string, KeyPressHandler> keyValuePair in snapshot)
			{
				keyValuePair.Value(key, keyChar, e.ModifierKeys.ToString());
			}
		}
	}

	internal void ActivateMouseClickHandlers(System.Windows.Forms.MouseEventArgs e)
	{
		if (m_mouseClickHandlers.Count != 0)
		{
			// A copy: a handler may add or remove handlers.
			KeyValuePair<string, MouseClickHandler>[] snapshot = new KeyValuePair<string, MouseClickHandler>[m_mouseClickHandlers.Count];
			m_mouseClickHandlers.CopyTo(snapshot, 0);
			foreach (KeyValuePair<string, MouseClickHandler> keyValuePair in snapshot)
			{
				keyValuePair.Value(e.Button.ToString(), e.X, e.Y);
			}
		}
	}

	internal void ActivateMouseDoubleClickHandlers(System.Windows.Forms.MouseEventArgs e)
	{
		if (m_mouseDoubleClickHandlers.Count != 0)
		{
			// A copy: a handler may add or remove handlers.
			KeyValuePair<string, MouseClickHandler>[] snapshot = new KeyValuePair<string, MouseClickHandler>[m_mouseDoubleClickHandlers.Count];
			m_mouseDoubleClickHandlers.CopyTo(snapshot, 0);
			foreach (KeyValuePair<string, MouseClickHandler> keyValuePair in snapshot)
			{
				keyValuePair.Value(e.Button.ToString(), e.X, e.Y);
			}
		}
	}

	private EngineApplication()
	{
		SystemEvents.SessionEnding += delegate
		{
			if (base.MainForm != null)
			{
				base.MainForm.Close();
			}
		};
		System.Windows.Forms.Application.ThreadException += delegate(object sender, ThreadExceptionEventArgs e)
		{
			try
			{
				ServiceLocator.Instance.GetService<IKernelExtensionService>()?.NotifyOfHostCrash(e.Exception);
				LogHelper.Instance.Log("Sending ApplicationCrash KioskAlert");
				ServiceLocator.Instance.GetService<IHealthServices>()?.SendAlert(ServiceLocator.Instance.GetService<IStoreServices>()?.StoreNumberString, "ApplicationCrash", null, e.Exception.ToString(), DateTime.Now, null);
			}
			catch (Exception e2)
			{
				LogHelper.Instance.Log("Unable to enqueue application crash alert.", e2);
			}
			try
			{
				ShoppingSessionService.Instance.AbandonAll("Application crash.");
			}
			catch (Exception e3)
			{
				LogHelper.Instance.Log("Unable to save the active shopping session.", e3);
			}
			LogHelper.Instance.Log("An unhandled exception was rasied in the Application.ThreadException handler.", e.Exception);
			System.Windows.Forms.MessageBox.Show(e.Exception.ToString());
		};
		System.Windows.Forms.Application.Idle += OnIdle;
	}

	private static void InitializePreferencePages()
	{
		IPreferenceService preferenceService = ServiceLocator.Instance.GetService<IPreferenceService>();
		if (preferenceService != null)
		{
			preferenceService.AddPreferencePage("EngineCore_General", "Store", "Engine Core/General", PreferencePageTarget.LocalSystem, new Redbox.KioskEngine.IDE.GeneralPreferencePage());
			preferenceService.AddPreferencePage("EngineCore_Ide", "Ide", "Engine Core/IDE", PreferencePageTarget.LocalSystem, new IdePreferencePage());
			preferenceService.AddPreferencePage("EngineCore_Resource", "Resource", "Engine Core/Resource", PreferencePageTarget.LocalSystem, new ResourcePreferencePage());
		}
	}

	private Redbox.KioskEngine.ComponentModel.ErrorList InitializeConfiguration()
	{
		Redbox.KioskEngine.ComponentModel.ErrorList errorList = new Redbox.KioskEngine.ComponentModel.ErrorList();
		ServiceLocator.Instance.GetService<IMacroService>();
		IConfigurationService service = ServiceLocator.Instance.GetService<IConfigurationService>();
		if (service != null)
		{
			LogHelper.Instance.Log("Starting Update Manager Configuration Service.");
			errorList.AddRange(service.Start());
			DirectoryInfo directoryInfo = new DirectoryInfo(DataPath);
			try
			{
				FileInfo[] files = directoryInfo.GetFiles("*.config");
				foreach (FileInfo file in files)
				{
					LogHelper.Instance.Log("> adding " + file.Name);
					string name = Path.GetFileNameWithoutExtension(file.Name);
					service.Add(name, file.FullName, "default", null, null, delegate
					{
						LogHelper.Instance.Log("A new {0} file was loaded.", Path.GetFileName(file.Name));
						if (name.Equals("kioskhealth", StringComparison.CurrentCultureIgnoreCase))
						{
							IKioskEventService kioskEventService = ServiceLocator.Instance.GetService<IKioskEventService>();
							if (kioskEventService == null)
							{
								LogHelper.Instance.Log("Event service was not found when trying to trigger a new health config file.");
							}
							else
							{
								kioskEventService.Restart();
							}
							IKioskHealthService kioskHealthService = ServiceLocator.Instance.GetService<IKioskHealthService>();
							if (kioskHealthService == null)
							{
								LogHelper.Instance.Log("Health service was not found when trying to trigger a new health config file.");
							}
							else
							{
								kioskHealthService.Restart();
							}
						}
					});
				}
			}
			catch (Exception e)
			{
				LogHelper.Instance.Log("'data' folder issue.", e);
				System.Environment.Exit(0);
			}
		}
		return errorList;
	}

	private static Redbox.KioskEngine.ComponentModel.ErrorList SingleInstanceError(IMacroService macroService)
	{
		return new Redbox.KioskEngine.ComponentModel.ErrorList { Redbox.KioskEngine.ComponentModel.Error.NewError("INSTANCE", macroService.ExpandProperties("Only one instance of ${ProductName} ${ProductVersion} may be run at a time."), macroService.ExpandProperties("Another instance of ${ProductName} ${ProductVersion} is already running.")) };
	}
}
