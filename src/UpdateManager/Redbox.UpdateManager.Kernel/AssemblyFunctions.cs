using System.Collections.Generic;
#if !NETFRAMEWORK
using System;
using System.Diagnostics;
using System.IO;
#endif
using Redbox.Core;
using Redbox.Lua;

namespace Redbox.UpdateManager.Kernel;

public static class AssemblyFunctions
{
	[KernelFunction(Name = "kernel.executeassemblyinstaller")]
	internal static LuaTable ExecuteAssemblyInstaller(string fileName, string mode, params object[] options)
	{
		List<string> arguments = new List<string>();
		if (options != null)
		{
			foreach (object option in options)
			{
				if (option != null)
				{
					arguments.Add(option.ToString()!);
				}
			}
		}
		List<string> installerErrors = new List<string>();
#if NETFRAMEWORK
		bool rebootRequired = AssemblyInstallerHelper.ExecuteInAppDomain(fileName, arguments.ToArray(), (string.Compare(mode, "install", ignoreCase: true) != 0) ? InstallMode.Uninstall : InstallMode.Install, installerErrors);
#else
		bool rebootRequired = RunInstallUtil(fileName, arguments, string.Compare(mode, "install", ignoreCase: true) == 0, installerErrors);
#endif
		LuaTable luaTable = new LuaTable(KernelService.Instance.LuaRuntime);
		luaTable["reboot_required"] = rebootRequired;
		LuaTable errorsTable = new LuaTable(KernelService.Instance.LuaRuntime);
		int index = 1;
		foreach (string error in installerErrors)
		{
			errorsTable[index++] = error;
		}
		luaTable["errors"] = errorsTable;
		return luaTable;
	}
#if !NETFRAMEWORK

	// .NET 10 has no System.Configuration.Install or AppDomains. The assemblies this installs are .NET Framework
	// service assemblies, so run .NET Framework's own InstallUtil (in-box on Windows 10/11) on them instead. InstallUtil
	// cannot report "reboot required", so this always returns false; failures go to the error list.
	private static bool RunInstallUtil(string fileName, List<string> options, bool install, List<string> errors)
	{
		// Same bitness as this process, as the in-process installer had (updatemgrd is x86).
		string framework = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Windows), "Microsoft.NET", System.Environment.Is64BitProcess ? "Framework64" : "Framework", "v4.0.30319", "InstallUtil.exe");
		if (!File.Exists(framework))
		{
			errors.Add("InstallUtil.exe (.NET Framework 4) was not found.");
			return false;
		}
		ProcessStartInfo startInfo = new ProcessStartInfo(framework)
		{
			UseShellExecute = false,
			CreateNoWindow = true,
			RedirectStandardOutput = true,
			RedirectStandardError = true
		};
		startInfo.ArgumentList.Add("/LogToConsole=true");
		if (!install)
		{
			startInfo.ArgumentList.Add("/u");
		}
		foreach (string option in options)
		{
			startInfo.ArgumentList.Add(option.StartsWith("/") || option.StartsWith("-") ? option : "/" + option);
		}
		startInfo.ArgumentList.Add(fileName);
		using Process process = Process.Start(startInfo)!; // used right below; throws on null, as before
		string output = process.StandardOutput.ReadToEnd() + process.StandardError.ReadToEnd();
		process.WaitForExit();
		if (process.ExitCode != 0)
		{
			errors.Add($"InstallUtil exited with {process.ExitCode}: {output}");
		}
		return false;
	}
#endif
}
