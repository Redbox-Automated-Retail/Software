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
		IStoreServices storeServices = ServiceLocator.Instance.GetService<IStoreServices>();
		if (storeServices == null)
		{
			return;
		}
		ICallbackService callbackService = ServiceLocator.Instance.GetService<ICallbackService>();
		if (callbackService != null)
		{
			storeServices.GetOpsMarket(storeNumber, delegate(IRemoteServiceResult r)
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
		IStoreServices storeServices = ServiceLocator.Instance.GetService<IStoreServices>();
		if (storeServices == null)
		{
			return false;
		}
		DateTime? rebootTime = storeServices.GetRebootTime();
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
				IStoreServices storeServices = ServiceLocator.Instance.GetService<IStoreServices>();
				if (storeServices != null && store == "machine" && action == "set" && path == "Store" && (key == "RunInitialSync" || key == "InitialSyncType"))
				{
					callback.Call(storeServices.ShouldRunInitialSync(), storeServices.GetInitialSyncType());
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
			string[] lines = File.ReadAllLines(path);
			string userName = Array.Find(lines, (string p) => p.StartsWith("User_Name")).RemoveString("User_Name").TrimStart();
			string serverName = Array.Find(lines, (string p) => p.StartsWith("Server_Name")).RemoveString("Server_Name").TrimStart();
			LogHelper.Instance.Log("KaseyaD.ini values: User_Name = {0}, Server_Name = {1}", userName, serverName);
			char[] separator = new char[1] { '.' };
			string opsMarket = userName.Split(separator, 3, StringSplitOptions.None)[1];
			string kaseyaServer = serverName.Split(separator, 3, StringSplitOptions.None)[0].Substring(2, 1).Insert(0, "Kaseya");
			LogHelper.Instance.Log("KaseyaD.ini parsed values: ops_market = {0}, kaseya_server = {1}", opsMarket, kaseyaServer);
			LuaTable table = (LuaTable)ServiceLocator.Instance.GetService<IKernelService>().NewTable();
			table["ops_market"] = opsMarket;
			table["kaseya_server"] = kaseyaServer;
			return table;
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("An unhandled exception was raised in StoreServicesFunctions.GetOpsMarketAndKaseyaServerFromFile.", e);
		}
		return null;
	}
}
