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
		List<Type> returnTypes = new List<Type>();
		Type wrapperType;
		if (m_delegateCollection.ContainsKey(delegateType))
		{
			wrapperType = m_delegateCollection[delegateType];
		}
		else
		{
			wrapperType = GenerateDelegate(delegateType);
			m_delegateCollection[delegateType] = wrapperType;
		}
		MethodInfo method = delegateType.GetMethod("Invoke");
		returnTypes.Add(method.ReturnType);
		ParameterInfo[] parameters = method.GetParameters();
		foreach (ParameterInfo parameterInfo in parameters)
		{
			if (parameterInfo.ParameterType.IsByRef)
			{
				returnTypes.Add(parameterInfo.ParameterType);
			}
		}
		LuaDelegate luaDelegate = (LuaDelegate)Activator.CreateInstance(wrapperType);
		luaDelegate.m_function = luaFunc;
		luaDelegate.m_returnTypes = returnTypes.ToArray();
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
		m_newAssembly = AssemblyBuilder.DefineDynamicAssembly(m_assemblyName, AssemblyBuilderAccess.Run);
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
		Type[] paramTypes = new Type[parameters.Length];
		Type returnType = method.ReturnType;
		int nOutParams = 0;
		int nOutAndRefParams = 0;
		for (int i = 0; i < paramTypes.Length; i++)
		{
			paramTypes[i] = parameters[i].ParameterType;
			if (!parameters[i].IsIn && parameters[i].IsOut)
			{
				nOutParams++;
			}
			if (paramTypes[i].IsByRef)
			{
				nOutAndRefParams++;
			}
		}
		int[] refArgs = new int[nOutAndRefParams];
		MethodBuilder methodBuilder = typeBuilder.DefineMethod("CallFunction", method.Attributes, returnType, paramTypes);
		ILGenerator generator = methodBuilder.GetILGenerator();
		generator.DeclareLocal(typeof(object[]));
		generator.DeclareLocal(typeof(object[]));
		generator.DeclareLocal(typeof(int[]));
		if (returnType != typeof(void))
		{
			generator.DeclareLocal(returnType);
		}
		else
		{
			generator.DeclareLocal(typeof(object));
		}
		generator.Emit(OpCodes.Ldc_I4, paramTypes.Length);
		generator.Emit(OpCodes.Newarr, typeof(object));
		generator.Emit(OpCodes.Stloc_0);
		generator.Emit(OpCodes.Ldc_I4, paramTypes.Length - nOutParams);
		generator.Emit(OpCodes.Newarr, typeof(object));
		generator.Emit(OpCodes.Stloc_1);
		generator.Emit(OpCodes.Ldc_I4, nOutAndRefParams);
		generator.Emit(OpCodes.Newarr, typeof(int));
		generator.Emit(OpCodes.Stloc_2);
		int j = 0;
		int argIndex = 0;
		int refArgIndex = 0;
		for (; j < paramTypes.Length; j++)
		{
			generator.Emit(OpCodes.Ldloc_0);
			generator.Emit(OpCodes.Ldc_I4, j);
			generator.Emit(OpCodes.Ldarg, j + 1);
			if (paramTypes[j].IsByRef)
			{
				if (paramTypes[j].GetElementType().IsValueType)
				{
					generator.Emit(OpCodes.Ldobj, paramTypes[j].GetElementType());
					generator.Emit(OpCodes.Box, paramTypes[j].GetElementType());
				}
				else
				{
					generator.Emit(OpCodes.Ldind_Ref);
				}
			}
			else if (paramTypes[j].IsValueType)
			{
				generator.Emit(OpCodes.Box, paramTypes[j]);
			}
			generator.Emit(OpCodes.Stelem_Ref);
			if (paramTypes[j].IsByRef)
			{
				generator.Emit(OpCodes.Ldloc_2);
				generator.Emit(OpCodes.Ldc_I4, refArgIndex);
				generator.Emit(OpCodes.Ldc_I4, j);
				generator.Emit(OpCodes.Stelem_I4);
				refArgs[refArgIndex] = j;
				refArgIndex++;
			}
			if (!parameters[j].IsIn && parameters[j].IsOut)
			{
				continue;
			}
			generator.Emit(OpCodes.Ldloc_1);
			generator.Emit(OpCodes.Ldc_I4, argIndex);
			generator.Emit(OpCodes.Ldarg, j + 1);
			if (paramTypes[j].IsByRef)
			{
				if (paramTypes[j].GetElementType().IsValueType)
				{
					generator.Emit(OpCodes.Ldobj, paramTypes[j].GetElementType());
					generator.Emit(OpCodes.Box, paramTypes[j].GetElementType());
				}
				else
				{
					generator.Emit(OpCodes.Ldind_Ref);
				}
			}
			else if (paramTypes[j].IsValueType)
			{
				generator.Emit(OpCodes.Box, paramTypes[j]);
			}
			generator.Emit(OpCodes.Stelem_Ref);
			argIndex++;
		}
		generator.Emit(OpCodes.Ldarg_0);
		generator.Emit(OpCodes.Ldloc_0);
		generator.Emit(OpCodes.Ldloc_1);
		generator.Emit(OpCodes.Ldloc_2);
		MethodInfo method2 = typeof(LuaDelegate).GetMethod("CallFunction");
		generator.Emit(OpCodes.Call, method2);
		if (returnType == typeof(void))
		{
			generator.Emit(OpCodes.Pop);
			generator.Emit(OpCodes.Ldnull);
		}
		else if (returnType.IsValueType)
		{
			generator.Emit(OpCodes.Unbox, returnType);
			generator.Emit(OpCodes.Ldobj, returnType);
		}
		else
		{
			generator.Emit(OpCodes.Castclass, returnType);
		}
		generator.Emit(OpCodes.Stloc_3);
		for (int k = 0; k < refArgs.Length; k++)
		{
			generator.Emit(OpCodes.Ldarg, refArgs[k] + 1);
			generator.Emit(OpCodes.Ldloc_0);
			generator.Emit(OpCodes.Ldc_I4, refArgs[k]);
			generator.Emit(OpCodes.Ldelem_Ref);
			if (paramTypes[refArgs[k]].GetElementType().IsValueType)
			{
				generator.Emit(OpCodes.Unbox, paramTypes[refArgs[k]].GetElementType());
				generator.Emit(OpCodes.Ldobj, paramTypes[refArgs[k]].GetElementType());
				generator.Emit(OpCodes.Stobj, paramTypes[refArgs[k]].GetElementType());
			}
			else
			{
				generator.Emit(OpCodes.Castclass, paramTypes[refArgs[k]].GetElementType());
				generator.Emit(OpCodes.Stind_Ref);
			}
		}
		if (returnType != typeof(void))
		{
			generator.Emit(OpCodes.Ldloc_3);
		}
		generator.Emit(OpCodes.Ret);
		return typeBuilder.CreateType();
	}
}
