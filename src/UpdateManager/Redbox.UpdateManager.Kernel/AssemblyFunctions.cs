using System.Collections.Generic;
using Redbox.Core;
using Redbox.Lua;

namespace Redbox.UpdateManager.Kernel;

public static class AssemblyFunctions
{
	[KernelFunction(Name = "kernel.executeassemblyinstaller")]
	internal static LuaTable ExecuteAssemblyInstaller(string fileName, string mode, params object[] options)
	{
		List<string> list = new List<string>();
		if (options != null)
		{
			foreach (object obj in options)
			{
				if (obj != null)
				{
					list.Add(obj.ToString());
				}
			}
		}
		List<string> list2 = new List<string>();
		bool flag = AssemblyInstallerHelper.ExecuteInAppDomain(fileName, list.ToArray(), (string.Compare(mode, "install", ignoreCase: true) != 0) ? InstallMode.Uninstall : InstallMode.Install, list2);
		LuaTable luaTable = new LuaTable(KernelService.Instance.LuaRuntime);
		luaTable["reboot_required"] = flag;
		LuaTable luaTable2 = new LuaTable(KernelService.Instance.LuaRuntime);
		int num = 1;
		foreach (string item in list2)
		{
			luaTable2[num++] = item;
		}
		luaTable["errors"] = luaTable2;
		return luaTable;
	}
}
