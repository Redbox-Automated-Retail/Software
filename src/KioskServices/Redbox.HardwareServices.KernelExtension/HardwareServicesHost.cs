using System;
using Redbox.Core;
using Redbox.HardwareServices.Kernel;
using Redbox.HardwareServices.Proxy;
using Redbox.HardwareServices.Proxy.ComponentModel;
using Redbox.KioskEngine.ComponentModel;

namespace Redbox.HardwareServices.KernelExtension;

public class HardwareServicesHost : IKernelExtensionHost, IDisposable
{
	private readonly IHardwareService _hardwareService;

	public IKernelExtension Extension { get; set; }

	public HardwareServicesHost()
	{
		_hardwareService = HardwareService.Instance;
	}

	public void Dispose()
	{
	}

	public bool CanSwitch(out ErrorList errors)
	{
		errors = new ErrorList();
		bool flag = _hardwareService.CanSwitch();
		LogHelper.Instance.Log("HardwareService CanSwitch: {0}", flag);
		if (!flag)
		{
			errors.Add(Redbox.KioskEngine.ComponentModel.Error.NewError("E246", "Bundle Switch Error", "Can not switch bundles while a return is running, try again after the return has finished."));
			return false;
		}
		return true;
	}

	public ErrorList PreLoad()
	{
		return new ErrorList();
	}

	public ErrorList Load()
	{
		LogHelper.Instance.Log("Hardware Services Kernel Extension Load:");
		ErrorList errorList = new ErrorList();
		errorList.AddRange(_hardwareService.Initialize());
		HardwareAlertManagerService.Instance.Initialize();
		IPreferenceService service = ServiceLocator.Instance.GetService<IPreferenceService>();
		if (service != null)
		{
			service.AddPreferencePage("HardwareServices_General", "Local Services\\Hardware Services", "Hardware Services/General", PreferencePageTarget.LocalSystem, new GeneralPreferencePage());
			service.AddPreferencePage("HardwareServices_CardReader", "Local Services\\Hardware Services", "Hardware Services/Credit Card Reader", PreferencePageTarget.LocalSystem, new CardReaderPage());
			service.AddPreferencePage("HardwareServices_DeviceServiceGeneral", "Local Services\\Hardware Services", "Device Service/General", PreferencePageTarget.LocalSystem, new DeviceServiceGeneralPage());
			service.AddPreferencePage("HardwareServices_DeviceService", "Local Services\\Hardware Services", "Device Service/Device Service", PreferencePageTarget.LocalSystem, new DeviceServicePage());
			service.AddPreferencePage("HardwareServices_DeviceServiceSimulator", "Local Services\\Hardware Services", "Device Service/Device Service Simulator", PreferencePageTarget.LocalSystem, new DeviceServiceSimulatorPage());
			if (_hardwareService.IsTest())
			{
				service.AddPreferencePage("HardwareServices_Return", "Local Services\\Hardware Services", "Hardware Services/Return", PreferencePageTarget.LocalSystem, new ReturnPreferencePage());
				service.AddPreferencePage("HardwareServices_Vend", "Local Services\\Hardware Services", "Hardware Services/Vend", PreferencePageTarget.LocalSystem, new VendPreferencePage());
				service.AddPreferencePage("HardwareServices_Statistics", "Local Services\\Hardware Services", "Hardware Services/Statistics", PreferencePageTarget.LocalSystem, new StatisticsPreferencePage());
				service.AddPreferencePage("HardwareServices_State", "Local Services\\Hardware Services", "Hardware Services/Hardware State", PreferencePageTarget.LocalSystem, new HardwareStatePage());
			}
		}
		LogHelper.Instance.Log("...Loaded.");
		return errorList;
	}

	public ErrorList Reset()
	{
		LogHelper.Instance.Log("Hardware Services Kernel Extension Reset:");
		ErrorList result = new ErrorList();
		HardwareService.Instance.Reset();
		ServiceLocator.Instance.GetService<IKernelFunctionRegistryService>()?.RegisterKernelFunctions(typeof(HardwareFunctions).Assembly, "Hardware Services");
		return result;
	}

	public ErrorList Activate()
	{
		LogHelper.Instance.Log("Hardware Services Kernel Extension Activate:");
		return new ErrorList();
	}

	public ErrorList Deactivate()
	{
		LogHelper.Instance.Log("Hardware Services Kernel Extension Deactivate:");
		return new ErrorList();
	}

	public void HandleHostCrash(Exception e)
	{
		LogHelper.Instance.Log("Hardware Services Kernel Extension Host Crash.");
	}

	public void RegisterCommunicationActivity(Action<object, EventArgs> action)
	{
	}
}
