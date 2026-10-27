using System;
using System.Collections.ObjectModel;
using Redbox.Core;

namespace Redbox.Lua;

public class LuaDelegate
{
	internal Type[] m_returnTypes;

	internal LuaFunction m_function;

	public LuaDelegate()
	{
		m_function = null;
		m_returnTypes = null;
	}

	public object CallFunction(object[] args, object[] inArgs, int[] outArgs)
	{
		ReadOnlyCollection<object> readOnlyCollection = m_function.Call(inArgs);
		object result;
		int num;
		if (m_returnTypes[0] == typeof(void))
		{
			result = null;
			num = 0;
		}
		else
		{
			result = readOnlyCollection[0];
			num = 1;
		}
		for (int i = 0; i < outArgs.Length; i++)
		{
			args[outArgs[i]] = ConversionHelper.ChangeType(readOnlyCollection[num], m_returnTypes[i]);
			num++;
		}
		return result;
	}
}
