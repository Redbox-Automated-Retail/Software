using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Threading;
using Redbox.Core;
using Redbox.Lua;
using Redbox.UpdateManager.ComponentModel;

namespace Redbox.UpdateManager.Kernel;

public sealed class KernelService : IKernelService
{
	private bool _performedShutdown;

	private ReaderWriterLockSlim _lock = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);

	private int _lockTimeout = 3000;

	public static KernelService Instance => Singleton<KernelService>.Instance;

	public string StartupScript { get; set; }

	public bool ScriptCompleted { get; private set; }

	public ShutdownType ShutdownType { get; set; }

	public bool IsShuttingDown => ShutdownType != ShutdownType.None;

	internal LuaVirtualMachine LuaRuntime { get; set; }

	internal LuaDebug? DebugState { get; set; }

	public void Initialize()
	{
		Reset();
		ShutdownType = ShutdownType.None;
		ServiceLocator.Instance.AddService(typeof(IKernelService), this);
	}

	public void Reset()
	{
		if (!_lock.TryEnterWriteLock(_lockTimeout))
		{
			new ErrorList().Add(Redbox.UpdateManager.ComponentModel.Error.NewError("KernelService.Reset", "TryEnterWriteLock timeout expired prior to acquiring the lock."));
			return;
		}
		try
		{
			InternalReset();
		}
		finally
		{
			_lock.ExitWriteLock();
		}
	}

	public Dictionary<object, object> GetTable(string name)
	{
		if (!(LuaRuntime[name] is LuaTable luaTable))
		{
			return null;
		}
		Dictionary<object, object> dictionary = new Dictionary<object, object>();
		foreach (object key in luaTable.Keys)
		{
			dictionary.Add(key, luaTable[key]);
		}
		return dictionary;
	}

	public string FormatLuaValue(object value)
	{
		return LuaHelper.FormatLuaValue(value);
	}

	public ReadOnlyCollection<object> ExecuteChunk(string chunk)
	{
		bool locked;
		return ExecuteChunkLockCheck(chunk, out locked);
	}

	public ReadOnlyCollection<object> ExecuteChunkLockCheck(string chunk, out bool locked)
	{
		locked = false;
		if (!_lock.TryEnterWriteLock(_lockTimeout))
		{
			locked = true;
			new ErrorList().Add(Redbox.UpdateManager.ComponentModel.Error.NewError("KernelService.ExecuteChunck(chunk)", "TryEnterWriteLock timeout expired prior to acquiring the lock."));
			return null;
		}
		try
		{
			if (IsShuttingDown)
			{
				return null;
			}
			return LuaRuntime.DoString(chunk);
		}
		finally
		{
			_lock.ExitWriteLock();
		}
	}

	public ReadOnlyCollection<object> ExecuteChunk(string chunk, bool reset)
	{
		if (!_lock.TryEnterWriteLock(_lockTimeout))
		{
			new ErrorList().Add(Redbox.UpdateManager.ComponentModel.Error.NewError("KernelService.ExecuteChunck(chunk,reset)", "TryEnterWriteLock timeout expired prior to acquiring the lock."));
			return null;
		}
		try
		{
			if (IsShuttingDown)
			{
				return null;
			}
			if (reset)
			{
				InternalReset();
			}
			return LuaRuntime.DoString(chunk);
		}
		finally
		{
			_lock.ExitWriteLock();
		}
	}

	public ReadOnlyCollection<object> ExecuteChunkNoLock(string chunk, bool reset, string resultTableName, out Dictionary<object, object> result, out bool scriptCompleted)
	{
		return InternalExecuteChunk(chunk, reset, resultTableName, out result, out scriptCompleted);
	}

	public ReadOnlyCollection<object> ExecuteChunk(string chunk, bool reset, string resultTableName, out Dictionary<object, object> result, out bool scriptCompleted)
	{
		if (!_lock.TryEnterWriteLock(_lockTimeout))
		{
			new ErrorList().Add(Redbox.UpdateManager.ComponentModel.Error.NewError("KernelService.ExecuteChunck(chunck,reset,resulttablename,out,out", "TryEnterWriteLock timeout expired prior to acquiring the lock."));
			result = null;
			scriptCompleted = false;
			return null;
		}
		try
		{
			return InternalExecuteChunk(chunk, reset, resultTableName, out result, out scriptCompleted);
		}
		finally
		{
			_lock.ExitWriteLock();
		}
	}

	public void Execute(Action action)
	{
		if (!_lock.TryEnterWriteLock(_lockTimeout))
		{
			new ErrorList().Add(Redbox.UpdateManager.ComponentModel.Error.NewError("KernelService.Execute", "TryEnterWriteLock timeout expired prior to acquiring the lock."));
			return;
		}
		try
		{
			if (!IsShuttingDown)
			{
				action?.Invoke();
			}
		}
		finally
		{
			_lock.ExitWriteLock();
		}
	}

	public void Shutdown()
	{
		if (LuaRuntime != null)
		{
			LuaRuntime.Dispose();
		}
	}

	public void AddGlobal(string key, object value)
	{
		if (value is string)
		{
			value = ServiceLocator.Instance.GetService<IMacroService>().ExpandProperties((string)value);
		}
		LuaRuntime[key] = value;
	}

	public void SetScriptComplete()
	{
		ScriptCompleted = true;
	}

	public void PerformShutdown()
	{
		if (!_performedShutdown)
		{
			switch (ShutdownType)
			{
			case ShutdownType.Reboot:
				_performedShutdown = true;
				ShutdownTool.Shutdown(ShutdownFlags.Reboot | ShutdownFlags.Force | ShutdownFlags.ForceIfHung, ShutdownReason.FlagPlanned);
				break;
			case ShutdownType.Shutdown:
				_performedShutdown = true;
				ShutdownTool.Shutdown(ShutdownFlags.ShutDown | ShutdownFlags.Force, ShutdownReason.FlagPlanned);
				break;
			}
		}
	}

	internal void ExecutePreloadScripts()
	{
	}

	internal void InternalReset()
	{
		ScriptCompleted = false;
		if (LuaRuntime != null)
		{
			LuaRuntime.Dispose();
			LuaRuntime = null;
		}
		LuaRuntime = new LuaVirtualMachine();
		ConfigureKernelFunctions();
		ExecutePreloadScripts();
	}

	internal ReadOnlyCollection<object> InternalExecuteChunk(string chunk, bool reset, string resultTableName, out Dictionary<object, object> result, out bool scriptCompleted)
	{
		result = null;
		scriptCompleted = false;
		if (IsShuttingDown)
		{
			return null;
		}
		if (reset)
		{
			InternalReset();
		}
		ReadOnlyCollection<object> result2 = LuaRuntime.DoString(chunk);
		result = GetTable(resultTableName);
		scriptCompleted = ScriptCompleted;
		return result2;
	}

	internal void ConfigureKernelFunctions()
	{
		try
		{
			LuaRuntime["kernel"] = new LuaTable(LuaRuntime);
			Type[] types = typeof(KernelService).Assembly.GetTypes();
			for (int i = 0; i < types.Length; i++)
			{
				MethodInfo[] methods = types[i].GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
				foreach (MethodInfo methodInfo in methods)
				{
					KernelFunctionAttribute kernelFunctionAttribute = (KernelFunctionAttribute)Attribute.GetCustomAttribute(methodInfo, typeof(KernelFunctionAttribute));
					if (kernelFunctionAttribute != null)
					{
						LuaRuntime.RegisterMethod(kernelFunctionAttribute.Name, methodInfo, compileStaticCallSite: true);
					}
				}
			}
		}
		catch (ReflectionTypeLoadException ex)
		{
			Exception[] loaderExceptions = ex.LoaderExceptions;
			LogHelper.Instance.Log("There was a ReflectionTypeLoadException in ConfigureKernelFunctions", ex);
			Exception[] array = loaderExceptions;
			foreach (Exception e in array)
			{
				LogHelper.Instance.Log("Loader Exception: ", e);
			}
		}
		catch (Exception e2)
		{
			LogHelper.Instance.Log("There was an unknown exception in ConfigureKernelFunctions", e2);
		}
	}

	private KernelService()
	{
	}
}
