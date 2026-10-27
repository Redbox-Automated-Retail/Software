using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Xml;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Lua;
using Redbox.REDS.Framework;

namespace Redbox.KioskEngine.Kernel;

public static class BundleFunctions
{
	[KernelFunction(Name = "kernel.activatebundle")]
	internal static void ActivateBundle(string productName, string productVersion)
	{
		try
		{
			ServiceLocator.Instance.GetService<IResourceBundleService>().Activate(productName, productVersion);
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("Error in Kernel Function activatebundle.", e);
		}
	}

	[KernelFunction(Name = "kernel.activatedefaultbundle")]
	internal static void ActivateBundleDefault()
	{
		try
		{
			ServiceLocator.Instance.GetService<IResourceBundleService>().ActivateDefaultBundle();
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("Error in Kernel Function activatedefaultbundle.", e);
		}
	}

	[KernelFunction(Name = "kernel.getresourcestoragename")]
	internal static string GetResourceStorageName(string name)
	{
		IResourceBundleService service = ServiceLocator.Instance.GetService<IResourceBundleService>();
		if (service == null)
		{
			return null;
		}
		if (string.IsNullOrEmpty(name))
		{
			return null;
		}
		return service.GetResource(name)?.StorageEntry.Path;
	}

	[KernelFunction(Name = "kernel.getresourceaspectproperty")]
	internal static string GetResourceAspectProperty(string resourceName, string aspectName, string propertyName)
	{
		IResourceBundleService service = ServiceLocator.Instance.GetService<IResourceBundleService>();
		if (service == null)
		{
			return null;
		}
		if (string.IsNullOrEmpty(resourceName) || string.IsNullOrEmpty(aspectName) || string.IsNullOrEmpty(propertyName))
		{
			return null;
		}
		IResource resource = service.GetResource(resourceName);
		if (resource == null)
		{
			return null;
		}
		IAspect aspect = resource.GetAspect(aspectName);
		if (aspect == null)
		{
			return null;
		}
		if (!aspect.Properties.ContainsKey(propertyName))
		{
			return null;
		}
		return aspect.Properties[propertyName];
	}

	[KernelFunction(Name = "kernel.isresourceoverridden")]
	internal static bool IsResourceOverridden(string name, object exclusions)
	{
		IResourceBundleService service = ServiceLocator.Instance.GetService<IResourceBundleService>();
		if (service == null)
		{
			return false;
		}
		if (string.IsNullOrEmpty(name))
		{
			return false;
		}
		List<string> list = new List<string>();
		if (exclusions is string item)
		{
			list.Add(item);
		}
		if (exclusions is LuaTable luaTable)
		{
			foreach (object value in luaTable.Values)
			{
				if (value != null)
				{
					list.Add(value.ToString());
				}
			}
		}
		return service.GetResource(name)?.IsOverriden(list.AsReadOnly()) ?? false;
	}

	[KernelFunction(Name = "kernel.executescript")]
	public static object[] ExecuteScript(string name)
	{
		List<object> list = new List<object>();
		ReadOnlyCollection<object> readOnlyCollection = KernelService.Instance.ExecuteScript(name);
		if (readOnlyCollection != null)
		{
			list.AddRange(readOnlyCollection);
		}
		return list.ToArray();
	}

	[KernelFunction(Name = "kernel.getresourceproperty")]
	internal static object GetResourceProperty(string resourceName, string propertyName)
	{
		return ServiceLocator.Instance.GetService<IResourceBundleService>()?.GetResource(resourceName)?[propertyName];
	}

	[KernelFunction(Name = "kernel.gettable")]
	internal static LuaTable GetTable(string name)
	{
		XmlNode xml = ServiceLocator.Instance.GetService<IResourceBundleService>().GetXml(name);
		if (xml == null)
		{
			return null;
		}
		XmlNodeList xmlNodeList = xml.SelectNodes("row");
		if (xmlNodeList == null)
		{
			return null;
		}
		LuaTable luaTable = new LuaTable(KernelService.Instance.LuaRuntime);
		decimal num = 1m;
		foreach (XmlNode item in xmlNodeList)
		{
			XmlNodeList xmlNodeList2 = item.SelectNodes("column");
			if (xmlNodeList2 == null)
			{
				continue;
			}
			LuaTable luaTable2 = new LuaTable(KernelService.Instance.LuaRuntime);
			foreach (XmlNode item2 in xmlNodeList2)
			{
				string attributeValue = item2.GetAttributeValue<string>("name");
				object value = item2.GetNodeValue(string.Empty);
				string attributeValue2 = item2.GetAttributeValue<string>("type");
				if (!string.IsNullOrEmpty(attributeValue2))
				{
					Type type = attributeValue2.ToType();
					if (type != null)
					{
						value = ConversionHelper.ChangeType(value, type);
					}
				}
				luaTable2[attributeValue] = value;
			}
			luaTable[num++] = luaTable2;
		}
		return luaTable;
	}

	[KernelFunction(Name = "kernel.getdictionary")]
	public static LuaTable GetDictionary(string name)
	{
		XmlNode xml = ServiceLocator.Instance.GetService<IResourceBundleService>().GetXml(name);
		if (xml == null)
		{
			return null;
		}
		XmlNodeList xmlNodeList = xml.SelectNodes("row");
		if (xmlNodeList == null)
		{
			return null;
		}
		LuaTable luaTable = new LuaTable(KernelService.Instance.LuaRuntime);
		foreach (XmlNode item in xmlNodeList)
		{
			XmlNodeList xmlNodeList2 = item.SelectNodes("column");
			if (xmlNodeList2 == null)
			{
				continue;
			}
			foreach (XmlNode item2 in xmlNodeList2)
			{
				string attributeValue = item2.GetAttributeValue<string>("name");
				object value = item2.GetNodeValue(string.Empty);
				string attributeValue2 = item2.GetAttributeValue<string>("type");
				if (!string.IsNullOrEmpty(attributeValue2))
				{
					Type type = attributeValue2.ToType();
					if (type != null)
					{
						value = ConversionHelper.ChangeType(value, type);
					}
				}
				luaTable[attributeValue] = value;
			}
		}
		return luaTable;
	}

	[KernelFunction(Name = "kernel.getfilter")]
	internal static string GetFilterProperty(string name)
	{
		return ServiceLocator.Instance.GetService<IResourceBundleService>().Filter[name];
	}

	[KernelFunction(Name = "kernel.setfilter")]
	internal static void SetFilterProperty(string name, string value)
	{
		ServiceLocator.Instance.GetService<IResourceBundleService>().Filter[name] = value;
	}

	[KernelFunction(Name = "kernel.getvendor")]
	[Obsolete("Kernel function is deprecated in favor of kernel.getfilter.")]
	internal static string GetVendor()
	{
		return ServiceLocator.Instance.GetService<IResourceBundleService>().Filter["vendor"];
	}

	[KernelFunction(Name = "kernel.setvendor")]
	[Obsolete("Kernel function is deprecated in favor of kernel.setfilter.")]
	internal static void SetVendor(string vendor)
	{
		ServiceLocator.Instance.GetService<IResourceBundleService>().Filter["vendor"] = vendor;
		ResetCurrentView();
	}

	[KernelFunction(Name = "kernel.getmarket")]
	[Obsolete("Kernel function is deprecated in favor of kernel.getfilter.")]
	internal static string GetMarket()
	{
		return ServiceLocator.Instance.GetService<IResourceBundleService>().Filter["market"];
	}

	[KernelFunction(Name = "kernel.setmarket")]
	[Obsolete("Kernel function is deprecated in favor of kernel.setfilter.")]
	internal static void SetMarket(string market)
	{
		ServiceLocator.Instance.GetService<IResourceBundleService>().Filter["market"] = market;
		ResetCurrentView();
	}

	[KernelFunction(Name = "kernel.getstorenumber")]
	[Obsolete("Kernel function is deprecated in favor of kernel.getfilter.")]
	internal static string GetStoreNumber()
	{
		return ServiceLocator.Instance.GetService<IResourceBundleService>().Filter["store_number"];
	}

	[KernelFunction(Name = "kernel.setstorenumber")]
	[Obsolete("Kernel function is deprecated in favor of kernel.setfilter.")]
	internal static void SetStoreNumber(string storeNumber)
	{
		ServiceLocator.Instance.GetService<IResourceBundleService>().Filter["store_number"] = storeNumber;
		ResetCurrentView();
	}

	[KernelFunction(Name = "kernel.getculture")]
	[Obsolete("Kernel function is deprecated in favor of kernel.getfilter.")]
	internal static string GetCulture()
	{
		return ServiceLocator.Instance.GetService<IResourceBundleService>().Filter["culture"];
	}

	[KernelFunction(Name = "kernel.setculture")]
	[Obsolete("Kernel function is deprecated in favor of kernel.setfilter.")]
	internal static void SetCulture(string culture)
	{
		ServiceLocator.Instance.GetService<IResourceBundleService>().Filter["culture"] = culture;
		ResetCurrentView();
	}

	[KernelFunction(Name = "kernel.getenvironment")]
	[Obsolete("Kernel function is deprecated in favor of kernel.getfilter.")]
	internal static string GetEnvironment()
	{
		return ServiceLocator.Instance.GetService<IResourceBundleService>().Filter["environment"];
	}

	[KernelFunction(Name = "kernel.setenvironment")]
	[Obsolete("Kernel function is deprecated in favor of kernel.setfilter.")]
	internal static void SetEnvironment(string environment)
	{
		ServiceLocator.Instance.GetService<IResourceBundleService>().Filter["environment"] = environment;
		ResetCurrentView();
	}

	[KernelFunction(Name = "kernel.getruntime")]
	[Obsolete("Kernel function is deprecated in favor of kernel.getfilter.")]
	internal static string GetRuntime()
	{
		return ServiceLocator.Instance.GetService<IResourceBundleService>().Filter["runtime"];
	}

	[KernelFunction(Name = "kernel.getplatform")]
	[Obsolete("Kernel function is deprecated in favor of kernel.getfilter.")]
	internal static string GetPlatform()
	{
		return ServiceLocator.Instance.GetService<IResourceBundleService>().Filter["platform"];
	}

	[KernelFunction(Name = "kernel.getapplication")]
	[Obsolete("Kernel function is deprecated in favor of kernel.getfilter.")]
	internal static string GetApplication()
	{
		return ServiceLocator.Instance.GetService<IResourceBundleService>().Filter["application"];
	}

	[KernelFunction(Name = "kernel.getswitchstatusmessage")]
	internal static string GetSwitchStatusMessage()
	{
		return ServiceLocator.Instance.GetService<IResourceBundleService>().SwitchStatusMessage;
	}

	[KernelFunction(Name = "kernel.setswitchstatusmessage")]
	internal static void SetSwitchStatusMessage(string message)
	{
		ServiceLocator.Instance.GetService<IResourceBundleService>().SwitchStatusMessage = message;
	}

	[KernelFunction(Name = "kernel.isculturevalid")]
	internal static bool IsCultureValid(string culture)
	{
		return CultureInfo.GetCultures(CultureTypes.SpecificCultures).Any((CultureInfo c) => c.Name == culture);
	}

	[KernelFunction(Name = "kernel.load_xps_document")]
	internal static void LoadXPSDocument(string documentName)
	{
		LogHelper.Instance.Log("Starting load of XPS document");
		ServiceLocator.Instance.GetService<IResourceBundleService>()?.GetXpsDocument(documentName);
		LogHelper.Instance.Log("Finished load of XPS document");
	}

	private static void ResetCurrentView()
	{
		IViewService service = ServiceLocator.Instance.GetService<IViewService>();
		service.ClearCache(clearResourceViewsOnly: true);
		service.Show();
	}
}
