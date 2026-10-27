using System;
using System.IO;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.ComponentModel.KioskServices;
using Redbox.Lua;
using Redbox.Rental.Model;

namespace Redbox.Rental.Kernel;

internal static class StoreServicesFunctions
{
	[KernelFunction(Name = "kernel.storesvcs_getopsmarket")]
	internal static void GetOpsMarket(string storeNumber, LuaFunction completeCallback)
	{
		IStoreServices service = ServiceLocator.Instance.GetService<IStoreServices>();
		if (service == null)
		{
			return;
		}
		ICallbackService callbackService = ServiceLocator.Instance.GetService<ICallbackService>();
		if (callbackService != null)
		{
			service.GetOpsMarket(storeNumber, delegate(IRemoteServiceResult r)
			{
				ICallbackService callbackService2 = callbackService;
				ResultCallbackEntry resultCallbackEntry = new ResultCallbackEntry
				{
					Function = completeCallback
				};
				object[] parameters = new LuaTable[1] { r.AsTable() };
				resultCallbackEntry.Parameters = parameters;
				callbackService2.EnqueueCallback(resultCallbackEntry);
			});
		}
	}

	[KernelFunction(Name = "kernel.storesvcs_shouldreboot")]
	internal static bool ShouldReboot()
	{
		IStoreServices service = ServiceLocator.Instance.GetService<IStoreServices>();
		if (service == null)
		{
			return false;
		}
		DateTime? rebootTime = service.GetRebootTime();
		if (!rebootTime.HasValue)
		{
			return false;
		}
		DateTime now = DateTime.Now;
		DateTime dateTime = rebootTime.Value.AddMinutes(-1.0);
		DateTime dateTime2 = rebootTime.Value.AddMinutes(1.0);
		if (now >= dateTime)
		{
			return now <= dateTime2;
		}
		return false;
	}

	[KernelFunction(Name = "kernel.storesvcs_shouldruninitialsync")]
	internal static bool ShouldRunInitialSync()
	{
		return ServiceLocator.Instance.GetService<IStoreServices>()?.ShouldRunInitialSync() ?? false;
	}

	[KernelFunction(Name = "kernel.storesvcs_setruninitialsync")]
	internal static void SetRunInitialSync(bool flag)
	{
		ServiceLocator.Instance.GetService<IStoreServices>()?.SetRunInitialSync(flag);
	}

	[KernelFunction(Name = "kernel.storesvcs_registerinitialsyncconfigcallback")]
	internal static void RegisterInitialSyncChangeCallback(LuaFunction callback)
	{
		ServiceLocator.Instance.GetService<IEnvironmentNotificationService>()?.RegisterConfigurationCallback("iniital-sync-config-callback", delegate(string store, string action, string path, string key, string newValue)
		{
			if (callback != null)
			{
				IStoreServices service = ServiceLocator.Instance.GetService<IStoreServices>();
				if (service != null && store == "machine" && action == "set" && path == "Store" && (key == "RunInitialSync" || key == "InitialSyncType"))
				{
					callback.Call(service.ShouldRunInitialSync(), service.GetInitialSyncType());
				}
			}
		});
	}

	[KernelFunction(Name = "kernel.storesvcs_clearinitialsyncconfigcallback")]
	internal static void ClearInitialSyncChangeCallback()
	{
		ServiceLocator.Instance.GetService<IEnvironmentNotificationService>()?.UnregisterConfigurationCallback("iniital-sync-config-callback");
	}

	[KernelFunction(Name = "kernel.storesvcs_issinglebilling")]
	internal static bool IsSingleBilling(byte? collectionMethod)
	{
		return (collectionMethod.Value & 4) != 0;
	}

	[KernelFunction(Name = "kernel.storesvcs_getkaseyainipath")]
	internal static string GetKaseyainiPath()
	{
		return KaseyaMethods.GetKaseyaIniDPath();
	}

	[KernelFunction(Name = "kernel.storesvcs_getopsmarketandkaseyaserverfromfile")]
	internal static LuaTable GetOpsMarketAndKaseyaServerFromFile(string path)
	{
		try
		{
			string[] array = File.ReadAllLines(path);
			string text = Array.Find(array, (string p) => p.StartsWith("User_Name")).RemoveString("User_Name").TrimStart();
			string text2 = Array.Find(array, (string p) => p.StartsWith("Server_Name")).RemoveString("Server_Name").TrimStart();
			LogHelper.Instance.Log("KaseyaD.ini values: User_Name = {0}, Server_Name = {1}", text, text2);
			char[] separator = new char[1] { '.' };
			string text3 = text.Split(separator, 3, StringSplitOptions.None)[1];
			string text4 = text2.Split(separator, 3, StringSplitOptions.None)[0].Substring(2, 1).Insert(0, "Kaseya");
			LogHelper.Instance.Log("KaseyaD.ini parsed values: ops_market = {0}, kaseya_server = {1}", text3, text4);
			LuaTable obj = (LuaTable)ServiceLocator.Instance.GetService<IKernelService>().NewTable();
			obj["ops_market"] = text3;
			obj["kaseya_server"] = text4;
			return obj;
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("An unhandled exception was raised in StoreServicesFunctions.GetOpsMarketAndKaseyaServerFromFile.", e);
		}
		return null;
	}
}
