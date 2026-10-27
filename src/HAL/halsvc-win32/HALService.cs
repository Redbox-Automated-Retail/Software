using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.ServiceProcess;
using System.Threading;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Component.Model.Extensions;
using Redbox.HAL.Component.Model.Services;
using Redbox.HAL.Configuration;
using Redbox.HAL.Controller.Framework;
using Redbox.HAL.Core;
using Redbox.HAL.DataMatrix.Framework;
using Redbox.HAL.DataStorage;
using Redbox.HAL.Script.Framework;
using Redbox.HAL.Service.Win32.Commands;
using Redbox.HAL.Service.Win32.Properties;
using log4net;

namespace Redbox.HAL.Service.Win32;

public class HALService : ServiceBase, ILogger
{
	private enum PowerState
	{
		Normal,
		Suspended
	}

	private const string StopMessage = "Stop Hardware Abstraction Layer (HAL) Service.";

	private const string StartMessage = "Start Hardware Abstraction Layer (HAL) Service";

	private Thread ListenerThread;

	private IPCProxy Proxy;

	private PowerState State;

	private static readonly ILog m_errorLog = LogManager.GetLogger("ErrorLog");

	private static readonly ILog m_log = LogManager.GetLogger("ServiceLog");

	private readonly object PowerStateLock = new object();

	private IContainer components;

	public HALService()
	{
		InitializeComponent();
	}

	public void Log(string message, Exception e)
	{
		m_log.Error(message, e);
	}

	public void Log(string msg)
	{
		Log(msg, LogEntryType.Info);
	}

	public void Log(string message, LogEntryType type)
	{
		switch (type)
		{
		case LogEntryType.Info:
			m_log.Info(message);
			break;
		case LogEntryType.Debug:
			m_log.Debug(message);
			break;
		case LogEntryType.Error:
			LogServiceError(message);
			m_log.Error(message);
			break;
		case LogEntryType.Fatal:
			m_log.Fatal(message);
			break;
		}
	}

	public void Log(string message, Exception e, LogEntryType type)
	{
		switch (type)
		{
		case LogEntryType.Info:
			m_log.Info(message, e);
			break;
		case LogEntryType.Debug:
			m_log.Debug(message, e);
			break;
		case LogEntryType.Error:
		{
			string msg = $"{message}: {e.Message} ({e.StackTrace})";
			LogServiceError(msg);
			m_log.Error(message, e);
			break;
		}
		case LogEntryType.Fatal:
			m_log.Fatal(message, e);
			break;
		}
	}

	public bool IsLevelEnabled(LogEntryType entryLogLevel)
	{
		return entryLogLevel switch
		{
			LogEntryType.Info => m_log.IsInfoEnabled, 
			LogEntryType.Debug => m_log.IsDebugEnabled, 
			LogEntryType.Error => m_log.IsErrorEnabled, 
			LogEntryType.Fatal => m_log.IsFatalEnabled, 
			_ => false, 
		};
	}

	protected override void OnStart(string[] args)
	{
		StartService();
	}

	protected override void OnStop()
	{
		Log("Stop service request from OnStop.");
		StopService();
	}

	protected override bool OnPowerEvent(PowerBroadcastStatus powerStatus)
	{
		LogHelper.Instance.Log("OnPowerEvent powerStatus = {0} State = {1}.", powerStatus.ToString(), State.ToString());
		IExecutionService service = ServiceLocator.Instance.GetService<IExecutionService>();
		IControllerService service2 = ServiceLocator.Instance.GetService<IControllerService>();
		IScannerDeviceService service3 = ServiceLocator.Instance.GetService<IScannerDeviceService>();
		lock (PowerStateLock)
		{
			if (PowerBroadcastStatus.Suspend == powerStatus)
			{
				if (State == PowerState.Normal)
				{
					State = PowerState.Suspended;
					service.Suspend();
					service2.Shutdown();
					service3.Shutdown();
				}
			}
			else if (PowerBroadcastStatus.ResumeSuspend == powerStatus && PowerState.Suspended == State)
			{
				service.Restart();
				State = PowerState.Normal;
			}
		}
		return base.OnPowerEvent(powerStatus);
	}

	protected override void OnShutdown()
	{
		Log("Stop service request from OnShutdown.");
		StopService();
		base.OnShutdown();
	}

	internal void StopService()
	{
		ServiceLocator.Instance.GetService<IExecutionService>().Shutdown();
		ServiceLocator.Instance.GetService<IControllerService>().Shutdown();
		ServiceLocator.Instance.GetService<IScannerDeviceService>().Shutdown();
		Log("Stop Hardware Abstraction Layer (HAL) Service.", LogEntryType.Info);
		WriteEventLogEntry("Stop Hardware Abstraction Layer (HAL) Service.", EventLogEntryType.Information);
		Proxy.Dispose();
	}

	internal void StartService()
	{
		ListenerThread = new Thread(ExecuteListener);
		ListenerThread.TrySetApartmentState(ApartmentState.MTA);
		ListenerThread.Start();
	}

	internal void ExecuteListener()
	{
		IRuntimeService runtimeService = new RuntimeService();
		string message = string.Format("{0} version {1} (.NET version {2}; OS = {3})", "Start Hardware Abstraction Layer (HAL) Service", typeof(HALService).Assembly.GetName().Version, Environment.Version.ToString(), runtimeService.GetPlatform().ToString());
		ErrorList errorList = new ErrorList();
		Dictionary<string, object> initProperties = new Dictionary<string, object>();
		try
		{
			Log(message, LogEntryType.Info);
			WriteEventLogEntry("Start Hardware Abstraction Layer (HAL) Service", EventLogEntryType.Information);
			ServiceLocator.Instance.AddService<ITableTypeFactory>(new TableTypeFactory());
			ServiceLocator.Instance.AddService<IFormattedLogFactoryService>(new FormattedLogFactory());
			ServiceLocator.Instance.AddService<ILogger>(this);
			ServiceLocator.Instance.AddService<IEncryptionService>(new TripleDesEncryptionService());
			ServiceLocator.Instance.AddService<IRuntimeService>(runtimeService);
			ServiceLocator.Instance.AddService<IPortManagerService>(new PortManager());
			ServiceLocator.Instance.AddService<IZipFileService>(new ZipFileService(runtimeService));
			ServiceLocator.Instance.AddService<IDeviceSetupClassFactory>(new DeviceSetupClassFactory());
			ServiceLocator.Instance.AddService<IUsbDeviceService>(new UsbDeviceService(Settings.Default.UsbServiceDebug));
			ServiceLocator.Instance.AddService<IDataTableService>(new DataTableService(Settings.Default.CountersDBExclusiveMode));
			ServiceLocator.Instance.AddService<IPersistentMapService>(new PersistentMapService());
			ConfigurationService configurationService = ConfigurationService.Make(runtimeService.RuntimePath("HAL.xml"));
			ServiceLocator.Instance.AddService<IConfigurationService>(configurationService);
			Redbox.HAL.Controller.Framework.KioskConfiguration.Instance.Initialize(errorList);
			ControllerService controllerService = new ControllerService();
			ServiceLocator.Instance.AddService<IControllerService>(controllerService);
			controllerService.Initialize(errorList, initProperties);
			LogHelper.Instance.Log("Loading Barcode Configuration Instance.");
			BarcodeConfiguration.NewInstance();
			LogHelper.Instance.Log("Loading BarcodeReaderFactory.");
			BarcodeReaderFactory barcodeReaderFactory = new BarcodeReaderFactory();
			ServiceLocator.Instance.AddService<IBarcodeReaderFactory>(barcodeReaderFactory);
			LogHelper.Instance.Log("Initializing barcode reader factory.");
			barcodeReaderFactory.Initialize(errorList);
			string scriptsPath = runtimeService.InstallPath("Scripts");
			LogHelper.Instance.Log("Loading ExecutionEngine.");
			IExecutionService executionService = ExecutionEngine.NewEngine(scriptsPath);
			ServiceLocator.Instance.AddService<IExecutionService>(executionService);
			executionService.Initialize(errorList);
			configurationService.Load(errorList);
			if (errorList.ContainsError())
			{
				errorList.ForEach(delegate(Error initError)
				{
					Log($"Error = {initError}", LogEntryType.Error);
				});
				Log("The HAL service encountered startup problems, and is stopping.", LogEntryType.Error);
				Log("Failed to load configuration.", LogEntryType.Error);
				WriteEventLogEntry("HAL Service encountered startup problems, and is stopping.", EventLogEntryType.Error);
				Stop();
				return;
			}
			IPCProxy.InitializeServices(Enum<IpcHostVersion>.Parse(Settings.Default.ServiceHostGeneration, IpcHostVersion.Legacy));
			Proxy = IPCProxy.Parse(Settings.Default.ServiceProtocol);
			if (!Proxy.ProtocolValid)
			{
				string text = "ProtocolURI is malformed; please fix. HAL service is exiting.";
				Log(text, LogEntryType.Fatal);
				WriteEventLogEntry(text, EventLogEntryType.Error);
			}
			else
			{
				executionService.Start();
				Proxy.StartListener();
			}
		}
		catch (Exception e)
		{
			Log("Top level exception caught", e);
		}
	}

	private void LogServiceError(string msg)
	{
		try
		{
			m_errorLog.Error(msg);
		}
		catch (Exception e)
		{
			Log("Unable to log error message to diagnostic log.", e);
		}
	}

	private bool WriteEventLogEntry(string msg, EventLogEntryType type)
	{
		try
		{
			EventLog.WriteEntry(msg, type);
			return true;
		}
		catch (Exception e)
		{
			Log("Unable to write to the event log.", e);
			return false;
		}
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing && components != null)
		{
			components.Dispose();
		}
		base.Dispose(disposing);
	}

	private void InitializeComponent()
	{
		base.CanHandlePowerEvent = true;
		base.CanShutdown = true;
		base.ServiceName = "HalSvc$Default";
	}
}
