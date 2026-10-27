#define TRACE
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace Redbox.Lua;

public class LuaStack
{
	public EventHandler CountChange;

	private readonly LuaVirtualMachine m_virtualMachine;

	private const string RedboxExceptionName = "(redboxException)";

	private Stack<LuaScriptException> _redboxExceptionList = new Stack<LuaScriptException>();

	public int Count => LuaVirtualMachine.lua_gettop(m_virtualMachine);

	public object Top => GetAt(-1);

	public LuaStack(LuaVirtualMachine virtualMachine)
	{
		m_virtualMachine = virtualMachine;
	}

	public object Pop()
	{
		object top = Top;
		LuaVirtualMachine.lua_pop(m_virtualMachine, 1);
		OnCountChange();
		return top;
	}

	public ReadOnlyCollection<object> PopTo(int oldTop)
	{
		List<object> list = new List<object>();
		if (oldTop != Count)
		{
			for (int i = oldTop + 1; i <= Count; i++)
			{
				list.Add(GetAt(i));
			}
			SetTop(oldTop);
			OnCountChange();
		}
		return list.AsReadOnly();
	}

	public void Drop(int count)
	{
		if (count != 0)
		{
			LuaVirtualMachine.lua_pop(m_virtualMachine, count);
			OnCountChange();
		}
	}

	public void GetReference(int reference)
	{
		LuaVirtualMachine.lua_rawgeti(m_virtualMachine, -10000, reference);
		OnCountChange();
	}

	public void NewTable()
	{
		LuaVirtualMachine.lua_newtable(m_virtualMachine);
		OnCountChange();
	}

	public void NewMetaTable(string name)
	{
		LuaVirtualMachine.luaL_newmetatable(m_virtualMachine, name);
		OnCountChange();
	}

	public IntPtr NewUserData(int size)
	{
		return LuaVirtualMachine.lua_newuserdata(m_virtualMachine, (IntPtr)size);
	}

	public IntPtr ToUserData(int index)
	{
		return LuaVirtualMachine.lua_touserdata(m_virtualMachine, index);
	}

	public void GetTable()
	{
		GetTable(-2);
	}

	public void GetTable(int index)
	{
		LuaVirtualMachine.lua_gettable(m_virtualMachine, index);
		OnCountChange();
	}

	public void SetTable()
	{
		SetTable(-3);
	}

	public void SetTable(int index)
	{
		LuaVirtualMachine.lua_settable(m_virtualMachine, index);
		OnCountChange();
	}

	public void SetTop(int index)
	{
		Trace.Assert(IsValidStackIndex(index), "not a valid stack index");
		LuaVirtualMachine.lua_settop(m_virtualMachine, index);
		OnCountChange();
	}

	public void GetGlobal(string name)
	{
		LuaVirtualMachine.lua_getglobal(m_virtualMachine, name);
		OnCountChange();
	}

	public void SetGlobal(string name)
	{
		LuaVirtualMachine.lua_setglobal(m_virtualMachine, name);
		OnCountChange();
	}

	public void GetField(int index, string name)
	{
		LuaVirtualMachine.lua_getfield(m_virtualMachine, index, name);
		OnCountChange();
	}

	public void SetField(int index, string name)
	{
		LuaVirtualMachine.lua_setfield(m_virtualMachine, index, name);
		OnCountChange();
	}

	public void SetMetaTable(int index)
	{
		LuaVirtualMachine.lua_setmetatable(m_virtualMachine, index);
		OnCountChange();
	}

	public int NextKey()
	{
		int result = LuaVirtualMachine.lua_next(m_virtualMachine, -2);
		OnCountChange();
		return result;
	}

	public int ProtectedCall(int nargs, int nresults, int errfunc)
	{
		return LuaVirtualMachine.lua_pcall(m_virtualMachine, nargs, nresults, errfunc);
	}

	public bool Check(int depth)
	{
		return LuaVirtualMachine.lua_checkstack(m_virtualMachine, depth) != 0;
	}

	public void Remove(int index)
	{
		LuaVirtualMachine.lua_remove(m_virtualMachine, index);
	}

	public void GetPath(string[] path)
	{
		for (int i = 0; i < path.Length; i++)
		{
			Push(path[i]);
			GetTable();
			if (Top == null)
			{
				break;
			}
		}
	}

	public void SetPath(string[] path, object value)
	{
		for (int i = 0; i < path.Length - 1; i++)
		{
			Push(path[i]);
			GetTable(-2);
		}
		Push(path[path.Length - 1]);
		Push(value);
		SetTable(-3);
	}

	public object GetAt(int index)
	{
		if (index < 0)
		{
			index = Count + index + 1;
		}
		Trace.Assert(IsValidStackIndex(index), "not a valid stack index");
		switch (LuaVirtualMachine.lua_type(m_virtualMachine, index))
		{
		case LuaTypes.Boolean:
			return LuaVirtualMachine.lua_toboolean(m_virtualMachine, index);
		case LuaTypes.Function:
			LuaVirtualMachine.lua_pushvalue(m_virtualMachine, index);
			return new LuaFunction(m_virtualMachine, LuaVirtualMachine.luaL_ref(m_virtualMachine, -10000));
		case LuaTypes.LightUserData:
		case LuaTypes.UserData:
			LuaVirtualMachine.lua_pushvalue(m_virtualMachine, index);
			return new LuaUserData(m_virtualMachine, LuaVirtualMachine.luaL_ref(m_virtualMachine, -10000));
		case LuaTypes.None:
		case LuaTypes.Nil:
			return null;
		case LuaTypes.Number:
			return LuaVirtualMachine.lua_tonumber(m_virtualMachine, index);
		case LuaTypes.String:
		{
			string text = LuaVirtualMachine.lua_tostring(m_virtualMachine, index);
			if (string.IsNullOrEmpty(text))
			{
				return text;
			}
			if (text == "(redboxException)")
			{
				LuaScriptException ex = _redboxExceptionList.Pop();
				if (ex != null)
				{
					return ex;
				}
			}
			return text;
		}
		case LuaTypes.Table:
			LuaVirtualMachine.lua_pushvalue(m_virtualMachine, index);
			return new LuaTable(m_virtualMachine, LuaVirtualMachine.luaL_ref(m_virtualMachine, -10000));
		default:
			return null;
		}
	}

	public void Push(object value)
	{
		if (value == null)
		{
			LuaVirtualMachine.lua_pushnil(m_virtualMachine);
		}
		else if (value is bool)
		{
			LuaVirtualMachine.lua_pushboolean(m_virtualMachine, Convert.ToBoolean(value));
		}
		else if (value is sbyte || value is byte || value is short || value is ushort || value is int || value is uint || value is long || value is float || value is ulong || value is decimal || value is double || value is char)
		{
			LuaVirtualMachine.lua_pushnumber(m_virtualMachine, Convert.ToDouble(value));
		}
		else if (value is string)
		{
			LuaVirtualMachine.lua_pushstring(m_virtualMachine, (string)value);
		}
		else if (value is LuaObject)
		{
			((LuaObject)value).Push();
		}
		else if (value is LuaScriptException)
		{
			_redboxExceptionList.Push((LuaScriptException)value);
			LuaVirtualMachine.lua_pushstring(m_virtualMachine, "(redboxException)");
		}
		OnCountChange();
	}

	public void PushClosure(LuaCFunctionDelegate function)
	{
		LuaVirtualMachine.lua_pushcclosure(m_virtualMachine, function, 0);
	}

	protected void OnCountChange()
	{
		if (CountChange != null)
		{
			CountChange(this, EventArgs.Empty);
		}
	}

	private bool IsValidStackIndex(int index)
	{
		if (index >= 0)
		{
			return index <= Count;
		}
		return false;
	}
}
