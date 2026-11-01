using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.Packaging;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Windows.Media.Imaging;
using System.Windows.Xps.Packaging;
using System.Xml;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.IDE;
using Redbox.REDS.Framework;
using Redbox.Rental.Model.Health;
using Redbox.Rental.Model.Reservation;

namespace Redbox.KioskEngine.Environment;

public class ResourceBundleService : IResourceBundleService
{
	private string _devBundlePath;

	private string m_applicationPath;

	private BundleCollection m_innerBundles;

	private Dictionary<string, Assembly> _asseblies = new Dictionary<string, Assembly>();

	private string[] _removedBundles = new string[7] { "content-banner-marketing.bundle", "content-incart-rundata.bundle", "content-pricing.bundle", "content-promo.bundle", "content-vend-screen.bundle", "content-marketing-rundata.bundle", "content-email-opt-in.bundle" };

	public static ResourceBundleService Instance => Singleton<ResourceBundleService>.Instance;

	public bool IsRentalRunning
	{
		get
		{
			if (ActiveBundle == null)
			{
				return false;
			}
			return ServiceLocator.Instance.GetService<IMacroService>()["ProductName"] == "Rental Application";
		}
	}

	public string DevBundleFullPath
	{
		get
		{
			string text = _devBundlePath;
			if (string.IsNullOrWhiteSpace(text))
			{
				return string.Empty;
			}
			if (!Path.IsPathRooted(text))
			{
				text = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(m_applicationPath), text));
			}
			return text;
		}
	}

	public IEnumerable<IBundleSpecifier> Bundles => InnerBundles.GetAllBundleVersions();

	public IResourceBundleFilter Filter { get; internal set; }

	public IResourceBundleSet ActiveBundleSet { get; internal set; }

	public IResourceBundle ActiveBundle { get; internal set; }

	public string SwitchStatusMessage { get; set; }

	public bool ShowDebugger { get; set; }

	public string[] SearchPath
	{
		get
		{
			IMachineSettingsStore machineSettingsStore = ServiceLocator.Instance.GetService<IMachineSettingsStore>();
			if (machineSettingsStore != null)
			{
				return machineSettingsStore.GetValue("Core", "BundleSearchPath", new string[1] { "..\\bundles" });
			}
			return new string[1] { "..\\bundles" };
		}
		set
		{
			ServiceLocator.Instance.GetService<IMachineSettingsStore>()?.SetValue("Core", "BundleSearchPath", value);
		}
	}

	public string Environment
	{
		get
		{
			return ServiceLocator.Instance.GetService<IMachineSettingsStore>()?.GetValue("Store", "Environment", "Production");
		}
		set
		{
			ServiceLocator.Instance.GetService<IMachineSettingsStore>()?.SetValue("Store", "Environment", value);
		}
	}

	public string CurrentBundleName { get; set; }

	public string DefaultBundleName
	{
		get
		{
			IConfigurationService configurationService = ServiceLocator.Instance.GetService<IConfigurationService>();
			if (configurationService != null && configurationService.TryGetObject("system", "Store", "DefaultBundle", out var value))
			{
				return value.ToString();
			}
			return "Rental Application,*";
		}
		set
		{
			ServiceLocator.Instance.GetService<IMachineSettingsStore>()?.SetValue("Core", "DefaultBundle", value);
		}
	}

	internal BundleCollection InnerBundles
	{
		get
		{
			if (m_innerBundles == null)
			{
				m_innerBundles = new BundleCollection();
			}
			return m_innerBundles;
		}
	}

	internal IResourceBundle SwitchToBundle { get; set; }

	public bool IsPendingBundleSwitch()
	{
		return SwitchToBundle != null;
	}

	public bool SetSwitchToBundle(IResourceBundle bundle)
	{
		if (bundle == ActiveBundle && MachineSettingsStore.Instance.GetValue("Store", "Environment", "MISSING") != "Integration")
		{
			return false;
		}
		SwitchToBundle = bundle;
		return true;
	}

	public Redbox.KioskEngine.ComponentModel.ErrorList Switch(out IResourceBundle previousBundle)
	{
		LogHelper.Instance.Log("Beginning ResourceBundleService.Switch");
		previousBundle = ActiveBundle;
		Redbox.KioskEngine.ComponentModel.ErrorList errorList = new Redbox.KioskEngine.ComponentModel.ErrorList();
		if (SwitchToBundle == null)
		{
			return errorList;
		}
		SwitchStatusForm switchStatusForm = new SwitchStatusForm();
		if (!string.IsNullOrEmpty(SwitchStatusMessage))
		{
			switchStatusForm.StatusLabel = SwitchStatusMessage;
		}
		switchStatusForm.Show();
		Application.DoEvents();
		ManualResetEvent manualResetEvent = new ManualResetEvent(initialState: false);
		IKernelExtensionService service = ServiceLocator.Instance.GetService<IKernelExtensionService>();
		try
		{
			LogHelper.Instance.Log("Checking kernelExtensionService.CanSwitch before bundle switch");
			DateTime now = DateTime.Now;
			Redbox.KioskEngine.ComponentModel.ErrorList errors;
			while (!service.CanSwitch(out errors))
			{
				LogHelper.Instance.Log("Unable to switch resource bundles due to switch error from kernel extension(s):", LogEntryType.Error);
				foreach (Redbox.KioskEngine.ComponentModel.Error item in errors)
				{
					LogHelper.Instance.Log("...{0}", item);
				}
				LogHelper.Instance.Log("Wait for 15 seconds before next check.");
				manualResetEvent.WaitOne(15000);
				if ((DateTime.Now - now).TotalMinutes >= 5.0)
				{
					return errorList;
				}
			}
			ServiceLocator.Instance.GetService<IHealthServices>()?.ResetMinutesInMaintenanceMode();
		}
		finally
		{
			manualResetEvent.Close();
		}
		try
		{
			using ExecutionTimer executionTimer = new ExecutionTimer();
			errorList.AddRange(Deactivate());
			new ManualResetEvent(initialState: false).WaitOne(5000);
			LogHelper.Instance.Log("Activate resource bundle: {0}", SwitchToBundle.Storage.BundlePath);
			ActiveBundle = SwitchToBundle;
			Filter = ActiveBundle.CreateFilter();
			Filter["runtime"] = "Client";
			Filter["platform"] = ".NET/Windows";
			Filter["environment"] = Environment;
			IConfigurationService configurationService = ServiceLocator.Instance.GetService<IConfigurationService>();
			List<string> list = new List<string>();
			configurationService.GetSettingNames("system", "Filters", out list);
			foreach (string filterName in list)
			{
				ConfigurationData value = new ConfigurationData();
				if (configurationService.TryGetConfigurationData("system", "Filters", filterName, out value) && value.Value.GetType() == typeof(string))
				{
					Filter[filterName] = value.Value;
					LogHelper.Instance.Log("Added filter: {0} = {1}", filterName, value.Value);
				}
			}
			SwitchToBundle = null;
			ActiveBundle.Activate(Filter, out var manifestInfo).CopyToLocalCollection(errorList);
			List<IResourceBundle> resourceBundles = new List<IResourceBundle>();
			List<string> startupScripts = new List<string> { manifestInfo.StartupScript };
			foreach (IBundleSpecifier require in manifestInfo.Requires)
			{
				InnerBundles.GetBundle(require.Name, require.Version.ToString(), out var bundle, out var manifestInfo2).CopyToLocalCollection(errorList);
				if (manifestInfo2 == null)
				{
					continue;
				}
				if (manifestInfo2.BundleType == BundleType.Application)
				{
					errorList.Add(Redbox.KioskEngine.ComponentModel.Error.NewError("B122", "Only Library and Content bundles may be referenced in the Requires property.", "Specify a bundle that has a type of Library or Content."));
					continue;
				}
				if (!string.IsNullOrEmpty(manifestInfo2.StartupScript) && !startupScripts.Contains(manifestInfo2.StartupScript))
				{
					startupScripts.Add(manifestInfo2.StartupScript);
				}
				resourceBundles.Add(bundle);
			}
			if (!errorList.ContainsError())
			{
				ActiveBundleSet = new ResourceBundleSet($"Bundle Set: {manifestInfo.ProductName}, {manifestInfo.ProductVersion}", ActiveBundle, resourceBundles);
				CurrentBundleName = manifestInfo.ProductName;
				IMacroService macroService = ServiceLocator.Instance.GetService<IMacroService>();
				macroService["ProductName"] = manifestInfo.ProductName;
				macroService["ProductVersion"] = manifestInfo.ProductVersion;
				errorList.AddRange(service.ResetAllExtensions());
				ServiceLocator.Instance.GetService<IReservationService>().UnRegisterFromAWSBrokerService(UnRegisterFromBrokerServiceReason.BundleSwitch);
				IKernelService kernelService = ServiceLocator.Instance.GetService<IKernelService>();
				foreach (string startupScript in startupScripts)
				{
					if (!kernelService.LoadStartupScript(startupScript))
					{
						errorList.Add(Redbox.KioskEngine.ComponentModel.Error.NewError("B033", $"Unable to load the startup script resource: '{startupScript}'", "Review the startup script resource for syntax errors."));
					}
				}
				if (!errorList.ContainsError())
				{
					IKernelFunctionRegistryService kernelFunctionRegistryService = ServiceLocator.Instance.GetService<IKernelFunctionRegistryService>();
					if (kernelFunctionRegistryService != null)
					{
						_asseblies.ForEach(delegate(KeyValuePair<string, Assembly> pair)
						{
							kernelFunctionRegistryService.RegisterKernelFunctions(pair.Value, pair.Key);
						});
					}
					kernelService.RegisterFunctions(kernelFunctionRegistryService.GetKernelFunctions());
					ServiceLocator.Instance.GetService<ICallbackService>().Resume();
					if (ShowDebugger)
					{
						ShowDebugger = false;
						IDebugService debugService = ServiceLocator.Instance.GetService<IDebugService>();
						if (debugService != null)
						{
							debugService.IsApplicationRunning = false;
							debugService.ActivateDebugger();
						}
					}
					else
					{
						string testControl = manifestInfo.TestControl;
						if (testControl != null && MachineSettingsStore.Instance.GetValue<string>("Store", "Environment") == "Integration")
						{
							if (!kernelService.IsTestPlanExecuting)
							{
								kernelService.ExecuteTestPlan(testControl);
							}
						}
						else
						{
							kernelService.ExecuteApplication();
						}
					}
				}
			}
			LogHelper.Instance.Log("Time to switch bundle: {0}", executionTimer.Elapsed);
			return errorList;
		}
		finally
		{
			ServiceLocator.Instance.GetService<IEngineApplication>().AppStarting = false;
			switchStatusForm.Close();
		}
	}

	public Redbox.KioskEngine.ComponentModel.ErrorList LoadBundles()
	{
		Redbox.KioskEngine.ComponentModel.ErrorList errorList = new Redbox.KioskEngine.ComponentModel.ErrorList();
		InnerBundles.Clear();
		List<string> adjustedPaths = new List<string>();
		if (Debugger.IsAttached)
		{
			string devBundleFullPath = DevBundleFullPath;
			if (!string.IsNullOrWhiteSpace(devBundleFullPath))
			{
				if (!Directory.Exists(devBundleFullPath))
				{
					LogHelper.Instance.Log("Application Property DevBundlePath {0} doesn't exist", devBundleFullPath);
				}
				else
				{
					string[] files = Directory.GetFiles(devBundleFullPath, "manifest.resource", SearchOption.AllDirectories);
					if (files.Length > 1)
					{
						files.ForEach(delegate(string manifestPath)
						{
							adjustedPaths.Add(Path.GetDirectoryName(manifestPath));
						});
					}
				}
			}
		}
		if (adjustedPaths.Count == 0)
		{
			string[] searchPath = SearchPath;
			foreach (string text in searchPath)
			{
				if (!Path.IsPathRooted(text))
				{
					adjustedPaths.Add(Path.Combine(Path.GetDirectoryName(m_applicationPath), text));
				}
				else
				{
					adjustedPaths.Add(text);
				}
			}
		}
		foreach (string eachPath in adjustedPaths)
		{
			LogHelper.Instance.Log("...Search path: {0}", eachPath);
			if (!Directory.Exists(eachPath))
			{
				errorList.Add(Redbox.KioskEngine.ComponentModel.Error.NewWarning("B001", $"Bundle directory does not exist: {eachPath}", "Correct the bundle search path and try again."));
				return errorList;
			}
			string[] removedBundlesFullPath = _removedBundles.Select((string removedBundle) => Path.Combine(eachPath, removedBundle)).ToArray();
			string[] bundleFiles = (from bundleFile in Directory.GetFiles(eachPath, "*.bundle")
				where !removedBundlesFullPath.Contains(bundleFile)
				select bundleFile).ToArray();
			if (bundleFiles.Length != 0)
			{
				string[] searchPath = bundleFiles;
				foreach (string bundlePath in searchPath)
				{
					ResourceBundle resourceBundle = new ResourceBundle(bundlePath, ResourceBundleStorageType.Zipped);
					Redbox.REDS.Framework.ErrorList errorList2 = resourceBundle.LoadResources();
					if (errorList2.ContainsError())
					{
						foreach (Redbox.REDS.Framework.Error item in errorList2)
						{
							errorList.Add(item.IsWarning ? Redbox.KioskEngine.ComponentModel.Error.NewWarning(item.Code, item.Description, item.Details) : Redbox.KioskEngine.ComponentModel.Error.NewError(item.Code, item.Description, item.Details));
						}
						continue;
					}
					InnerBundles.AddBundleVersion(resourceBundle).CopyToLocalCollection(errorList);
					if (!errorList.ContainsError())
					{
						LogHelper.Instance.Log("......Loaded bundle '{0}'.", bundlePath);
					}
				}
			}
			else if (Directory.GetFiles(eachPath, "manifest.resource").Length == 1)
			{
				ResourceBundle resourceBundle2 = new ResourceBundle(eachPath, ResourceBundleStorageType.FileSystem);
				resourceBundle2.LoadResources();
				InnerBundles.AddBundleVersion(resourceBundle2).CopyToLocalCollection(errorList);
				if (!errorList.ContainsError())
				{
					LogHelper.Instance.Log("......Loaded loose bundle '{0}'.", eachPath);
				}
			}
		}
		return errorList;
	}

	public Redbox.KioskEngine.ComponentModel.ErrorList Initialize(string applicationPath, string devBundlePath = null)
	{
		Redbox.KioskEngine.ComponentModel.ErrorList result = new Redbox.KioskEngine.ComponentModel.ErrorList();
		_devBundlePath = devBundlePath;
		LogHelper.Instance.Log("Initialize resource bundle service:");
		ServiceLocator.Instance.AddService(typeof(IResourceBundleService), Instance);
		m_applicationPath = applicationPath;
		return result;
	}

	public Redbox.KioskEngine.ComponentModel.ErrorList ActivateDefaultBundle()
	{
		BundleSpecifier bundleSpecifier = new BundleSpecifier(DefaultBundleName);
		return Activate(bundleSpecifier.Name, bundleSpecifier.Version.ToString());
	}

	public Redbox.KioskEngine.ComponentModel.ErrorList Activate(string productName, string productVersion)
	{
		Redbox.KioskEngine.ComponentModel.ErrorList errorList = new Redbox.KioskEngine.ComponentModel.ErrorList();
		InnerBundles.GetBundle(productName, productVersion, out var bundle, out var manifestInfo).CopyToLocalCollection(errorList);
		if (manifestInfo != null && manifestInfo.BundleType != BundleType.Application)
		{
			errorList.Add(Redbox.KioskEngine.ComponentModel.Error.NewError("B122", "Only Application bundles may be activated.", "Specify a bundle that has a type of Application."));
			return errorList;
		}
		if (!errorList.ContainsError())
		{
			SwitchToBundle = bundle;
		}
		return errorList;
	}

	public Redbox.KioskEngine.ComponentModel.ErrorList Deactivate()
	{
		Redbox.KioskEngine.ComponentModel.ErrorList errorList = new Redbox.KioskEngine.ComponentModel.ErrorList();
		if (ActiveBundle != null)
		{
			LogHelper.Instance.Log("Deactivate resource bundle: {0}", ActiveBundle.Storage.BundlePath);
		}
		ITweenService service = ServiceLocator.Instance.GetService<ITweenService>();
		if (service != null)
		{
			LogHelper.Instance.Log("Resetting tween service.");
			service.Reset();
		}
		ICallbackService callbackService = ServiceLocator.Instance.GetService<ICallbackService>();
		callbackService.Flush();
		callbackService.Suspend();
		Filter = null;
		IKernelExtensionService kernelExtensionService = ServiceLocator.Instance.GetService<IKernelExtensionService>();
		errorList.AddRange(kernelExtensionService.DeactivateAllExtensions());
		LogHelper.Instance.Log("Deactivate all kernel extensions", LogEntryType.Info);
		ServiceLocator.Instance.GetService<IMacroService>().Clear(new string[2] { "RunningPath", "EngineVersion" });
		ResetServices(errorList);
		ActiveBundle = null;
		ActiveBundleSet = null;
		return errorList;
	}

	public void Restart()
	{
		if (GetManifest(out var manifestInfo) != null)
		{
			Activate(manifestInfo.ProductName, manifestInfo.ProductVersion);
		}
	}

	public bool HasActiveBundle()
	{
		return ActiveBundle != null;
	}

	public IResourceBundle GetBundle(IBundleSpecifier bundleSpecifier)
	{
		InnerBundles.GetBundle(bundleSpecifier.Name, bundleSpecifier.Version.ToString(), out var bundle, out var _);
		return bundle;
	}

	public IResource GetResource(string resourceName)
	{
		if (ActiveBundleSet == null)
		{
			return null;
		}
		return ActiveBundleSet.GetResource(resourceName, Filter);
	}

	public ReadOnlyCollection<IResource> GetResources(string typeName)
	{
		if (ActiveBundleSet == null)
		{
			return new List<IResource>().AsReadOnly();
		}
		return ActiveBundleSet.GetResources(typeName, Filter);
	}

	public ReadOnlyCollection<IResource> GetResourcesUnfiltered(string typeName)
	{
		if (ActiveBundleSet == null)
		{
			return new List<IResource>().AsReadOnly();
		}
		return ActiveBundleSet.GetResourcesUnfiltered(typeName);
	}

	public IDictionary<string, IResourceType> GetResourceTypes()
	{
		if (ActiveBundleSet == null)
		{
			return new Dictionary<string, IResourceType>();
		}
		return ActiveBundleSet.GetResourceTypes();
	}

	public Font GetFont(string resourceName, out bool resourceFound)
	{
		resourceFound = false;
		IFontCacheService fontCacheService = ServiceLocator.Instance.GetService<IFontCacheService>();
		IResource resource = GetResource(resourceName);
		if (resource != null)
		{
			float size = 10f;
			FontStyle style = FontStyle.Regular;
			if (resource["point_size"] != null)
			{
				size = Convert.ToSingle(resource["point_size"]);
			}
			if (resource["font_style"] != null)
			{
				style = (FontStyle)resource["font_style"];
			}
			IAspect aspect = resource.GetAspect("content");
			if (aspect != null)
			{
				object content = aspect.GetContent();
				if (content != null)
				{
					resourceFound = true;
					return fontCacheService.RegisterFont(resource.GetFullName(), (string)resource["font_family"], size, style, (byte[])content);
				}
			}
			if (resource["font_family"] != null)
			{
				resourceFound = true;
				return fontCacheService.RegisterFont(resource.GetFullName(), (string)resource["font_family"], size, style);
			}
		}
		return fontCacheService.RegisterFont("default", "Arial", 12f, FontStyle.Bold);
	}

	public IResource GetManifest(out IManifestInfo manifestInfo)
	{
		manifestInfo = null;
		if (ActiveBundle == null)
		{
			return null;
		}
		return ActiveBundle.GetManifest(Filter, out manifestInfo);
	}

	public XmlNode GetXml(string resourceName)
	{
		IResource resource = GetResource(resourceName);
		if (resource == null)
		{
			return null;
		}
		return (XmlNode)resource.GetAspect("content").GetContent();
	}

	public byte[] GetSound(string resourceName)
	{
		IResource resource = GetResource(resourceName);
		if (resource == null)
		{
			return null;
		}
		return (byte[])resource.GetAspect("content").GetContent();
	}

	public XpsDocument GetXpsDocument(string resourceName)
	{
		XpsDocument result = null;
		IResource resource = GetResource(resourceName);
		if (resource != null)
		{
			IAspect aspect = resource.GetAspect("content");
			byte[] content = (byte[])resource.GetAspect("content").GetContent();
			if (content != null)
			{
				string text = string.Format("memorystream://{0}", aspect.Properties["file"]);
				Package package = PackageStore.GetPackage(new Uri(text));
				if (package == null)
				{
					package = Package.Open(new MemoryStream(content));
					PackageStore.AddPackage(new Uri(text), package);
				}
				result = new XpsDocument(package, CompressionOption.Fast, text);
			}
		}
		return result;
	}

	public string GetScript(string resourceName)
	{
		IResource resource = GetResource(resourceName);
		if (resource == null)
		{
			return null;
		}
		object content = resource.GetAspect("content").GetContent();
		if (content is byte[])
		{
			return Encoding.ASCII.GetString((byte[])content);
		}
		return (string)content;
	}

	public string GetJsonFile(string resourceName)
	{
		IResource resource = GetResource(resourceName);
		if (resource == null)
		{
			return null;
		}
		object content = resource.GetAspect("content").GetContent();
		if (content is byte[])
		{
			return Encoding.ASCII.GetString((byte[])content);
		}
		return (string)content;
	}

	public Image GetBitmap(string resourceName)
	{
		IBitmapCacheService bitmapCacheService = ServiceLocator.Instance.GetService<IBitmapCacheService>();
		IResource resource = GetResource(resourceName);
		if (resource == null)
		{
			return null;
		}
		return bitmapCacheService.RegisterImage(resource.GetFullName(), (byte[])resource.GetAspect("content").GetContent());
	}

	public BitmapImage GetBitmapImage(string resourceName)
	{
		IBitmapCacheService bitmapCacheService = ServiceLocator.Instance.GetService<IBitmapCacheService>();
		BitmapImage bitmapImage = bitmapCacheService.GetBitmapImage(resourceName);
		if (bitmapImage == null)
		{
			GetBitmap(resourceName);
			bitmapImage = bitmapCacheService.GetBitmapImage(resourceName);
		}
		return bitmapImage;
	}

	public ReadOnlyCollection<object> ExecuteScript(string resourceName)
	{
		return ServiceLocator.Instance.GetService<IKernelService>().ExecuteScript(resourceName);
	}

	private ResourceBundleService()
	{
	}

	private static void ResetServices(List<Redbox.KioskEngine.ComponentModel.Error> errors)
	{
		ICallbackService callbackService = ServiceLocator.Instance.GetService<ICallbackService>();
		IViewService viewService = ServiceLocator.Instance.GetService<IViewService>();
		ITimerService timerService = ServiceLocator.Instance.GetService<ITimerService>();
		IInputService inputService = ServiceLocator.Instance.GetService<IInputService>();
		ServiceLocator.Instance.GetService<ISoundService>();
		IKernelService kernelService = ServiceLocator.Instance.GetService<IKernelService>();
		ISchedulerService schedulerService = ServiceLocator.Instance.GetService<ISchedulerService>();
		IRenderingService renderingService = ServiceLocator.Instance.GetService<IRenderingService>();
		IDataStoreService dataStoreService = ServiceLocator.Instance.GetService<IDataStoreService>();
		IFontCacheService fontCacheService = ServiceLocator.Instance.GetService<IFontCacheService>();
		IStyleSheetService styleSheetService = ServiceLocator.Instance.GetService<IStyleSheetService>();
		IBitmapCacheService bitmapCacheService = ServiceLocator.Instance.GetService<IBitmapCacheService>();
		IShoppingSessionService shoppingSessionService = ServiceLocator.Instance.GetService<IShoppingSessionService>();
		IEnvironmentNotificationService environmentNotificationService = ServiceLocator.Instance.GetService<IEnvironmentNotificationService>();
		IKernelFunctionRegistryService kernelFunctionRegistryService = ServiceLocator.Instance.GetService<IKernelFunctionRegistryService>();
		schedulerService?.Reset();
		timerService?.Reset();
		if (renderingService.ActiveScene != null)
		{
			renderingService.ActiveScene.Clear();
		}
		viewService.Reset();
		styleSheetService?.Reset();
		bitmapCacheService.DropCache();
		fontCacheService.DropCache();
		kernelService.Reset();
		kernelFunctionRegistryService.Reset();
		inputService.Reset();
		environmentNotificationService.Reset();
		shoppingSessionService.Reset();
		callbackService.Reset();
		dataStoreService.CloseAllStores();
	}

	public void RegisterAssemblyFunctions(string name, Assembly assembly)
	{
		_asseblies[name] = assembly;
	}
}
