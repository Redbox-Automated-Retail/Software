using System;
using System.Collections.Generic;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Controller.Framework;

namespace Redbox.HAL.DataMatrix.Framework;

public sealed class ScannerDeviceFactory : IScannerDeviceService, IConfigurationObserver
{
	private IScannerDevice m_configuredDevice;

	private ScannerServices m_configuredService;

	private readonly List<IConfigurationObserver> ConfigurableScanners = new List<IConfigurationObserver>();

	private readonly CortexService CortexDevice = new CortexService();

	private readonly LegacyScanner LegacyDevice = new LegacyScanner();

	private readonly NilScanner m_nilInstance = new NilScanner();

	public DateTime? IRHardwareInstall => Redbox.HAL.Controller.Framework.KioskConfiguration.Instance.IRHardwareInstallDate;

	public CameraGeneration CurrentCameraGeneration
	{
		get
		{
			if (m_configuredDevice != null)
			{
				return m_configuredDevice.CurrentCameraGeneration;
			}
			return CameraGeneration.Unknown;
		}
	}

	private ScannerServices ConfiguredService
	{
		get
		{
			return m_configuredService;
		}
		set
		{
			m_configuredService = value;
			switch (m_configuredService)
			{
			case ScannerServices.Legacy:
				m_configuredDevice = LegacyDevice;
				break;
			case ScannerServices.Cortex:
				m_configuredDevice = CortexDevice;
				break;
			default:
				m_configuredDevice = m_nilInstance;
				break;
			}
		}
	}

	public IScannerDevice GetConfiguredDevice()
	{
		return m_configuredDevice;
	}

	public void Shutdown()
	{
		LogHelper.Instance.Log("[ScannerFactory] Shutdown");
		if (m_configuredDevice != null)
		{
			m_configuredDevice.Stop();
		}
	}

	public void NotifyConfigurationLoaded()
	{
		LogHelper.Instance.Log("[ScannerFactory] Configuration load.");
		ConfiguredService = BarcodeConfiguration.Instance.ScannerService;
		ConfigurableScanners.ForEach(delegate(IConfigurationObserver each)
		{
			each.NotifyConfigurationLoaded();
		});
	}

	public void NotifyConfigurationChangeStart()
	{
		LogHelper.Instance.Log("[ScannerFactory] Configuration change start.");
		m_configuredDevice.Stop();
		m_configuredDevice = m_nilInstance;
		ConfigurableScanners.ForEach(delegate(IConfigurationObserver each)
		{
			each.NotifyConfigurationChangeStart();
		});
	}

	public void NotifyConfigurationChangeEnd()
	{
		LogHelper.Instance.Log("[ScannerFactory] Configuration change end.");
		ConfiguredService = BarcodeConfiguration.Instance.ScannerService;
		ConfigurableScanners.ForEach(delegate(IConfigurationObserver each)
		{
			each.NotifyConfigurationChangeEnd();
		});
		m_configuredDevice.Start();
	}

	public ScannerDeviceFactory(ScannerServices service)
	{
		ConfiguredService = service;
	}

	internal ScannerDeviceFactory()
	{
		m_configuredDevice = m_nilInstance;
		ConfigurableScanners.Add(CortexDevice);
		ConfigurableScanners.Add(LegacyDevice);
	}
}
