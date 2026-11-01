using System.Diagnostics;
using Redbox.Core;
using Redbox.UpdateManager.ComponentModel;

namespace Redbox.UpdateManager.Kernel;

public static class ComObjectFunctions
{
	[KernelFunction(Name = "kernel.ismoduleselfregistering")]
	internal static bool IsModuleSelfRegistering(string fileName)
	{
		IMacroService macroService = ServiceLocator.Instance.GetService<IMacroService>();
		if (macroService != null)
		{
			return SelfRegistrationTool.IsModuleSelfRegistering(macroService.ExpandProperties(fileName));
		}
		return false;
	}

	[KernelFunction(Name = "kernel.registercomobject")]
	internal static bool RegisterComObject(string fileName)
	{
		string text = ServiceLocator.Instance.GetService<IMacroService>().ExpandProperties(fileName);
		if (SelfRegistrationTool.IsModuleSelfRegistering(text))
		{
			SelfRegistrationTool.Register(text);
			return true;
		}
		Process process = new Process();
		process.StartInfo = new ProcessStartInfo
		{
			CreateNoWindow = true,
			UseShellExecute = false,
			FileName = "regsvr32.exe",
			WindowStyle = ProcessWindowStyle.Hidden,
			Arguments = $"/s {text}"
		};
		process.Start();
		process.WaitForExit();
		return process.ExitCode == 0;
	}

	[KernelFunction(Name = "kernel.unregistercomobject")]
	internal static bool UnregisterComObject(string fileName)
	{
		string text = ServiceLocator.Instance.GetService<IMacroService>().ExpandProperties(fileName);
		if (SelfRegistrationTool.IsModuleSelfRegistering(text))
		{
			SelfRegistrationTool.Unregister(text);
			return true;
		}
		Process process = new Process();
		process.StartInfo = new ProcessStartInfo
		{
			CreateNoWindow = true,
			UseShellExecute = false,
			FileName = "regsvr32.exe",
			WindowStyle = ProcessWindowStyle.Hidden,
			Arguments = $"/s /u {text}"
		};
		process.Start();
		process.WaitForExit();
		return process.ExitCode == 0;
	}
}
