using System;
using System.IO;
using System.ServiceProcess;
using Microsoft.Win32;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;

namespace Redbox.KioskEngine.Kernel;

internal static class Win32ServiceFunctions
{
	private const string ServicesKey = "System\\CurrentControlSet\\Services";

	[KernelFunction(Name = "kernel.getservicepath")]
	internal static object GetServicePath(string serviceName)
	{
		string name = string.Format("{0}\\{1}", "System\\CurrentControlSet\\Services", serviceName);
		RegistryKey registryKey = null;
		try
		{
			registryKey = Registry.LocalMachine.OpenSubKey(name);
			if (registryKey == null)
			{
				return null;
			}
			return Path.GetDirectoryName(((string)registryKey.GetValue("ImagePath")).TrimStart('"').TrimEnd('"'));
		}
		catch (Exception ex)
		{
			LogFunctions.Log($"Caught an exception reading service information: {ex.Message}");
			return null;
		}
		finally
		{
			registryKey?.Close();
		}
	}

	[KernelFunction(Name = "kernel.startservice")]
	internal static bool StartService(string serviceName)
	{
		try
		{
			using ServiceController serviceController = new ServiceController(serviceName);
			serviceController.Start();
			return true;
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("An unhandled exception was raised in Win32ServiceFunctions.StartService.", e);
			return false;
		}
	}

	[KernelFunction(Name = "kernel.stopservice")]
	internal static bool StopService(string serviceName)
	{
		try
		{
			using ServiceController serviceController = new ServiceController(serviceName);
			serviceController.Stop();
			return true;
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("An unhandled exception was raised in Win32ServiceFunctions.StartService.", e);
			return false;
		}
	}

	[KernelFunction(Name = "kernel.getservicestatus")]
	internal static string GetServiceStatus(string serviceName)
	{
		try
		{
			using ServiceController serviceController = new ServiceController(serviceName);
			return serviceController.Status.ToString();
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("An unhandled exception was raised in Win32ServiceFunctions.GetServiceStatus.", e);
			return null;
		}
	}
}
