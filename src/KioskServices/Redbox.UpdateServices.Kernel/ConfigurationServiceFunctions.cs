using System;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Lua;

namespace Redbox.UpdateServices.Kernel;

public static class ConfigurationServiceFunctions
{
	[KernelFunction(Name = "kernel.config_getsettingdata")]
	internal static LuaTable? ConfigGetResultTable(string configFile, string categoryName, string settingName)
	{
		IConfigurationService configurationService = ServiceLocator.Instance.GetService<IConfigurationService>();
		if (configurationService == null)
		{
			return null;
		}
		LuaTable luaTable = CreateResultTable(new ErrorList());
		if (configurationService.TryGetConfigurationData(configFile, categoryName, settingName, out var value))
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
	internal static object? ConfigGetValue(string configFile, string categoryName, string settingName)
	{
		IConfigurationService configurationService = ServiceLocator.Instance.GetService<IConfigurationService>();
		if (configurationService == null)
		{
			return null;
		}
		if (configurationService.TryGetObject(configFile, categoryName, settingName, out var value))
		{
			return value;
		}
		return null;
	}

	[KernelFunction(Name = "kernel.config_getvalidconfigdata")]
	internal static LuaTable? ConfigGetTable(string configFile, string categoryName)
	{
		IConfigurationService configurationService = ServiceLocator.Instance.GetService<IConfigurationService>();
		if (configurationService == null)
		{
			return null;
		}
		IKernelService kernelService = ServiceLocator.Instance.GetService<IKernelService>();
		LuaTable resultTable = (LuaTable)kernelService.NewTable();
		configurationService.GetSettingNames(configFile, categoryName, out var settingNames);
		if (settingNames == null)
		{
			return null;
		}
		for (int i = 0; i < settingNames.Count; i++)
		{
			if (configurationService.TryGetConfigurationData(configFile, categoryName, settingNames[i], out var configurationData))
			{
				_ = configurationData.EffectiveTime;
				if (!(configurationData.EffectiveTime > DateTime.Now) && (configurationData == null || !(configurationData.ExpireTime < DateTime.Now)))
				{
					LuaTable settingTable = (LuaTable)kernelService.NewTable();
					settingTable["category_name"] = configurationData!.CategoryName;
					settingTable["setting_name"] = configurationData.SettingName;
					settingTable["value"] = configurationData.Value;
					resultTable[i + 1] = settingTable;
				}
			}
		}
		return resultTable;
	}

	private static LuaTable CreateResultTable(ErrorList errors)
	{
		IKernelService kernelService = ServiceLocator.Instance.GetService<IKernelService>();
		LuaTable resultTable = (LuaTable)kernelService.NewTable();
		resultTable["success"] = !errors.ContainsError();
		LuaTable errorTable = (LuaTable)kernelService.NewTable();
		foreach (Redbox.KioskEngine.ComponentModel.Error error in errors)
		{
			errorTable[error.Code] = error.Description;
		}
		resultTable["errors"] = errorTable;
		return resultTable;
	}
}
