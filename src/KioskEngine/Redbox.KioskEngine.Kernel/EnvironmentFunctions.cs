using System.Collections.ObjectModel;
using System.Threading;
using System.Windows.Forms;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Lua;
using Redbox.REDS.Framework;
using Redbox.Rental.Model;

namespace Redbox.KioskEngine.Kernel;

public static class EnvironmentFunctions
{
	private static bool m_safeShutdownFlagged;

	private static LuaFunction m_dataStoreErrorHandler;

	public static bool CanShutdown()
	{
		if (!CanSwitch())
		{
			return false;
		}
		IResourceBundleService service = ServiceLocator.Instance.GetService<IResourceBundleService>();
		IManifestInfo manifestInfo = null;
		service?.GetManifest(out manifestInfo);
		string message = "Kiosk Engine is checking if it can shut-down";
		if (manifestInfo?.ProductName == "Field Maintenance Application" || manifestInfo?.ProductName == "Control Panel Application")
		{
			LogHelper.Instance.Log(manifestInfo?.ProductName);
			LogHelper.Instance.Log(message);
			return false;
		}
		LogHelper.Instance.Log(message);
		IApplicationState service2 = ServiceLocator.Instance.GetService<IApplicationState>();
		IRedboxRental service3 = ServiceLocator.Instance.GetService<IRedboxRental>();
		if (service3 == null || service3.IsRentalSessionActive())
		{
			return service2?.IsInMaintenanceMode ?? false;
		}
		return true;
	}

	public static bool CanRestart()
	{
		if (!CanSwitch())
		{
			return false;
		}
		LogHelper.Instance.Log("Kiosk Engine is requesting a restart");
		IRedboxRental service = ServiceLocator.Instance.GetService<IRedboxRental>();
		if (service == null)
		{
			return false;
		}
		return !service.IsRentalSessionActive();
	}

	public static bool CanSwitch()
	{
		LogHelper.Instance.Log("Start CanSwitch Check");
		IResourceBundleService service = ServiceLocator.Instance.GetService<IResourceBundleService>();
		IKernelExtensionService service2 = ServiceLocator.Instance.GetService<IKernelExtensionService>();
		bool flag = ServiceLocator.Instance.GetService<IEngineApplication>()?.AppStarting ?? false;
		LogHelper.Instance.Log("Performing Startup: {0}", flag);
		bool flag2 = true;
		if (service != null && service.IsRentalRunning)
		{
			IRedboxRental service3 = ServiceLocator.Instance.GetService<IRedboxRental>();
			if (service3 != null && service3.IsRentalSessionActive())
			{
				LogHelper.Instance.Log("CanSwitch Check: Rental Session Active: true");
				flag2 = false;
			}
		}
		if (flag2)
		{
			flag2 = !flag && service2.CanSwitch(out var _);
		}
		LogHelper.Instance.Log("CanSwitch: {0}", flag2);
		return flag2;
	}

	public static bool HandleDataStoreErrors(string dataStore)
	{
		if (m_dataStoreErrorHandler == null)
		{
			return false;
		}
		ReadOnlyCollection<object> readOnlyCollection = m_dataStoreErrorHandler.Call(dataStore);
		if (readOnlyCollection == null || readOnlyCollection.Count <= 0)
		{
			return false;
		}
		return (bool)readOnlyCollection[0];
	}

	public static void FlagForSafeShutdown()
	{
		m_safeShutdownFlagged = true;
	}

	public static void Reset()
	{
		LogHelper.Instance.Log("Reset EnvironmentFunctions");
		if (m_dataStoreErrorHandler != null)
		{
			LogHelper.Instance.Log("...Reset data store errors handler.");
			m_dataStoreErrorHandler.Dispose();
			m_dataStoreErrorHandler = null;
		}
	}

	[KernelFunction(Name = "kernel.applysafeshutdown")]
	internal static void ApplySafeShutdown()
	{
		if (!m_safeShutdownFlagged)
		{
			LogHelper.Instance.Log("kernel.applysafeshutdown is not allowed now; returning.");
			return;
		}
		m_safeShutdownFlagged = false;
		LogHelper.Instance.Log("kernel.applysafeshutdown is allowed; calling Application.Exit().");
		Application.Exit();
	}

	[KernelFunction(Name = "kernel.applyunsafeshutdown")]
	internal static void ApplyUnSafeShutdown()
	{
		LogHelper.Instance.Log("kernel.applyunsafeshutdown; calling Application.Exit().");
		Application.Exit();
	}

	[KernelFunction(Name = "kernel.registerdatastoreerrorhandler")]
	internal static void RegisterDataStoreErrorHandler(LuaFunction handleDataStoreErrors)
	{
		m_dataStoreErrorHandler = handleDataStoreErrors;
	}

	[KernelFunction(Name = "kernel.hasvalidcardreader")]
	internal static bool HasValidCardReader()
	{
		IEnvironmentNotificationService service = ServiceLocator.Instance.GetService<IEnvironmentNotificationService>();
		if (service != null)
		{
			if (service.CheckCardReader)
			{
				return service.HasValidCardReader;
			}
			return true;
		}
		return false;
	}

	[KernelFunction(Name = "kernel.wait")]
	internal static void Wait(int delay)
	{
		LogHelper.Instance.Log($"Wait {delay}", LogEntryType.Info);
		new ManualResetEvent(initialState: false).WaitOne(delay);
	}

	[KernelFunction(Name = "kernel.handle_disconnected_usb_card_reader")]
	internal static void HandleDisconnectedCardReader()
	{
		ServiceLocator.Instance.GetService<IRedboxRental>()?.EnterMaintenanceModeBecauseOfDisconnectedCardReader();
	}
}
