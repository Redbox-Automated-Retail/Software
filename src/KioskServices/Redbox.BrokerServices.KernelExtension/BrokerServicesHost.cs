using System;
using Redbox.BrokerServices.Kernel;
using Redbox.BrokerServices.Proxy;
using Redbox.BrokerServices.Proxy.ComponentModel;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;

namespace Redbox.BrokerServices.KernelExtension;

public class BrokerServicesHost : IKernelExtensionHost, IDisposable
{
	public IKernelExtension Extension { get; set; }

	public void Dispose()
	{
	}

	public bool CanSwitch(out ErrorList errors)
	{
		errors = new ErrorList();
		bool flag = ReservationServicesProxy.Instance.CanSwitch();
		LogHelper.Instance.Log("BrokerService CanSwitch: {0}", flag);
		if (!flag)
		{
			errors.Add(Redbox.KioskEngine.ComponentModel.Error.NewError("BSH666", "Bundle Switch Error", "Can not switch while reservation service is performing work."));
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
		ErrorList result = new ErrorList();
		LogHelper.Instance.Log("Broker Services Kernel Extension Load:");
		ServiceLocator.Instance.AddService(typeof(IReservationServices), ReservationServicesProxy.Instance);
		ServiceLocator.Instance.GetService<IPreferenceService>()?.AddPreferencePage("BrokerServices_General", "Remote Services\\Reservation Services", "Broker Reservation Services/General", PreferencePageTarget.LocalSystem, new GeneralPreferencePage());
		LogHelper.Instance.Log("...Loaded.");
		return result;
	}

	public ErrorList Reset()
	{
		ErrorList result = new ErrorList();
		LogHelper.Instance.Log("Broker Services Kernel Extension Reset:");
		IKernelFunctionRegistryService service = ServiceLocator.Instance.GetService<IKernelFunctionRegistryService>();
		if (service != null)
		{
			LogHelper.Instance.Log("...Register Broker Services kernel functions.");
			service.RegisterKernelFunctions(typeof(KernelRegistrationObject).Assembly, "Broker Services");
		}
		LogHelper.Instance.Log("...Reset Reservation Services Proxy.");
		ReservationServicesProxy.Instance.Reset();
		return result;
	}

	public ErrorList Activate()
	{
		ErrorList result = new ErrorList();
		LogHelper.Instance.Log("Broker Services Kernel Extension Activate:");
		LogHelper.Instance.Log("...Activated.");
		return result;
	}

	public ErrorList Deactivate()
	{
		ErrorList result = new ErrorList();
		LogHelper.Instance.Log("Broker Services Kernel Extension Deactivate:");
		LogHelper.Instance.Log("...Deactivated.");
		return result;
	}

	public void HandleHostCrash(Exception e)
	{
		LogHelper.Instance.Log("Broker Services Kernel Extension Host Crash.");
	}

	public void RegisterCommunicationActivity(Action<object, EventArgs> action)
	{
	}
}
