using System;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Lua;

namespace Redbox.UpdateServices.Kernel;

public static class ConfigurationServiceFunctions
{
	[KernelFunction(Name = "kernel.config_getsettingdata")]
	internal static LuaTable ConfigGetResultTable(string configFile, string categoryName, string settingName)
	{
		IConfigurationService service = ServiceLocator.Instance.GetService<IConfigurationService>();
		if (service == null)
		{
			return null;
		}
		LuaTable luaTable = CreateResultTable(new ErrorList());
		if (service.TryGetConfigurationData(configFile, categoryName, settingName, out var value))
		{
			luaTable["value"] = value.Value;
			luaTable["success"] = true;
			luaTable["datatype"] = value.SettingDataType;
		}
		else
		{
			luaTable["value"] = null;
			luaTable["success"] = false;
		}
		return luaTable;
	}

	[KernelFunction(Name = "kernel.config_getvalue")]
	internal static object ConfigGetValue(string configFile, string categoryName, string settingName)
	{
		IConfigurationService service = ServiceLocator.Instance.GetService<IConfigurationService>();
		if (service == null)
		{
			return null;
		}
		if (service.TryGetObject(configFile, categoryName, settingName, out var value))
		{
			return value;
		}
		return null;
	}

	[KernelFunction(Name = "kernel.config_getvalidconfigdata")]
	internal static LuaTable ConfigGetTable(string configFile, string categoryName)
	{
		IConfigurationService service = ServiceLocator.Instance.GetService<IConfigurationService>();
		if (service == null)
		{
			return null;
		}
		IKernelService service2 = ServiceLocator.Instance.GetService<IKernelService>();
		LuaTable luaTable = (LuaTable)service2.NewTable();
		service.GetSettingNames(configFile, categoryName, out var list);
		if (list == null)
		{
			return null;
		}
		for (int i = 0; i < list.Count; i++)
		{
			if (service.TryGetConfigurationData(configFile, categoryName, list[i], out var value))
			{
				_ = value.EffectiveTime;
				if (!(value.EffectiveTime > DateTime.Now) && (value == null || !(value.ExpireTime < DateTime.Now)))
				{
					LuaTable luaTable2 = (LuaTable)service2.NewTable();
					luaTable2["category_name"] = value.CategoryName;
					luaTable2["setting_name"] = value.SettingName;
					luaTable2["value"] = value.Value;
					luaTable[i + 1] = luaTable2;
				}
			}
		}
		return luaTable;
	}

	private static LuaTable CreateResultTable(ErrorList errors)
	{
		IKernelService service = ServiceLocator.Instance.GetService<IKernelService>();
		LuaTable luaTable = (LuaTable)service.NewTable();
		luaTable["success"] = !errors.ContainsError();
		LuaTable luaTable2 = (LuaTable)service.NewTable();
		foreach (Redbox.KioskEngine.ComponentModel.Error error in errors)
		{
			luaTable2[error.Code] = error.Description;
		}
		luaTable["errors"] = luaTable2;
		return luaTable;
	}
}
