using System;
using System.Threading;
using Redbox.Core;
using Redbox.IPC.Framework;
using Redbox.KioskEngine.ComponentModel;
using Redbox.UpdateServices.Kernel;
using Redbox.UpdateServices.Proxy;

namespace Redbox.UpdateServices.KernelExtension;

public class UpdateServicesHost : IKernelExtensionHost, IDisposable
{
	private Thread _thread;

	private IPCServiceHost _listener;

	public IKernelExtension Extension { get; set; }

	public void Dispose()
	{
	}

	public bool CanSwitch(out Redbox.KioskEngine.ComponentModel.ErrorList errors)
	{
		errors = new Redbox.KioskEngine.ComponentModel.ErrorList();
		LogHelper.Instance.Log("UpdateService CanSwitch: True");
		return true;
	}

	public Redbox.KioskEngine.ComponentModel.ErrorList PreLoad()
	{
		Redbox.KioskEngine.ComponentModel.ErrorList errorList = new Redbox.KioskEngine.ComponentModel.ErrorList();
		try
		{
			LogHelper.Instance.Log("Update Services Kernel Extension PreLoad:");
			LogHelper.Instance.Log("...Loaded.");
			LogHelper.Instance.Log("Initialized Update Manager Configuration Service");
			errorList.AddRange(ConfigurationService.Instance.Initialize());
		}
		catch (Exception e)
		{
			errorList.Add(Redbox.KioskEngine.ComponentModel.Error.NewError("USH000", "Unhandled exception occured in UpdateServicesHost.PreLoad", e));
		}
		return errorList;
	}

	public Redbox.KioskEngine.ComponentModel.ErrorList Load()
	{
		Redbox.KioskEngine.ComponentModel.ErrorList errorList = new Redbox.KioskEngine.ComponentModel.ErrorList();
		try
		{
			LogHelper.Instance.Log("Update Services Kernel Extension Load:");
			errorList.AddRange(UpdateManagerServiceProxy.Instance.Initialize());
			IPreferenceService service = ServiceLocator.Instance.GetService<IPreferenceService>();
			if (service != null)
			{
				service.AddPreferencePage("UpdateServices_General", "Remote Services\\Store Services", "Update Services/General", PreferencePageTarget.LocalSystem, new GeneralPreferencePage());
				service.AddPreferencePage("UpdateServices_Config", "Local Services\\Update Services", "Update Services/Configuration", PreferencePageTarget.LocalSystem, new ConfigPreferencePage());
				_thread = new Thread(ExecuteListener);
				_thread.TrySetApartmentState(ApartmentState.MTA);
				_thread.Start();
			}
			LogHelper.Instance.Log("...Loaded.");
		}
		catch (Exception e)
		{
			errorList.Add(Redbox.KioskEngine.ComponentModel.Error.NewError("USH001", "Unhandled exception occured in UpdateServicesHost.Load", e));
		}
		return errorList;
	}

	public Redbox.KioskEngine.ComponentModel.ErrorList Reset()
	{
		Redbox.KioskEngine.ComponentModel.ErrorList result = new Redbox.KioskEngine.ComponentModel.ErrorList();
		LogHelper.Instance.Log("Update Services Kernel Extension Reset:");
		IKernelFunctionRegistryService service = ServiceLocator.Instance.GetService<IKernelFunctionRegistryService>();
		if (service != null)
		{
			service.RegisterKernelFunctions(typeof(UpdateManagerServiceFunctions).Assembly, "Update Services");
			service.RegisterKernelFunctions(typeof(ConfigurationServiceFunctions).Assembly, "Configuration Services");
		}
		LogHelper.Instance.Log("...Reset.");
		return result;
	}

	public Redbox.KioskEngine.ComponentModel.ErrorList Activate()
	{
		Redbox.KioskEngine.ComponentModel.ErrorList errorList = new Redbox.KioskEngine.ComponentModel.ErrorList();
		try
		{
			LogHelper.Instance.Log("Update Services Kernel Extension Activate:");
			LogHelper.Instance.Log("...Activated.");
		}
		catch (Exception e)
		{
			errorList.Add(Redbox.KioskEngine.ComponentModel.Error.NewError("USH002", "Unhandled exception occured in UpdateServicesHost.Activate", e));
		}
		return errorList;
	}

	public Redbox.KioskEngine.ComponentModel.ErrorList Deactivate()
	{
		Redbox.KioskEngine.ComponentModel.ErrorList errorList = new Redbox.KioskEngine.ComponentModel.ErrorList();
		try
		{
			LogHelper.Instance.Log("Update Services Kernel Extension Deactivate:");
			LogHelper.Instance.Log("...Deactivated.");
		}
		catch (Exception e)
		{
			errorList.Add(Redbox.KioskEngine.ComponentModel.Error.NewError("USH003", "Unhandled exception occured in UpdateServicesHost.Deactivate", e));
		}
		return errorList;
	}

	public void HandleHostCrash(Exception e)
	{
	}

	internal void ExecuteListener()
	{
		try
		{
			CommandRepository.AssemblyPatterns = new string[4] { "Redbox.DVD.UpdateServices.dll", "Redbox.UpdateServices.KernelExtension.dll", "kioskengine.exe", "Redbox.IPC.Framework.dll" };
			string value = ServiceLocator.Instance.GetService<IMachineSettingsStore>().GetValue("Local Services\\Update Services", "KioskEngineServiceUrl", "rcp://localhost:8004");
			_listener = IPCServiceHost.Create(IPCProtocol.Parse(value), AssemblyInfoHelper.GetProductName(typeof(EngineCommand).Assembly), AssemblyInfoHelper.GetVersion(typeof(EngineCommand).Assembly), AssemblyInfoHelper.GetCopyright(typeof(EngineCommand).Assembly), 3, 3, 8192, 64, 5);
			LogHelper.Instance.Log($"Start Service Listener for interface '{_listener.Protocol.Host}' on port '{_listener.Protocol.Port}'.", LogEntryType.Info);
			_listener.Start();
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("Top level exception caught", e);
		}
	}

	public void RegisterCommunicationActivity(Action<object, EventArgs> action)
	{
	}
}
