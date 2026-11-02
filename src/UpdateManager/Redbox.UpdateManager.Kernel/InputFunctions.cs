using System.IO;
using Redbox.Core;
using Redbox.Lua;
using Redbox.UpdateManager.ComponentModel;

namespace Redbox.UpdateManager.Kernel;

public static class InputFunctions
{
	[KernelFunction(Name = "kernel.inputyesno")]
	internal static string? AskYesNoQuestion(string title, string message)
	{
		IInputService inputService = ServiceLocator.Instance.GetService<IInputService>();
		if (inputService != null)
		{
			return inputService.AskYesNoQuestion(title, message).ToString("G").ToLower();
		}
		return null;
	}

	[KernelFunction(Name = "kernel.inputfindfile")]
	internal static LuaTable? FindFile()
	{
		IInputService inputService = ServiceLocator.Instance.GetService<IInputService>();
		if (inputService == null)
		{
			return null;
		}
		FileInfo fileInfo = inputService.LocateFile();
		if (fileInfo == null)
		{
			return null;
		}
		return new LuaTable(KernelService.Instance.LuaRuntime)
		{
			["name"] = fileInfo.Name,
			["length"] = fileInfo.Length,
			["directory"] = fileInfo.DirectoryName
		};
	}

	[KernelFunction(Name = "kernel.notifyinfo")]
	internal static void NotifyInfo(string title, string message)
	{
		ServiceLocator.Instance.GetService<IInputService>().NotifyInfo(title, message);
	}
}
