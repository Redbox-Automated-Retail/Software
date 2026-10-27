using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.ServiceProcess;
using Microsoft.Win32;
using Redbox.Core;
using Redbox.Lua;

namespace Redbox.UpdateManager.Kernel;

public static class Win32ServiceFunctions
{
	[KernelFunction(Name = "kernel.serviceexists")]
	internal static bool ServiceExists(string name)
	{
		ServiceController[] services = ServiceController.GetServices();
		for (int i = 0; i < services.Length; i++)
		{
			if (services[i].ServiceName == name)
			{
				return true;
			}
		}
		return false;
	}

	[KernelFunction(Name = "kernel.stopservice")]
	internal static void StopService(string name)
	{
		try
		{
			using ServiceController serviceController = new ServiceController(name);
			serviceController.Stop();
			serviceController.WaitForStatus(ServiceControllerStatus.Stopped, TimeSpan.FromMinutes(5.0));
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("An unhandled exception was rasied in Win32ServiceFunctions.StopService.", e);
		}
	}

	[KernelFunction(Name = "kernel.getservicestatus")]
	internal static string GetServiceStatus(string name)
	{
		try
		{
			using ServiceController serviceController = new ServiceController(name);
			return serviceController.Status.ToString();
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("An unhandled exception was rasied in Win32ServiceFunctions.GetServiceStatus.", e);
		}
		return "Unknown";
	}

	[KernelFunction(Name = "kernel.setservicestarttype")]
	internal static void SetStartupType(string name, string type)
	{
		try
		{
			if (new List<ServiceController>(ServiceController.GetServices()).FindIndex((ServiceController p) => p.ServiceName == name) > -1)
			{
				string keyName = $"HKEY_LOCAL_MACHINE\\SYSTEM\\CurrentControlSet\\Services\\{name}";
				switch (type)
				{
				case "automatic":
					Registry.SetValue(keyName, "Start", 2);
					break;
				case "disabled":
					Registry.SetValue(keyName, "Start", 4);
					break;
				case "manual":
					Registry.SetValue(keyName, "Start", 3);
					break;
				}
			}
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("An unhandled exception was rasied in Win32ServiceFunctions.SetStartupType.", e);
		}
	}

	[KernelFunction(Name = "kernel.uninstallservice")]
	internal static void UninstallService(string name)
	{
		try
		{
			Process process = new Process();
			process.StartInfo = new ProcessStartInfo("SC.exe", $"delete {name}")
			{
				CreateNoWindow = true,
				WindowStyle = ProcessWindowStyle.Hidden
			};
			process.Start();
			process.WaitForExit(120000);
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("An unhandled exception was rasied in Win32ServiceFunctions.UninstallService.", e);
		}
	}

	[KernelFunction(Name = "kernel.installservice")]
	internal static void InstallService(string name, string displayName, string file, string type, string startType, string dependencies)
	{
		try
		{
			string text = string.Format("create {0} displayName= {1} binPath= {2} type= {3} start= {4}", name, displayName, file, type ?? "own", startType ?? "auto");
			if (!string.IsNullOrEmpty(dependencies))
			{
				text += $" depend= {dependencies}";
			}
			Process process = new Process();
			process.StartInfo = new ProcessStartInfo("SC.exe", text)
			{
				CreateNoWindow = true,
				WindowStyle = ProcessWindowStyle.Hidden
			};
			process.Start();
			process.WaitForExit(120000);
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("An unhandled exception was rasied in Win32ServiceFunctions.InstallService.", e);
		}
	}

	[KernelFunction(Name = "kernel.pauseservice")]
	internal static void PauseService(string name)
	{
		try
		{
			using ServiceController serviceController = new ServiceController(name);
			serviceController.Pause();
			serviceController.WaitForStatus(ServiceControllerStatus.Paused, TimeSpan.FromMinutes(5.0));
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("An unhandled exception was rasied in Win32ServiceFunctions.PauseService.", e);
		}
	}

	[KernelFunction(Name = "kernel.resumeservice")]
	internal static void ResumeService(string name)
	{
		try
		{
			using ServiceController serviceController = new ServiceController(name);
			serviceController.Continue();
			serviceController.WaitForStatus(ServiceControllerStatus.Running, TimeSpan.FromMinutes(5.0));
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("An unhandled exception was rasied in Win32ServiceFunctions.ResumeService.", e);
		}
	}

	[KernelFunction(Name = "kernel.startservice")]
	internal static void StartService(string name)
	{
		try
		{
			using ServiceController serviceController = new ServiceController(name);
			serviceController.Start();
			serviceController.WaitForStatus(ServiceControllerStatus.Running, TimeSpan.FromMinutes(5.0));
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("An unhandled exception was rasied in Win32ServiceFunctions.StartService.", e);
		}
	}

	[KernelFunction(Name = "kernel.getservices")]
	internal static LuaTable GetServices()
	{
		LuaTable luaTable = new LuaTable(KernelService.Instance.LuaRuntime);
		int num = 1;
		try
		{
			ServiceController[] services = ServiceController.GetServices();
			foreach (ServiceController serviceController in services)
			{
				LuaTable luaTable2 = new LuaTable(KernelService.Instance.LuaRuntime);
				luaTable2["name"] = serviceController.ServiceName;
				luaTable2["display_name"] = serviceController.DisplayName;
				luaTable2["state"] = serviceController.Status.ToString();
				luaTable[num] = luaTable2;
				num++;
			}
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("An unhandled exception was rasied in Win32ServiceFunctions.GetServices.", e);
		}
		return luaTable;
	}
}
