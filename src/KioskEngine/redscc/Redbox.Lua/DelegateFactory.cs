using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading;

namespace Redbox.Lua;

public static class DelegateFactory
{
	private static int m_luaClassNumber;

	private static readonly ModuleBuilder m_newModule;

	private static readonly AssemblyName m_assemblyName;

	private static readonly AssemblyBuilder m_newAssembly;

	private static readonly IDictionary<Type, Type> m_delegateCollection;

	public static Delegate GetDelegate(Type delegateType, LuaFunction luaFunc)
	{
		List<Type> list = new List<Type>();
		Type type;
		if (m_delegateCollection.ContainsKey(delegateType))
		{
			type = m_delegateCollection[delegateType];
		}
		else
		{
			type = GenerateDelegate(delegateType);
			m_delegateCollection[delegateType] = type;
		}
		MethodInfo method = delegateType.GetMethod("Invoke");
		list.Add(method.ReturnType);
		ParameterInfo[] parameters = method.GetParameters();
		foreach (ParameterInfo parameterInfo in parameters)
		{
			if (parameterInfo.ParameterType.IsByRef)
			{
				list.Add(parameterInfo.ParameterType);
			}
		}
		LuaDelegate luaDelegate = (LuaDelegate)Activator.CreateInstance(type);
		luaDelegate.m_function = luaFunc;
		luaDelegate.m_returnTypes = list.ToArray();
		return Delegate.CreateDelegate(delegateType, luaDelegate, "CallFunction");
	}

	static DelegateFactory()
	{
		m_luaClassNumber = 1;
		m_delegateCollection = new Dictionary<Type, Type>();
		m_assemblyName = new AssemblyName
		{
			Name = "Redbox.Lua.GeneratedCode"
		};
		m_newAssembly = Thread.GetDomain().DefineDynamicAssembly(m_assemblyName, AssemblyBuilderAccess.Run);
		m_newModule = m_newAssembly.DefineDynamicModule("Redbox.Lua.GeneratedCode.Module");
	}

	private static Type GenerateDelegate(Type delegateType)
	{
		string name;
		lock (typeof(DelegateFactory))
		{
			name = "__DelegateWrapper_" + m_luaClassNumber;
			m_luaClassNumber++;
		}
		TypeBuilder typeBuilder = m_newModule.DefineType(name, TypeAttributes.Public, typeof(LuaDelegate));
		MethodInfo method = delegateType.GetMethod("Invoke");
		ParameterInfo[] parameters = method.GetParameters();
		Type[] array = new Type[parameters.Length];
		Type returnType = method.ReturnType;
		int num = 0;
		int num2 = 0;
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = parameters[i].ParameterType;
			if (!parameters[i].IsIn && parameters[i].IsOut)
			{
				num++;
			}
			if (array[i].IsByRef)
			{
				num2++;
			}
		}
		int[] array2 = new int[num2];
		MethodBuilder methodBuilder = typeBuilder.DefineMethod("CallFunction", method.Attributes, returnType, array);
		ILGenerator iLGenerator = methodBuilder.GetILGenerator();
		iLGenerator.DeclareLocal(typeof(object[]));
		iLGenerator.DeclareLocal(typeof(object[]));
		iLGenerator.DeclareLocal(typeof(int[]));
		if (returnType != typeof(void))
		{
			iLGenerator.DeclareLocal(returnType);
		}
		else
		{
			iLGenerator.DeclareLocal(typeof(object));
		}
		iLGenerator.Emit(OpCodes.Ldc_I4, array.Length);
		iLGenerator.Emit(OpCodes.Newarr, typeof(object));
		iLGenerator.Emit(OpCodes.Stloc_0);
		iLGenerator.Emit(OpCodes.Ldc_I4, array.Length - num);
		iLGenerator.Emit(OpCodes.Newarr, typeof(object));
		iLGenerator.Emit(OpCodes.Stloc_1);
		iLGenerator.Emit(OpCodes.Ldc_I4, num2);
		iLGenerator.Emit(OpCodes.Newarr, typeof(int));
		iLGenerator.Emit(OpCodes.Stloc_2);
		int j = 0;
		int num3 = 0;
		int num4 = 0;
		for (; j < array.Length; j++)
		{
			iLGenerator.Emit(OpCodes.Ldloc_0);
			iLGenerator.Emit(OpCodes.Ldc_I4, j);
			iLGenerator.Emit(OpCodes.Ldarg, j + 1);
			if (array[j].IsByRef)
			{
				if (array[j].GetElementType().IsValueType)
				{
					iLGenerator.Emit(OpCodes.Ldobj, array[j].GetElementType());
					iLGenerator.Emit(OpCodes.Box, array[j].GetElementType());
				}
				else
				{
					iLGenerator.Emit(OpCodes.Ldind_Ref);
				}
			}
			else if (array[j].IsValueType)
			{
				iLGenerator.Emit(OpCodes.Box, array[j]);
			}
			iLGenerator.Emit(OpCodes.Stelem_Ref);
			if (array[j].IsByRef)
			{
				iLGenerator.Emit(OpCodes.Ldloc_2);
				iLGenerator.Emit(OpCodes.Ldc_I4, num4);
				iLGenerator.Emit(OpCodes.Ldc_I4, j);
				iLGenerator.Emit(OpCodes.Stelem_I4);
				array2[num4] = j;
				num4++;
			}
			if (!parameters[j].IsIn && parameters[j].IsOut)
			{
				continue;
			}
			iLGenerator.Emit(OpCodes.Ldloc_1);
			iLGenerator.Emit(OpCodes.Ldc_I4, num3);
			iLGenerator.Emit(OpCodes.Ldarg, j + 1);
			if (array[j].IsByRef)
			{
				if (array[j].GetElementType().IsValueType)
				{
					iLGenerator.Emit(OpCodes.Ldobj, array[j].GetElementType());
					iLGenerator.Emit(OpCodes.Box, array[j].GetElementType());
				}
				else
				{
					iLGenerator.Emit(OpCodes.Ldind_Ref);
				}
			}
			else if (array[j].IsValueType)
			{
				iLGenerator.Emit(OpCodes.Box, array[j]);
			}
			iLGenerator.Emit(OpCodes.Stelem_Ref);
			num3++;
		}
		iLGenerator.Emit(OpCodes.Ldarg_0);
		iLGenerator.Emit(OpCodes.Ldloc_0);
		iLGenerator.Emit(OpCodes.Ldloc_1);
		iLGenerator.Emit(OpCodes.Ldloc_2);
		MethodInfo method2 = typeof(LuaDelegate).GetMethod("CallFunction");
		iLGenerator.Emit(OpCodes.Call, method2);
		if (returnType == typeof(void))
		{
			iLGenerator.Emit(OpCodes.Pop);
			iLGenerator.Emit(OpCodes.Ldnull);
		}
		else if (returnType.IsValueType)
		{
			iLGenerator.Emit(OpCodes.Unbox, returnType);
			iLGenerator.Emit(OpCodes.Ldobj, returnType);
		}
		else
		{
			iLGenerator.Emit(OpCodes.Castclass, returnType);
		}
		iLGenerator.Emit(OpCodes.Stloc_3);
		for (int k = 0; k < array2.Length; k++)
		{
			iLGenerator.Emit(OpCodes.Ldarg, array2[k] + 1);
			iLGenerator.Emit(OpCodes.Ldloc_0);
			iLGenerator.Emit(OpCodes.Ldc_I4, array2[k]);
			iLGenerator.Emit(OpCodes.Ldelem_Ref);
			if (array[array2[k]].GetElementType().IsValueType)
			{
				iLGenerator.Emit(OpCodes.Unbox, array[array2[k]].GetElementType());
				iLGenerator.Emit(OpCodes.Ldobj, array[array2[k]].GetElementType());
				iLGenerator.Emit(OpCodes.Stobj, array[array2[k]].GetElementType());
			}
			else
			{
				iLGenerator.Emit(OpCodes.Castclass, array[array2[k]].GetElementType());
				iLGenerator.Emit(OpCodes.Stind_Ref);
			}
		}
		if (returnType != typeof(void))
		{
			iLGenerator.Emit(OpCodes.Ldloc_3);
		}
		iLGenerator.Emit(OpCodes.Ret);
		return typeBuilder.CreateType();
	}
}
