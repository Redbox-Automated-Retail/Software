using System;
using System.Collections.Generic;
using System.Globalization;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.UpdateManager.Client;

namespace Redbox.UpdateServices.Proxy;

public class ConfigurationService : IConfigurationService
{
	public static ConfigurationService Instance => Singleton<ConfigurationService>.Instance;

	public ErrorList Initialize()
	{
		ErrorList errorList = new ErrorList();
		try
		{
			ConfigurationClient.Instance.Initialize();
			ServiceLocator.Instance.AddService(typeof(IConfigurationService), (object)Instance);
		}
		catch (Exception e)
		{
			errorList.Add(Redbox.KioskEngine.ComponentModel.Error.NewError("CS0001", "Unhandled exception occurred in ConfigurationSerivce.Initialize", e));
		}
		return errorList;
	}

	public ErrorList Add(string key, string filepath, string defaultCategory, TimeSpan? checkTime, Func<bool> canActivate, Action activateAction)
	{
		ErrorList errorList = new ErrorList();
		try
		{
			ConfigurationClient.Instance.Add(key, filepath, defaultCategory, checkTime, canActivate, activateAction);
		}
		catch (Exception e)
		{
			errorList.Add(Redbox.KioskEngine.ComponentModel.Error.NewError("CS0002", "Unhandled exception occurred in ConfigurationSerivce.Add", e));
		}
		return errorList;
	}

	public ErrorList Add(string key, string filepath, string defaultCategory)
	{
		ErrorList errorList = new ErrorList();
		try
		{
			ConfigurationClient.Instance.Add(key, filepath, defaultCategory);
		}
		catch (Exception ex)
		{
			if (ex.Message.StartsWith("Index (zero based)"))
			{
				LogHelper.Instance.LogException("Suppress known error iterating thru errors from config client when cannot communicate with UM to activate", ex);
				return errorList;
			}
			errorList.Add(Redbox.KioskEngine.ComponentModel.Error.NewError("CS0003", "Unhandled exception occurred in ConfigurationSerivce.Add", ex));
		}
		return errorList;
	}

	public ErrorList Remove(string key)
	{
		ErrorList errorList = new ErrorList();
		try
		{
			ConfigurationClient.Instance.Remove(key);
		}
		catch (Exception e)
		{
			errorList.Add(Redbox.KioskEngine.ComponentModel.Error.NewError("CS0004", "Unhandled exception occurred in ConfigurationSerivce.Remove", e));
		}
		return errorList;
	}

	public ErrorList RemoveAll()
	{
		ErrorList errorList = new ErrorList();
		try
		{
			ConfigurationClient.Instance.RemoveAll();
		}
		catch (Exception e)
		{
			errorList.Add(Redbox.KioskEngine.ComponentModel.Error.NewError("CS0005", "Unhandled exception occurred in ConfigurationSerivce.RemoveAll", e));
		}
		return errorList;
	}

	public List<Redbox.KioskEngine.ComponentModel.ConfigurationClientItem> GetItems()
	{
		List<Redbox.UpdateManager.Client.ConfigurationClientItem> items = ConfigurationClient.Instance.GetItems();
		List<Redbox.KioskEngine.ComponentModel.ConfigurationClientItem> list = new List<Redbox.KioskEngine.ComponentModel.ConfigurationClientItem>();
		foreach (Redbox.UpdateManager.Client.ConfigurationClientItem item in items)
		{
			list.Add(new Redbox.KioskEngine.ComponentModel.ConfigurationClientItem
			{
				Key = item.Key,
				DefaultCategory = item.DefaultCategory,
				CheckTime = item.CheckTime,
				FilePath = item.FilePath,
				HasCanActivate = item.HasCanActivate,
				HasActivateAction = item.HasActivateAction
			});
		}
		return list;
	}

	public ErrorList Start()
	{
		ErrorList errorList = new ErrorList();
		try
		{
			ConfigurationClient.Instance.Start();
		}
		catch (Exception e)
		{
			errorList.Add(Redbox.KioskEngine.ComponentModel.Error.NewError("CS0006", "Unhandled exception occurred in ConfigurationSerivce.Start", e));
		}
		return errorList;
	}

	public ErrorList Stop()
	{
		ErrorList errorList = new ErrorList();
		try
		{
			ConfigurationClient.Instance.Stop();
		}
		catch (Exception e)
		{
			errorList.Add(Redbox.KioskEngine.ComponentModel.Error.NewError("CS0007", "Unhandled exception occurred in ConfigurationSerivce.Stop", e));
		}
		return errorList;
	}

	public bool GetCategoryNames(string key, out List<string> list)
	{
		return ConfigurationClient.Instance.GetCategoryNames(key, out list);
	}

	public bool GetSettingNames(string key, out List<string> list)
	{
		return ConfigurationClient.Instance.GetSettingNames(key, out list);
	}

	public bool GetSettingNames(string key, string category, out List<string> list)
	{
		return ConfigurationClient.Instance.GetSettingNames(key, category, out list);
	}

	public bool TryGetValue<T>(string key, string category, string setting, out T? value)
	{
		return ConfigurationClient.Instance.TryGetValue<T>(key, category, setting, out value);
	}

	public bool TryGetTimeSpanRangeValue(string key, string category, string setting, out TimeSpan? startTime, out TimeSpan? endTime)
	{
		startTime = null;
		endTime = null;
		string? rangeText = null;
		bool parsed = false;
		if (ConfigurationClient.Instance.TryGetValue<string>(key, category, setting, out rangeText))
		{
			string[] parts = rangeText!.Split('-');
			if (parts.Length == 2 && TimeSpan.TryParseExact(parts[0], "h\\:mm", DateTimeFormatInfo.InvariantInfo, TimeSpanStyles.None, out var parsedStart) && TimeSpan.TryParseExact(parts[1], "h\\:mm", DateTimeFormatInfo.InvariantInfo, TimeSpanStyles.None, out var parsedEnd))
			{
				if (parsedStart <= parsedEnd)
				{
					startTime = parsedStart;
					endTime = parsedEnd;
					parsed = true;
				}
				else
				{
					LogHelper.Instance.Log("Error in ConfigurationService.TryGetTimeSpanRangeValue: end time is less than start time : " + rangeText + " for key: " + key + ", category: " + category + ", setting: " + setting);
				}
			}
			if (!parsed)
			{
				LogHelper.Instance.Log("Error in ConfigurationService.TryGetTimeSpanRangeValue: invalid time span range value : " + rangeText + " for key: " + key + ", category: " + category + ", setting: " + setting);
			}
		}
		return parsed;
	}

	public bool TryGetValue<T>(string key, string setting, out T? value)
	{
		return ConfigurationClient.Instance.TryGetValue<T>(key, setting, out value);
	}

	public bool TryGetObject(string key, string category, string setting, out object? value)
	{
		return ConfigurationClient.Instance.TryGetObject(key, category, setting, out value);
	}

	public bool TryGetObject(string key, string setting, out object? value)
	{
		return ConfigurationClient.Instance.TryGetObject(key, setting, out value);
	}

	public bool TryGetConfigurationData(string key, string category, string setting, out ConfigurationData? value)
	{
		value = null;
		bool found = ConfigurationClient.Instance.TryGetConfigurationData(key, category, setting, out var clientData);
		if (found)
		{
			value = ToConfigurationData(clientData!);
		}
		return found;
	}

	public bool TryGetConfigurationData(string key, string setting, out ConfigurationData? value)
	{
		value = null;
		bool found = ConfigurationClient.Instance.TryGetConfigurationData(key, setting, out var clientData);
		if (found)
		{
			value = ToConfigurationData(clientData!);
		}
		return found;
	}

	public bool GetAllConfigurationData(string key, out List<ConfigurationData> list)
	{
		list = new List<ConfigurationData>();
		bool allConfigurationData = ConfigurationClient.Instance.GetAllConfigurationData(key, out var clientDataList);
		if (allConfigurationData)
		{
			foreach (ClientConfigurationData clientData in clientDataList)
			{
				list.Add(ToConfigurationData(clientData));
			}
		}
		return allConfigurationData;
	}

	public bool GetAllConfigurationData(string key, string category, string setting, out List<ConfigurationData> list)
	{
		list = new List<ConfigurationData>();
		bool allConfigurationData = ConfigurationClient.Instance.GetAllConfigurationData(key, category, setting, out var clientDataList);
		if (allConfigurationData)
		{
			foreach (ClientConfigurationData clientData in clientDataList)
			{
				list.Add(ToConfigurationData(clientData));
			}
		}
		return allConfigurationData;
	}

	public bool GetAllConfigurationData(string key, string setting, out List<ConfigurationData> list)
	{
		list = new List<ConfigurationData>();
		bool allConfigurationData = ConfigurationClient.Instance.GetAllConfigurationData(key, setting, out var clientDataList);
		if (allConfigurationData)
		{
			foreach (ClientConfigurationData clientData in clientDataList)
			{
				list.Add(ToConfigurationData(clientData));
			}
		}
		return allConfigurationData;
	}

	private ConfigurationService()
	{
	}

	private static ConfigurationData ToConfigurationData(ClientConfigurationData data)
	{
		return new ConfigurationData
		{
			CategoryID = data.CategoryID,
			CategoryName = data.CategoryName,
			SettingName = data.SettingName,
			SettingDataType = data.SettingDataType,
			DefaultTypeName = data.DefaultTypeName,
			DefaultTypeRank = data.DefaultTypeRank,
			DefaultTypeDataType = data.DefaultTypeDataType,
			DefaultTypeKey = data.DefaultTypeKey,
			DefaultTypeValue = data.DefaultTypeValue,
			Value = data.Value,
			EffectiveTime = data.EffectiveTime,
			ExpireTime = data.ExpireTime
		};
	}
}
