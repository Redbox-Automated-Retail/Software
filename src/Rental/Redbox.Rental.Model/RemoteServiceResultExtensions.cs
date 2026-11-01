using System.Collections;
using System.Collections.Generic;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.ComponentModel.KioskServices;
using Redbox.Lua;

namespace Redbox.Rental.Model;

public static class RemoteServiceResultExtensions
{
	public static LuaTable AsTable(this IRemoteServiceResult result)
	{
		IKernelService kernelService = ServiceLocator.Instance.GetService<IKernelService>();
		LuaTable luaTable = (LuaTable)kernelService.NewTable();
		luaTable["success"] = result.Success;
		LuaTable executionTimeTable = (LuaTable)kernelService.NewTable();
		executionTimeTable["hours"] = result.ExecutionTime.Hours;
		executionTimeTable["minutes"] = result.ExecutionTime.Minutes;
		executionTimeTable["seconds"] = result.ExecutionTime.Seconds;
		executionTimeTable["milliseconds"] = result.ExecutionTime.Milliseconds;
		luaTable["execution_time"] = executionTimeTable;
		LuaTable propertiesTable = (LuaTable)kernelService.NewTable();
		foreach (string key in result.Properties.Keys)
		{
			object propertyValue = result.Properties[key];
			if (!(propertyValue is IEnumerable) || propertyValue is string)
			{
				propertiesTable[key] = propertyValue;
			}
		}
		luaTable["properties"] = propertiesTable;
		int errorIndex = 1;
		LuaTable errorsTable = (LuaTable)kernelService.NewTable();
		foreach (Redbox.KioskEngine.ComponentModel.Error error in result.Errors)
		{
			LuaTable errorTable = (LuaTable)kernelService.NewTable();
			errorTable["code"] = error.Code;
			errorTable["is_warning"] = error.IsWarning;
			errorTable["description"] = error.Description;
			errorTable["details"] = error.Details;
			errorsTable[errorIndex++] = errorTable;
		}
		luaTable["errors"] = errorsTable;
		return luaTable;
	}

	public static LuaTable AsTableExtended(this IRemoteServiceResult result)
	{
		IKernelService kernelService = ServiceLocator.Instance.GetService<IKernelService>();
		LuaTable luaTable = (LuaTable)kernelService.NewTable();
		luaTable["success"] = result.Success;
		LuaTable executionTimeTable = (LuaTable)kernelService.NewTable();
		executionTimeTable["hours"] = result.ExecutionTime.Hours;
		executionTimeTable["minutes"] = result.ExecutionTime.Minutes;
		executionTimeTable["seconds"] = result.ExecutionTime.Seconds;
		executionTimeTable["milliseconds"] = result.ExecutionTime.Milliseconds;
		luaTable["execution_time"] = executionTimeTable;
		if (result.Properties is Dictionary<string, object> properties)
		{
			luaTable["properties"] = ConvertDictionary(properties);
		}
		else
		{
			LogHelper.Instance.Log("IRemoteServiceResult: IRemoteServiceResult.Properties could not be cast as Dictionary");
			luaTable["properties"] = (LuaTable)kernelService.NewTable();
		}
		int errorIndex = 1;
		LuaTable errorsTable = (LuaTable)kernelService.NewTable();
		foreach (Redbox.KioskEngine.ComponentModel.Error error in result.Errors)
		{
			LuaTable errorTable = (LuaTable)kernelService.NewTable();
			errorTable["code"] = error.Code;
			errorTable["is_warning"] = error.IsWarning;
			errorTable["description"] = error.Description;
			errorTable["details"] = error.Details;
			errorsTable[errorIndex++] = errorTable;
		}
		luaTable["errors"] = errorsTable;
		return luaTable;
	}

	private static LuaTable ConvertDictionary(Dictionary<string, object> properties)
	{
		LuaTable luaTable = (LuaTable)ServiceLocator.Instance.GetService<IKernelService>().NewTable();
		foreach (string key in properties.Keys)
		{
			object propertyValue = properties[key];
			if (propertyValue is Dictionary<string, object>)
			{
				if (propertyValue is Dictionary<string, object> nestedProperties)
				{
					luaTable[key] = ConvertDictionary(nestedProperties);
				}
				else
				{
					LogHelper.Instance.Log("IRemoteServiceResult: Property[" + key + "] is Dictionary<string,object> but could be converted.");
				}
			}
			else if (propertyValue is string || propertyValue is bool || propertyValue is int)
			{
				luaTable[key] = propertyValue;
			}
			else if (propertyValue != null)
			{
				LogHelper.Instance.Log("IRemoteServiceResult: unexpected data type for Property[" + key + "] skipped.");
			}
		}
		return luaTable;
	}
}
