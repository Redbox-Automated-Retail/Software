using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Runtime.InteropServices;
using Redbox.Core;

namespace Redbox.Lua;

public class LuaVirtualMachine : IDisposable
{
	internal const int LuaErrRun = 2;

	internal const int LuaErrSyntax = 3;

	internal const int LuaErrMem = 4;

	internal const int LuaMultRet = -1;

	internal const int LuaRegistryIndex = -10000;

	internal const int LuaEnvironIndex = -10001;

	internal const int LuaGlobalsIndex = -10002;

	internal const int LuaIdSize = 60;

	internal const string LuaDllName = "lua51";

	private const string RedboxLuaFlag = "__redbox_lua_flag";

	private object _lock = new object();

	private Stack<int> _removeReferenceList = new Stack<int>();

	private IntPtr m_luaState;

	private LuaHookFunctionDelegate m_hookCallback;

	private readonly LuaCFunctionDelegate m_panicCallback;

	private readonly LuaCFunctionDelegate m_collectObjectCallback;

	private readonly ObjectCache m_objectCache = new ObjectCache();

	private readonly LuaCFunctionDelegate m_executeFunctionCallback;

	public object this[string fullPath]
	{
		get
		{
			if (string.IsNullOrEmpty(fullPath))
			{
				return null;
			}
			object found = null;
			int count = Stack.Count;
			string[] pathParts = fullPath.Split(".".ToCharArray());
			Stack.GetGlobal(pathParts[0]);
			found = Stack.GetAt(-1);
			if (pathParts.Length > 1)
			{
				string[] subPath = new string[pathParts.Length - 1];
				Array.Copy(pathParts, 1, subPath, 0, pathParts.Length - 1);
				Stack.GetPath(subPath);
				found = Stack.Top;
			}
			Stack.SetTop(count);
			return found;
		}
		set
		{
			int count = Stack.Count;
			string[] pathParts = fullPath.Split(".".ToCharArray());
			if (pathParts.Length == 1)
			{
				Stack.Push(value);
				Stack.SetGlobal(fullPath);
			}
			else
			{
				Stack.GetGlobal(pathParts[0]);
				string[] subPath = new string[pathParts.Length - 1];
				Array.Copy(pathParts, 1, subPath, 0, pathParts.Length - 1);
				Stack.SetPath(subPath, value);
			}
			Stack.SetTop(count);
		}
	}

	public LuaStack Stack { get; private set; }

	public bool IsExecuting { get; internal set; }

	public LuaCompiler Compiler { get; private set; }

	public event EventHandler<DebugHookEventArgs> DebugHook;

	public event EventHandler<HookExceptionEventArgs> HookException;

	public LuaVirtualMachine()
	{
		m_luaState = luaL_newstate();
		Stack = new LuaStack(this);
		m_panicCallback = PanicCallback;
		m_collectObjectCallback = CollectObject;
		m_executeFunctionCallback = ExecuteFunction;
		luaL_openlibs(this);
		if (IsAlreadyLoaded())
		{
			throw new LuaException("Lua has already been loaded into the given state.");
		}
		SetRedboxFlag();
		CreateFunctionMetaTable();
		CreateObjectTable();
		lua_atpanic(this, m_panicCallback);
		Compiler = new LuaCompiler(this);
	}

	public static implicit operator IntPtr(LuaVirtualMachine virtualMachine)
	{
		return virtualMachine.m_luaState;
	}

	public void Dispose()
	{
		if (!(m_luaState == IntPtr.Zero))
		{
			RemoveReferences();
			lua_close(this);
			m_luaState = IntPtr.Zero;
		}
	}

	public ReadOnlyCollection<object> DoString(string chunk)
	{
		return LoadString(chunk, "__dostring__")?.Call();
	}

	public LuaFunction LoadString(string chunk, string name)
	{
		int count = Stack.Count;
		IsExecuting = true;
		try
		{
			if (luaL_loadbuffer(this, chunk, (uint)chunk.Length, name) != 0)
			{
				ThrowExceptionFromError(count);
			}
		}
		finally
		{
			IsExecuting = false;
		}
		LuaFunction result = (LuaFunction)Stack.Pop();
		Stack.SetTop(count);
		return result;
	}

	public LuaFunction LoadFile(string fileName)
	{
		int count = Stack.Count;
		IsExecuting = true;
		try
		{
			if (luaL_loadfile(this, fileName) != 0)
			{
				ThrowExceptionFromError(count);
			}
		}
		finally
		{
			IsExecuting = false;
		}
		LuaFunction result = (LuaFunction)Stack.Pop();
		Stack.SetTop(count);
		return result;
	}

	public RegisteredMethod RegisterMethod(string name, MethodInfo function, bool compileStaticCallSite, string namespacePath)
	{
		if (this[namespacePath] == null)
		{
			this[namespacePath] = new LuaTable(this);
		}
		return RegisterMethod(name, function, compileStaticCallSite);
	}

	public RegisteredMethod RegisterMethod(string name, MethodInfo function, bool compileStaticCallSite)
	{
		RegisteredMethod registeredMethod = new RegisteredMethod(this, function);
		if (compileStaticCallSite)
		{
			registeredMethod.Compile();
		}
		int objectId = m_objectCache.CacheObject(registeredMethod);
		Stack.Push(objectId);
		Stack.GetField(-10000, "kernel_objects_mt");
		Stack.SetMetaTable(-2);
		this[name] = Stack.Top;
		Stack.Pop();
		return registeredMethod;
	}

	public int GetInfo(string what, ref LuaDebug luaDebug)
	{
		return lua_getinfo(this, what, ref luaDebug);
	}

	public int SetDebugHook(LuaHookEventMasks mask, int count)
	{
		if (m_hookCallback == null)
		{
			m_hookCallback = DebugHookCallback;
			return lua_sethook(this, Marshal.GetFunctionPointerForDelegate(m_hookCallback), (int)mask, count);
		}
		return -1;
	}

	public int RemoveDebugHook()
	{
		m_hookCallback = null;
		return lua_sethook(this, IntPtr.Zero, 0, 0);
	}

	public LuaHookEventMasks GetHookMask()
	{
		return lua_gethookmask(this);
	}

	public int GetHookCount()
	{
		return lua_gethookcount(this);
	}

	public bool GetStack(int level, out LuaDebug luaDebug)
	{
		luaDebug = default(LuaDebug);
		return lua_getstack(this, level, ref luaDebug) != 0;
	}

	public string GetLocal(LuaDebug luaDebug, int n)
	{
		return lua_getlocal(this, ref luaDebug, n);
	}

	public string SetLocal(LuaDebug luaDebug, int n)
	{
		return lua_setlocal(this, ref luaDebug, n);
	}

	public string GetUpValue(int funcindex, int n)
	{
		return lua_getupvalue(this, funcindex, n);
	}

	public string SetUpValue(int funcindex, int n)
	{
		return lua_setupvalue(this, funcindex, n);
	}

	public bool CompareReference(int ref1, int ref2)
	{
		return lua_compareref(this, ref1, ref2);
	}

	public void RemoveReference(int reference)
	{
		lock (_lock)
		{
			_removeReferenceList.Push(reference);
		}
	}

	public void RemoveReferences()
	{
		lock (_lock)
		{
			if (m_luaState != IntPtr.Zero)
			{
				while (_removeReferenceList.Count > 0)
				{
					int reference = _removeReferenceList.Pop();
					luaL_unref(m_luaState, -10000, reference);
				}
			}
		}
	}

	internal static string StringPointerToString(IntPtr ptr)
	{
		return Marshal.PtrToStringAnsi(ptr);
	}

	internal static string StringPointerToString(IntPtr ptr, int len)
	{
		return Marshal.PtrToStringAnsi(ptr, len);
	}

	internal static void lua_getglobal(IntPtr luaState, string name)
	{
		lua_getfield(luaState, -10002, name);
	}

	internal static void lua_pop(IntPtr luaState, int n)
	{
		lua_settop(luaState, -n - 1);
	}

	internal static void lua_setglobal(IntPtr luaState, string name)
	{
		lua_setfield(luaState, -10002, name);
	}

	internal static void lua_newtable(IntPtr luaState)
	{
		lua_createtable(luaState, 0, 0);
	}

	internal static string lua_tostring(IntPtr luaState, int index)
	{
		IntPtr ptr = lua_tolstring(luaState, index, out var len);
		return StringPointerToString(ptr, len.ToInt32());
	}

	internal static bool lua_compareref(IntPtr luaState, int ref1, int ref2)
	{
		int newTop = lua_gettop(luaState);
		lua_rawgeti(luaState, -10000, ref1);
		lua_rawgeti(luaState, -10000, ref2);
		int equal = lua_equal(luaState, -1, -2);
		lua_settop(luaState, newTop);
		return equal != 0;
	}

	[DllImport("lua51")]
	internal static extern void lua_atpanic(IntPtr luaState, LuaCFunctionDelegate panicf);

	[DllImport("lua51")]
	internal static extern void lua_close(IntPtr luaState);

	[DllImport("lua51")]
	internal static extern void lua_createtable(IntPtr luaState, int narr, int nrec);

	[DllImport("lua51")]
	internal static extern void lua_call(IntPtr luaState, int nargs, int nresults);

	[DllImport("lua51")]
	internal static extern int lua_error(IntPtr luaState);

	[DllImport("lua51", CharSet = CharSet.Ansi)]
	internal static extern void lua_getfield(IntPtr luaState, int index, string k);

	[DllImport("lua51")]
	internal static extern void lua_gettable(IntPtr luaState, int index);

	[DllImport("lua51")]
	internal static extern int lua_gettop(IntPtr luaState);

	[DllImport("lua51")]
	internal static extern int lua_next(IntPtr luaState, int index);

	[DllImport("lua51")]
	internal static extern int lua_pcall(IntPtr luaState, int nargs, int nresults, int errfunc);

	[DllImport("lua51")]
	internal static extern void lua_pushboolean(IntPtr luaState, bool b);

	[DllImport("lua51")]
	internal static extern void lua_pushcclosure(IntPtr luaState, LuaCFunctionDelegate f, int n);

	[DllImport("lua51", CharSet = CharSet.Ansi)]
	internal static extern void lua_pushlstring(IntPtr luaState, string s, IntPtr len);

	[DllImport("lua51", CharSet = CharSet.Ansi)]
	internal static extern void lua_pushstring(IntPtr luaState, string s);

	[DllImport("lua51")]
	internal static extern void lua_pushnil(IntPtr luaState);

	[DllImport("lua51")]
	internal static extern void lua_pushnumber(IntPtr luaState, double n);

	[DllImport("lua51")]
	internal static extern void lua_pushvalue(IntPtr luaState, int index);

	[DllImport("lua51")]
	internal static extern void lua_settop(IntPtr luaState, int newTop);

	[DllImport("lua51", CharSet = CharSet.Ansi)]
	internal static extern void lua_setfield(IntPtr luaState, int index, string k);

	[DllImport("lua51")]
	internal static extern void lua_settable(IntPtr luaState, int index);

	[DllImport("lua51")]
	internal static extern bool lua_toboolean(IntPtr luaState, int index);

	[DllImport("lua51")]
	internal static extern IntPtr lua_tolstring(IntPtr luaState, int index, out IntPtr len);

	[DllImport("lua51")]
	internal static extern double lua_tonumber(IntPtr luaState, int index);

	[DllImport("lua51")]
	internal static extern IntPtr lua_touserdata(IntPtr luaState, int index);

	[DllImport("lua51")]
	internal static extern LuaTypes lua_type(IntPtr luaState, int index);

	[DllImport("lua51")]
	internal static extern IntPtr lua_typename(IntPtr luaState, LuaTypes type);

	[DllImport("lua51", CharSet = CharSet.Ansi)]
	internal static extern int luaL_loadbuffer(IntPtr luaState, string buff, uint sz, string name);

	[DllImport("lua51", CharSet = CharSet.Ansi)]
	internal static extern int luaL_loadfile(IntPtr luaState, string fileName);

	[DllImport("lua51")]
	internal static extern IntPtr luaL_newstate();

	[DllImport("lua51")]
	internal static extern void luaL_openlibs(IntPtr luaState);

	[DllImport("lua51")]
	internal static extern IntPtr lua_gethook(IntPtr luaState);

	[DllImport("lua51")]
	internal static extern LuaHookEventMasks lua_gethookmask(IntPtr luaState);

	[DllImport("lua51")]
	internal static extern int lua_gethookcount(IntPtr luaState);

	[DllImport("lua51", CharSet = CharSet.Ansi)]
	internal static extern int lua_getinfo(IntPtr luaState, string what, ref LuaDebug luaDebug);

	[DllImport("lua51")]
	internal static extern string lua_getlocal(IntPtr luaState, ref LuaDebug luaDebug, int n);

	[DllImport("lua51")]
	internal static extern string lua_setlocal(IntPtr luaState, ref LuaDebug luaDebug, int n);

	[DllImport("lua51")]
	internal static extern int lua_getstack(IntPtr luaState, int level, ref LuaDebug luaDebug);

	[DllImport("lua51")]
	internal static extern int lua_sethook(IntPtr luaState, IntPtr f, int mask, int count);

	[DllImport("lua51")]
	internal static extern int luaopen_base(IntPtr luaState);

	[DllImport("lua51")]
	internal static extern int luaopen_math(IntPtr luaState);

	[DllImport("lua51", CharSet = CharSet.Ansi)]
	internal static extern string lua_getupvalue(IntPtr luaState, int funcindex, int n);

	[DllImport("lua51", CharSet = CharSet.Ansi)]
	internal static extern string lua_setupvalue(IntPtr luaState, int funcindex, int n);

	[DllImport("lua51")]
	internal static extern void lua_rawgeti(IntPtr luaState, int tableIndex, int index);

	[DllImport("lua51")]
	internal static extern void lua_rawseti(IntPtr luaState, int tableIndex, int index);

	[DllImport("lua51")]
	internal static extern int luaL_ref(IntPtr luaState, int index);

	[DllImport("lua51")]
	internal static extern void luaL_unref(IntPtr luaState, int index, int reference);

	[DllImport("lua51", CharSet = CharSet.Ansi)]
	internal static extern void luaL_newmetatable(IntPtr luaState, string name);

	[DllImport("lua51")]
	internal static extern void lua_setmetatable(IntPtr luaState, int index);

	[DllImport("lua51")]
	internal static extern IntPtr lua_newuserdata(IntPtr luaState, IntPtr size);

	[DllImport("lua51")]
	internal static extern void lua_remove(IntPtr luaState, int index);

	[DllImport("lua51")]
	internal static extern int lua_equal(IntPtr luaState, int index1, int index2);

	[DllImport("lua51")]
	internal static extern int lua_checkstack(IntPtr luaState, int extra);

	[DllImport("lua51")]
	internal static extern int luaL_where(IntPtr luaState, int level);

	internal void ThrowExceptionFromError(int oldTop)
	{
		object error = Stack.Pop();
		Stack.SetTop(oldTop);
		if (error is LuaScriptException ex)
		{
			throw ex;
		}
		if (error == null)
		{
			error = "Unknown Lua Error";
		}
		throw new LuaScriptException(error.ToString(), string.Empty);
	}

	internal int SetPendingException(Exception e)
	{
		if (e == null)
		{
			return 0;
		}
		ThrowError(e);
		return 1;
	}

	internal void ThrowError(object e)
	{
		int count = Stack.Count;
		luaL_where(this, 1);
		ReadOnlyCollection<object> readOnlyCollection = Stack.PopTo(count);
		string source = string.Empty;
		if (readOnlyCollection.Count > 0)
		{
			source = readOnlyCollection[0].ToString();
		}
		if (e is string message)
		{
			e = new LuaScriptException(message, source);
		}
		else if (e is Exception innerException)
		{
			e = new LuaScriptException(innerException, source);
		}
		Stack.Push(e);
		lua_error(this);
	}

	private static int PanicCallback(IntPtr luaState)
	{
		string message = $"unprotected error in call to Lua API ({lua_tostring(luaState, -1)})";
		throw new LuaException(message);
	}

	private void DebugHookCallback(IntPtr luaState, ref LuaDebug luaDebug)
	{
		try
		{
			if (DebugHook != null)
			{
				LuaDebugInfo luaDebugInfo = new LuaDebugInfo(luaDebug);
				DebugHook(this, new DebugHookEventArgs(luaDebugInfo));
			}
		}
		catch (Exception ex)
		{
			OnHookException(new HookExceptionEventArgs(ex));
		}
	}

	private void OnHookException(HookExceptionEventArgs e)
	{
		HookException?.Invoke(this, e);
	}

	private bool IsAlreadyLoaded()
	{
		Stack.Push("__redbox_lua_flag");
		Stack.GetTable(-10000);
		object loadedFlag = Stack.Pop();
		if (loadedFlag != null)
		{
			return (bool)loadedFlag;
		}
		return false;
	}

	private void SetRedboxFlag()
	{
		Stack.Push("__redbox_lua_flag");
		Stack.Push(true);
		Stack.SetTable(-10000);
	}

	private void CreateFunctionMetaTable()
	{
		int count = Stack.Count;
		Stack.NewMetaTable("kernel_objects_mt");
		Stack.Push("__gc");
		Stack.PushClosure(m_collectObjectCallback);
		Stack.SetTable(-3);
		Stack.Push("__call");
		Stack.PushClosure(m_executeFunctionCallback);
		Stack.SetTable(-3);
		Stack.SetTop(count);
	}

	private void CreateObjectTable()
	{
		Stack.NewTable();
		Stack.Push("__mode");
		Stack.Push("v");
		Stack.SetTable(-3);
		Stack.Push("kernel_objects");
		Stack.SetTable(-10000);
	}

	private int CollectObject(IntPtr luaState)
	{
		using LuaUserData luaUserData = (LuaUserData)Stack.Pop();
		int objectId = luaUserData.ReadInt32(0);
		LogHelper.Instance.Log($"Lua: CollectObject called from runtime for object {objectId}.", LogEntryType.Debug);
		m_objectCache.RemoveId(objectId);
		return 0;
	}

	private int ExecuteFunction(IntPtr luaState)
	{
		object at = Stack.GetAt(1);
		int objectId = Convert.ToInt32(at);
		LogHelper.Instance.Log($"Lua: ExecuteFunction called from runtime for object {objectId}.", LogEntryType.Debug);
		RegisteredMethod registeredMethod = (RegisteredMethod)m_objectCache.GetObject(objectId);
		if (registeredMethod != null)
		{
			LogHelper.Instance.Log($"...Registered method '{registeredMethod.Method}' found in object cache; executing.", LogEntryType.Debug);
			ReadOnlyCollection<object> parms = Stack.PopTo(1);
			int itemsOnStack = 0;
			object value = registeredMethod.Invoke(parms, out itemsOnStack);
			if (itemsOnStack > 0)
			{
				return itemsOnStack;
			}
			if (registeredMethod.Method.ReturnType != typeof(void))
			{
				Stack.Push(value);
				return 1;
			}
		}
		RemoveReferences();
		return 0;
	}
}
