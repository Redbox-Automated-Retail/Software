using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.ServiceProcess;
using Microsoft.Win32;
using Redbox.Core;

namespace Redbox.UpdateManager.Kernel;

internal static class Win32ServiceFunctions
{
	internal static bool ServiceExists(string name)
	{
		ServiceController[] services = ServiceController.GetServices();
		foreach (ServiceController serviceController in services)
		{
			if (serviceController.ServiceName == name)
			{
				return true;
			}
		}
		return false;
	}

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

	internal static void SetStartupType(string name, string type)
	{
		try
		{
			List<ServiceController> list = new List<ServiceController>(ServiceController.GetServices());
			if (list.FindIndex((ServiceController p) => p.ServiceName == name) > -1)
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
			Process process2 = process;
			process2.Start();
			process2.WaitForExit(120000);
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("An unhandled exception was rasied in Win32ServiceFunctions.UninstallService.", e);
		}
	}

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
			Process process2 = process;
			process2.Start();
			process2.WaitForExit(120000);
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("An unhandled exception was rasied in Win32ServiceFunctions.InstallService.", e);
		}
	}

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
}
