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
		IKernelService service = ServiceLocator.Instance.GetService<IKernelService>();
		LuaTable luaTable = (LuaTable)service.NewTable();
		luaTable["success"] = result.Success;
		LuaTable luaTable2 = (LuaTable)service.NewTable();
		luaTable2["hours"] = result.ExecutionTime.Hours;
		luaTable2["minutes"] = result.ExecutionTime.Minutes;
		luaTable2["seconds"] = result.ExecutionTime.Seconds;
		luaTable2["milliseconds"] = result.ExecutionTime.Milliseconds;
		luaTable["execution_time"] = luaTable2;
		LuaTable luaTable3 = (LuaTable)service.NewTable();
		foreach (string key in result.Properties.Keys)
		{
			object obj = result.Properties[key];
			if (!(obj is IEnumerable) || obj is string)
			{
				luaTable3[key] = obj;
			}
		}
		luaTable["properties"] = luaTable3;
		int num = 1;
		LuaTable luaTable4 = (LuaTable)service.NewTable();
		foreach (Redbox.KioskEngine.ComponentModel.Error error in result.Errors)
		{
			LuaTable luaTable5 = (LuaTable)service.NewTable();
			luaTable5["code"] = error.Code;
			luaTable5["is_warning"] = error.IsWarning;
			luaTable5["description"] = error.Description;
			luaTable5["details"] = error.Details;
			luaTable4[num++] = luaTable5;
		}
		luaTable["errors"] = luaTable4;
		return luaTable;
	}

	public static LuaTable AsTableExtended(this IRemoteServiceResult result)
	{
		IKernelService service = ServiceLocator.Instance.GetService<IKernelService>();
		LuaTable luaTable = (LuaTable)service.NewTable();
		luaTable["success"] = result.Success;
		LuaTable luaTable2 = (LuaTable)service.NewTable();
		luaTable2["hours"] = result.ExecutionTime.Hours;
		luaTable2["minutes"] = result.ExecutionTime.Minutes;
		luaTable2["seconds"] = result.ExecutionTime.Seconds;
		luaTable2["milliseconds"] = result.ExecutionTime.Milliseconds;
		luaTable["execution_time"] = luaTable2;
		if (result.Properties is Dictionary<string, object> properties)
		{
			luaTable["properties"] = ConvertDictionary(properties);
		}
		else
		{
			LogHelper.Instance.Log("IRemoteServiceResult: IRemoteServiceResult.Properties could not be cast as Dictionary");
			luaTable["properties"] = (LuaTable)service.NewTable();
		}
		int num = 1;
		LuaTable luaTable3 = (LuaTable)service.NewTable();
		foreach (Redbox.KioskEngine.ComponentModel.Error error in result.Errors)
		{
			LuaTable luaTable4 = (LuaTable)service.NewTable();
			luaTable4["code"] = error.Code;
			luaTable4["is_warning"] = error.IsWarning;
			luaTable4["description"] = error.Description;
			luaTable4["details"] = error.Details;
			luaTable3[num++] = luaTable4;
		}
		luaTable["errors"] = luaTable3;
		return luaTable;
	}

	private static LuaTable ConvertDictionary(Dictionary<string, object> properties)
	{
		LuaTable luaTable = (LuaTable)ServiceLocator.Instance.GetService<IKernelService>().NewTable();
		foreach (string key in properties.Keys)
		{
			object obj = properties[key];
			if (obj is Dictionary<string, object>)
			{
				if (obj is Dictionary<string, object> properties2)
				{
					luaTable[key] = ConvertDictionary(properties2);
				}
				else
				{
					LogHelper.Instance.Log("IRemoteServiceResult: Property[" + key + "] is Dictionary<string,object> but could be converted.");
				}
			}
			else if (obj is string || obj is bool || obj is int)
			{
				luaTable[key] = obj;
			}
			else if (obj != null)
			{
				LogHelper.Instance.Log("IRemoteServiceResult: unexpected data type for Property[" + key + "] skipped.");
			}
		}
		return luaTable;
	}
}
