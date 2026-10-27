using Redbox.Core;
using Redbox.Lua;
using Redbox.UpdateManager.ComponentModel;

namespace Redbox.UpdateManager.Kernel;

public static class KioskEngineFunctions
{
	[KernelFunction(Name = "kernel.kioskengineexportqueue")]
	internal static LuaTable ExportQueue(string path)
	{
		ErrorList errorList = ServiceLocator.Instance.GetService<IKioskEngineService>().ExportQueue(path);
		if (errorList.ContainsError())
		{
			errorList.ForEach(delegate(Redbox.UpdateManager.ComponentModel.Error e)
			{
				LogHelper.Instance.Log("{0} Details: {1}", e, e.Details);
			});
		}
		return CreateResultTable(errorList);
	}

	[KernelFunction(Name = "kernel.kioskengineexportsession")]
	internal static LuaTable ExportSession(string path)
	{
		ErrorList errorList = ServiceLocator.Instance.GetService<IKioskEngineService>().ExportSessions(path);
		if (errorList.ContainsError())
		{
			errorList.ForEach(delegate(Redbox.UpdateManager.ComponentModel.Error e)
			{
				LogHelper.Instance.Log("{0} Details: {1}", e, e.Details);
			});
		}
		return CreateResultTable(errorList);
	}

	[KernelFunction(Name = "kernel.kioskenginestart")]
	internal static LuaTable StartEngine()
	{
		ErrorList errorList = ServiceLocator.Instance.GetService<IKioskEngineService>().StartEngine();
		if (errorList.ContainsError())
		{
			errorList.ForEach(delegate(Redbox.UpdateManager.ComponentModel.Error e)
			{
				LogHelper.Instance.Log("{0} Details: {1}", e, e.Details);
			});
		}
		return CreateResultTable(errorList);
	}

	[KernelFunction(Name = "kernel.kioskengineisrunning")]
	internal static LuaTable IsEngineRunning()
	{
		ErrorList errorList = ServiceLocator.Instance.GetService<IKioskEngineService>().IsEngineRunning(out var isRunning);
		if (errorList.ContainsError())
		{
			errorList.ForEach(delegate(Redbox.UpdateManager.ComponentModel.Error e)
			{
				LogHelper.Instance.Log("{0} Details: {1}", e, e.Details);
			});
		}
		LuaTable luaTable = CreateResultTable(errorList);
		luaTable["isrunning"] = isRunning;
		return luaTable;
	}

	[KernelFunction(Name = "kernel.kioskenginestartwithbundle")]
	internal static LuaTable StartEngine(string bundle)
	{
		ErrorList errorList = ServiceLocator.Instance.GetService<IKioskEngineService>().StartEngine(bundle);
		if (errorList.ContainsError())
		{
			errorList.ForEach(delegate(Redbox.UpdateManager.ComponentModel.Error e)
			{
				LogHelper.Instance.Log("{0} Details: {1}", e, e.Details);
			});
		}
		return CreateResultTable(errorList);
	}

	[KernelFunction(Name = "kernel.kioskenginechangebundle")]
	internal static LuaTable ChangeBundle(string bundle)
	{
		ErrorList errorList = ServiceLocator.Instance.GetService<IKioskEngineService>().ChangeBundle(bundle);
		if (errorList.ContainsError())
		{
			errorList.ForEach(delegate(Redbox.UpdateManager.ComponentModel.Error e)
			{
				LogHelper.Instance.Log("{0} Details: {1}", e, e.Details);
			});
		}
		return CreateResultTable(errorList);
	}

	[KernelFunction(Name = "kernel.kioskengineshutdownblocking")]
	public static LuaTable ShutdownBlocking(int timeout, int tries)
	{
		ErrorList errorList = ServiceLocator.Instance.GetService<IKioskEngineService>().Shutdown(timeout, tries);
		if (errorList.ContainsError())
		{
			errorList.ForEach(delegate(Redbox.UpdateManager.ComponentModel.Error e)
			{
				LogHelper.Instance.Log("{0} Details: {1}", e, e.Details);
			});
		}
		return CreateResultTable(errorList);
	}

	[KernelFunction(Name = "kernel.kioskengineshutdown")]
	internal static LuaTable Shutdown()
	{
		ErrorList errorList = ServiceLocator.Instance.GetService<IKioskEngineService>().Shutdown();
		if (errorList.ContainsError())
		{
			errorList.ForEach(delegate(Redbox.UpdateManager.ComponentModel.Error e)
			{
				LogHelper.Instance.Log("{0} Details: {1}", e, e.Details);
			});
		}
		return CreateResultTable(errorList);
	}

	[KernelFunction(Name = "kernel.kioskenginebringtofront")]
	internal static LuaTable BringToFront()
	{
		ErrorList errorList = ServiceLocator.Instance.GetService<IKioskEngineService>().BringToFront();
		if (errorList.ContainsError())
		{
			errorList.ForEach(delegate(Redbox.UpdateManager.ComponentModel.Error e)
			{
				LogHelper.Instance.Log("{0} Details: {1}", e, e.Details);
			});
		}
		return CreateResultTable(errorList);
	}

	[KernelFunction(Name = "kernel.kioskengineexecutescript")]
	internal static LuaTable ExecuteScript(string path)
	{
		ErrorList errorList = ServiceLocator.Instance.GetService<IKioskEngineService>().ExecuteScript(path);
		if (errorList.ContainsError())
		{
			errorList.ForEach(delegate(Redbox.UpdateManager.ComponentModel.Error e)
			{
				LogHelper.Instance.Log("{0} Details: {1}", e, e.Details);
			});
		}
		return CreateResultTable(errorList);
	}

	[KernelFunction(Name = "kernel.kioskenginereloadbundle")]
	internal static LuaTable ReloadBundle()
	{
		ErrorList errorList = ServiceLocator.Instance.GetService<IKioskEngineService>().ReloadBundle();
		if (errorList.ContainsError())
		{
			errorList.ForEach(delegate(Redbox.UpdateManager.ComponentModel.Error e)
			{
				LogHelper.Instance.Log("{0} Details: {1}", e, e.Details);
			});
		}
		return CreateResultTable(errorList);
	}

	[KernelFunction(Name = "kernel.kioskenginegetmemoryusage")]
	internal static long GetMemoryUsage(string path)
	{
		ErrorList memoryUsage = ServiceLocator.Instance.GetService<IKioskEngineService>().GetMemoryUsage(out var total);
		if (memoryUsage.ContainsError())
		{
			memoryUsage.ForEach(delegate(Redbox.UpdateManager.ComponentModel.Error e)
			{
				LogHelper.Instance.Log("{0} Details: {1}", e, e.Details);
			});
		}
		return total;
	}

	[KernelFunction(Name = "kernel.kioskengineshowofflinescreen")]
	internal static LuaTable ShowOfflineScreen()
	{
		ErrorList errorList = ServiceLocator.Instance.GetService<IKioskEngineService>().ShowOfflineScreen();
		if (errorList.ContainsError())
		{
			errorList.ForEach(delegate(Redbox.UpdateManager.ComponentModel.Error e)
			{
				LogHelper.Instance.Log("{0} Details: {1}", e, e.Details);
			});
		}
		return CreateResultTable(errorList);
	}

	private static LuaTable CreateResultTable(ErrorList errors)
	{
		LuaTable luaTable = new LuaTable(KernelService.Instance.LuaRuntime);
		luaTable["success"] = !errors.ContainsError();
		LuaTable luaTable2 = new LuaTable(KernelService.Instance.LuaRuntime);
		foreach (Redbox.UpdateManager.ComponentModel.Error error in errors)
		{
			luaTable2[error.Code] = error.Description;
		}
		luaTable["errors"] = luaTable2;
		return luaTable;
	}
}
