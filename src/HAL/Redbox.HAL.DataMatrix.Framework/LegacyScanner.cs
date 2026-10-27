using System;
using System.Xml;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.DataMatrix.Framework;

internal sealed class LegacyScanner : ScannerAdapter, IScannerDevice, IDisposable, IConfigurationObserver
{
	private ICameraPlugin Plugin;

	private CameraGeneration _currentCameraGeneration;

	public bool RequiresExternalLighting => true;

	public bool IsConnected { get; private set; }

	public bool SupportsRestart => Plugin.SupportsReset;

	public bool InError { get; private set; }

	public bool IsLegacy => true;

	public bool SupportsSecureReads
	{
		get
		{
			IScannerDeviceService service = ServiceLocator.Instance.GetService<IScannerDeviceService>();
			bool flag = ServiceLocator.Instance.GetService<IBarcodeReaderFactory>().GetConfiguredReader().Service == BarcodeServices.Cortex;
			if (service.IRHardwareInstall.HasValue & flag)
			{
				if (CurrentCameraGeneration != CameraGeneration.Fifth)
				{
					return CurrentCameraGeneration == CameraGeneration.Fourth;
				}
				return true;
			}
			return false;
		}
	}

	public ScannerServices Service => ScannerServices.Legacy;

	public CameraGeneration CurrentCameraGeneration
	{
		get
		{
			if (_currentCameraGeneration == CameraGeneration.Unknown)
			{
				LogHelper.Instance.Log("[Legacy Camera Service] locate camera generation.");
				IDeviceDescriptor deviceDescriptor = ServiceLocator.Instance.GetService<IUsbDeviceService>().FindActiveCamera(matchDriver: false);
				if (deviceDescriptor != null)
				{
					LogHelper.Instance.Log("Found camera {0}", deviceDescriptor.Friendlyname);
					_currentCameraGeneration = From(deviceDescriptor);
				}
			}
			return _currentCameraGeneration;
		}
	}

	public bool Start()
	{
		return Start(test: false);
	}

	public bool Start(bool test)
	{
		if (IsConnected)
		{
			return true;
		}
		try
		{
			InError = false;
			IsConnected = Plugin.Start();
			if (!test)
			{
				return IsConnected;
			}
			using ISnapResult snapResult = Snap();
			return snapResult.SnapOk;
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("CameraService.Start() caught an exception.", e);
			return IsConnected = false;
		}
	}

	public ISnapResult Snap()
	{
		string text = GenerateSnapFileName();
		Plugin.Snap(text);
		SnapResult snapResult = new SnapResult(text);
		if (!snapResult.SnapOk)
		{
			InError = true;
		}
		return snapResult;
	}

	public bool ValidateSettings()
	{
		return ValidateSettings(null);
	}

	public bool ValidateSettings(IMessageSink s)
	{
		return false;
	}

	public bool TestConfiguration(IMessageSink sink)
	{
		return true;
	}

	public bool Stop()
	{
		if (!IsConnected)
		{
			return false;
		}
		IsConnected = false;
		try
		{
			return Plugin.Stop();
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("LegacyCameraService.Stop() caught an exception.", e);
			return false;
		}
	}

	public bool Restart()
	{
		if (!Plugin.SupportsReset)
		{
			return false;
		}
		Stop();
		ServiceLocator.Instance.GetService<IRuntimeService>().SpinWait(500);
		LogHelper.Instance.Log("[Legacy Camera Service] Cycle camera driver.");
		IUsbDeviceService service = ServiceLocator.Instance.GetService<IUsbDeviceService>();
		IDeviceDescriptor deviceDescriptor = service.FindActiveCamera(matchDriver: false);
		if (deviceDescriptor != null)
		{
			LogHelper.Instance.Log("Found camera {0}", deviceDescriptor.Friendlyname);
			CameraGeneration cameraGeneration = From(deviceDescriptor);
			if (CameraGeneration.Third == cameraGeneration || CameraGeneration.Fourth == cameraGeneration)
			{
				LogHelper.Instance.Log("The running driver for the camera {0}", deviceDescriptor.MatchDriver() ? "is correct" : "is not correct.");
			}
			DeviceStatus deviceStatus = service.FindDeviceStatus(deviceDescriptor);
			if ((deviceStatus & DeviceStatus.Found) != DeviceStatus.None && (deviceStatus & DeviceStatus.Enabled) != DeviceStatus.None)
			{
				bool flag = deviceDescriptor.ResetDriver();
				LogHelper.Instance.Log(" RESET camera returned {0}", flag ? "OK" : "FAILURE");
			}
		}
		return Start();
	}

	public IScanResult Scan()
	{
		ISnapResult snapResult = Snap();
		if (!snapResult.SnapOk)
		{
			return new ScanResult
			{
				DeviceError = true
			};
		}
		return ServiceLocator.Instance.GetService<IBarcodeReaderFactory>().GetConfiguredReader().Scan(snapResult);
	}

	public void NotifyConfigurationLoaded()
	{
		OnLoadPlugin();
	}

	public void NotifyConfigurationChangeStart()
	{
	}

	public void NotifyConfigurationChangeEnd()
	{
		OnLoadPlugin();
	}

	protected override void DisposeInner()
	{
		Plugin.Stop();
	}

	internal LegacyScanner()
	{
	}

	private void OnLoadPlugin()
	{
		if (BarcodeConfiguration.Instance.ScannerService == Service && Plugin == null)
		{
			LoadPlugin(BarcodeConfiguration.Instance.CameraPlugin);
		}
	}

	private void LoadPlugin(string driver)
	{
		CameraPluginDescriptor cameraPluginDescriptor = PluginLoader.Instance.LocatePlugin(driver, out var plugin);
		if (cameraPluginDescriptor == null)
		{
			LogHelper.Instance.Log($"Unable to locate plugin {driver}", LogEntryType.Error);
			Plugin = new NilCameraPlugin();
		}
		else
		{
			LogHelper.Instance.Log($"Successfully loaded camera plugin {driver}");
			Plugin = plugin;
			if (cameraPluginDescriptor.DescriptorPath != null)
			{
				XmlDocument xmlDocument = new XmlDocument();
				xmlDocument.Load(cameraPluginDescriptor.DescriptorPath);
				if (xmlDocument.DocumentElement != null)
				{
					XmlNode xmlNode = xmlDocument.SelectSingleNode("Plugin/Properties");
					if (xmlNode != null)
					{
						foreach (XmlNode childNode in xmlNode.ChildNodes)
						{
							if (!childNode.Name.Equals("property"))
							{
								continue;
							}
							string value = childNode.Attributes["name"].Value;
							string value2 = childNode.Attributes["value"].Value;
							XmlAttribute xmlAttribute = childNode.Attributes["type"];
							if (xmlAttribute == null)
							{
								BarcodeConfiguration.Instance.Properties[value] = value2;
								continue;
							}
							try
							{
								Type type = Type.GetType(xmlAttribute.Value);
								BarcodeConfiguration.Instance.Properties[value] = Convert.ChangeType(value2, type);
							}
							catch
							{
								BarcodeConfiguration.Instance.Properties[value] = value2;
							}
						}
					}
				}
			}
		}
		Plugin.InitWithProperties(BarcodeConfiguration.Instance.Properties);
	}

	private CameraGeneration From(IDeviceDescriptor descriptor)
	{
		if (descriptor == null)
		{
			return CameraGeneration.Unknown;
		}
		if (descriptor.Vendor.Equals("1871") && (descriptor.Product.Equals("0d01", StringComparison.CurrentCultureIgnoreCase) || descriptor.Product.Equals("0f01", StringComparison.CurrentCultureIgnoreCase)))
		{
			return CameraGeneration.Fourth;
		}
		if (descriptor.Vendor.Equals("0c45", StringComparison.CurrentCultureIgnoreCase) && descriptor.Product.Equals("627b", StringComparison.CurrentCultureIgnoreCase))
		{
			return CameraGeneration.Third;
		}
		return CameraGeneration.Unknown;
	}
}
