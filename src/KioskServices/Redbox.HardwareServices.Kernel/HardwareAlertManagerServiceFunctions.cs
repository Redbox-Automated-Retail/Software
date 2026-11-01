using Redbox.Core;
using Redbox.HardwareServices.Proxy.ComponentModel;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Lua;

namespace Redbox.HardwareServices.Kernel;

public class HardwareAlertManagerServiceFunctions
{
	[KernelFunction(Name = "kernel.hardware_alert_manager_send_alert_job")]
	public static void SendAlertJob(string alertType, string source, string description, string jobId, string subType)
	{
		ServiceLocator.Instance.GetService<IHardwareAlertManagerService>()?.SendJobAlert(alertType, source, description, jobId, subType);
	}

	[KernelFunction(Name = "kernel.hardware_alert_manager_send_alert")]
	public static void SendAlert(string alertType, string source, string description, string jobResults, string jobErrors, string subType)
	{
		ServiceLocator.Instance.GetService<IHardwareAlertManagerService>()?.SendAlert(alertType, source, description, jobResults, jobErrors, subType);
	}

	[KernelFunction(Name = "kernel.hardware_alert_manager_send_alert2")]
	public static void SendAlert2(string typeOfSync, string jobId)
	{
		ServiceLocator.Instance.GetService<IHardwareAlertManagerService>()?.SendAlert2(typeOfSync, jobId);
	}

	[KernelFunction(Name = "kernel.hardware_alert_manager_format_job_result")]
	public static string FormatJobResult(LuaTable jobResult)
	{
		string result = null;
		IHardwareResult hardwareResultFromLuaTable = GetHardwareResultFromLuaTable(jobResult);
		if (hardwareResultFromLuaTable != null)
		{
			IHardwareAlertManagerService hardwareAlertManagerService = ServiceLocator.Instance.GetService<IHardwareAlertManagerService>();
			if (hardwareAlertManagerService != null)
			{
				result = hardwareAlertManagerService.FormatJobResult(hardwareResultFromLuaTable);
			}
		}
		return result;
	}

	[KernelFunction(Name = "kernel.hardware_alert_manager_format_errors")]
	public static string FormatErrors(string jobId)
	{
		return ServiceLocator.Instance.GetService<IHardwareAlertManagerService>()?.FormatErrors(jobId);
	}

	private static IHardwareResult GetHardwareResultFromLuaTable(LuaTable jobResult)
	{
		HardwareResult hardwareResult = null;
		if (jobResult != null)
		{
			hardwareResult = new HardwareResult();
			hardwareResult.Code = GetLuaTableStringValue(jobResult, "code");
			if (GetIntegerValue(jobResult, "deck", out var deck))
			{
				hardwareResult.Deck = deck;
			}
			if (GetIntegerValue(jobResult, "slot", out var slot))
			{
				hardwareResult.Deck = slot;
			}
			hardwareResult.ItemID = GetLuaTableStringValue(jobResult, "item_id");
			hardwareResult.Message = GetLuaTableStringValue(jobResult, "message");
		}
		else
		{
			LogHelper.Instance.Log("Error in GetHardwareResultFromLuaTable.  LuaTable JobResult is null");
		}
		return hardwareResult;
	}

	private static string GetLuaTableStringValue(LuaTable luaTable, string propertyName)
	{
		string result = null;
		if (luaTable != null && luaTable[propertyName] != null)
		{
			result = luaTable[propertyName].ToString();
		}
		return result;
	}

	private static bool GetIntegerValue(LuaTable luaTable, string propertyName, out int value)
	{
		bool found = false;
		value = 0;
		if (luaTable != null)
		{
			object rawValue = luaTable[propertyName];
			if (rawValue != null && int.TryParse(rawValue.ToString(), out var parsed))
			{
				value = parsed;
				found = true;
			}
		}
		return found;
	}
}
