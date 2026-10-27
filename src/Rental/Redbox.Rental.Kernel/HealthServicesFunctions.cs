using System;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.ComponentModel.KioskServices;
using Redbox.Lua;
using Redbox.REDS.Framework;
using Redbox.Rental.Model;
using Redbox.Rental.Model.Health;

namespace Redbox.Rental.Kernel;

internal static class HealthServicesFunctions
{
	[KernelFunction(Name = "kernel.healthsvcs_sendalert")]
	internal static void SendAlert(string storeNumber, string alertType, string message)
	{
		InnerSendAlert(storeNumber, alertType, null, message, null);
	}

	[KernelFunction(Name = "kernel.healthsvcs_sendalert2")]
	internal static void SendAlert(string storeNumber, string alertType, string subType, string message)
	{
		InnerSendAlert(storeNumber, alertType, subType, message, null);
	}

	[KernelFunction(Name = "kernel.healthsvcs_sendauditevent")]
	internal static void SendAuditEvent(string storeNumber, string eventType, string userName, string source, string message)
	{
		InnerSendAuditEvent(storeNumber, eventType, userName, source, message, null);
	}

	[KernelFunction(Name = "kernel.healthsvcs_enqueuestatistic")]
	internal static void EnqueueStatistic(string name, string value)
	{
		ServiceLocator.Instance.GetService<IHealthServices>()?.EnqueueStatistic(name, value);
	}

	[KernelFunction(Name = "kernel.healthsvcs_enqueueproperty")]
	internal static void EnqueueProperty(string name, string value)
	{
		ServiceLocator.Instance.GetService<IHealthServices>()?.EnqueueProperty(name, value);
	}

	[KernelFunction(Name = "kernel.getsoftwareversions")]
	internal static LuaTable GetSoftwareVersions()
	{
		LuaTable luaTable = (LuaTable)ServiceLocator.Instance.GetService<IKernelService>().NewTable();
		IMacroService service = ServiceLocator.Instance.GetService<IMacroService>();
		if (service != null)
		{
			luaTable["Kiosk Engine"] = service["EngineVersion"];
			luaTable["Hal Service"] = service["HALServiceVersion"] ?? "Not Found";
			luaTable["Hal Client"] = service["HALVersion"] ?? "Not Found";
		}
		IKernelExtensionService service2 = ServiceLocator.Instance.GetService<IKernelExtensionService>();
		if (service2 != null)
		{
			foreach (IKernelExtension extension in service2.Extensions)
			{
				int num = extension.Name.LastIndexOf('.');
				string key = "[Extension] " + extension.Name.Substring(num + 1);
				luaTable[key] = extension.Version;
			}
		}
		IResourceBundleService service3 = ServiceLocator.Instance.GetService<IResourceBundleService>();
		if (service3 != null)
		{
			foreach (IBundleSpecifier bundle in service3.Bundles)
			{
				string key2 = "[" + bundle.Type.ToString() + " bundle] " + bundle.Name;
				luaTable[key2] = bundle.Version.ToString();
			}
		}
		IDeviceServiceClientHelper service4 = ServiceLocator.Instance.GetService<IDeviceServiceClientHelper>();
		if (service != null)
		{
			service["DeviceServiceVersion"] = service4?.DeviceServiceVersion?.ToString() ?? "Not Found";
			service["DeviceServiceClientVersion"] = service4?.DeviceServiceClientVersion?.ToString() ?? "Not Found";
		}
		return luaTable;
	}

	private static void InnerSendAlert(string storeNumber, string alertType, string subType, string message, LuaFunction callback)
	{
		IHealthServices service = ServiceLocator.Instance.GetService<IHealthServices>();
		if (service == null)
		{
			return;
		}
		ICallbackService callbackService = ServiceLocator.Instance.GetService<ICallbackService>();
		if (callbackService == null)
		{
			return;
		}
		RemoteServiceCallback callback2 = null;
		if (callback != null)
		{
			callback2 = delegate(IRemoteServiceResult r)
			{
				ICallbackService callbackService2 = callbackService;
				ResultCallbackEntry resultCallbackEntry = new ResultCallbackEntry
				{
					Function = callback
				};
				object[] parameters = new LuaTable[1] { r.AsTable() };
				resultCallbackEntry.Parameters = parameters;
				callbackService2.EnqueueCallback(resultCallbackEntry);
			};
		}
		service.SendAlert(storeNumber, alertType, subType, message, DateTime.Now, callback2);
	}

	private static void InnerSendAuditEvent(string storeNumber, string eventType, string userName, string source, string message, LuaFunction callback)
	{
		IHealthServices service = ServiceLocator.Instance.GetService<IHealthServices>();
		if (service == null)
		{
			return;
		}
		ICallbackService callbackService = ServiceLocator.Instance.GetService<ICallbackService>();
		if (callbackService == null)
		{
			return;
		}
		RemoteServiceCallback callback2 = null;
		if (callback != null)
		{
			callback2 = delegate(IRemoteServiceResult r)
			{
				ICallbackService callbackService2 = callbackService;
				ResultCallbackEntry resultCallbackEntry = new ResultCallbackEntry
				{
					Function = callback
				};
				object[] parameters = new LuaTable[1] { r.AsTable() };
				resultCallbackEntry.Parameters = parameters;
				callbackService2.EnqueueCallback(resultCallbackEntry);
			};
		}
		service.SendAuditEvent(storeNumber, eventType, DateTime.Now, userName, message, source, callback2);
	}
}
