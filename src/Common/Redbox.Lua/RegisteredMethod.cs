using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Reflection.Emit;
using Redbox.Core;

namespace Redbox.Lua;

public class RegisteredMethod
{
	internal sealed class CompiledMethodHandler
	{
		public DynamicMethod DynamicMethod;

		public RegisteredMethodHandler MethodHandler;
	}

	private sealed class ScoredMethodInfo
	{
		public int Score;

		public MethodInfo MethodInfo;
	}

	private readonly IDictionary<MethodInfo, CompiledMethodHandler> m_compiledMethodHandlers = new Dictionary<MethodInfo, CompiledMethodHandler>();

	public MethodInfo Method { get; private set; }

	public List<MethodInfo> MethodOverloads { get; private set; }

	public LuaVirtualMachine VirtualMachineInstance { get; private set; }

	public RegisteredMethod(LuaVirtualMachine virtualMachine, MethodInfo methodInfo)
	{
		VirtualMachineInstance = virtualMachine;
		Method = methodInfo;
		MethodOverloads = FindMethodOverloads(methodInfo);
	}

	public object Invoke(ReadOnlyCollection<object> parms, out int itemsOnStack)
	{
		itemsOnStack = 0;
		try
		{
			MethodInfo methodInfo = FindBestMethodOverload(parms);
			return InvokeMethod(methodInfo, ConvertParametersForMethodInfo(methodInfo, parms));
		}
		catch (TargetInvocationException ex)
		{
			itemsOnStack = VirtualMachineInstance.SetPendingException(ex.GetBaseException());
		}
		catch (Exception pendingException)
		{
			itemsOnStack = VirtualMachineInstance.SetPendingException(pendingException);
		}
		return null;
	}

	internal void Compile()
	{
		Compile(Method);
		foreach (MethodInfo methodOverload in MethodOverloads)
		{
			Compile(methodOverload);
		}
	}

	internal void Compile(MethodInfo methodInfo)
	{
		if (m_compiledMethodHandlers.ContainsKey(methodInfo))
		{
			return;
		}
		CompiledMethodHandler compiledMethodHandler = new CompiledMethodHandler();
		if (compiledMethodHandler.DynamicMethod == null)
		{
			compiledMethodHandler.DynamicMethod = new DynamicMethod($"___{methodInfo.Name}", typeof(object), new Type[1] { typeof(object[]) }, methodInfo.DeclaringType, skipVisibility: true);
			ILGenerator iLGenerator = compiledMethodHandler.DynamicMethod.GetILGenerator();
			ParameterInfo[] parameters = methodInfo.GetParameters();
			for (int i = 0; i < parameters.Length; i++)
			{
				iLGenerator.Emit(OpCodes.Ldarg_0);
				iLGenerator.Emit(OpCodes.Ldc_I4, i);
				iLGenerator.Emit(OpCodes.Ldelem_Ref);
				Type parameterType = parameters[i].ParameterType;
				if (parameterType.IsValueType)
				{
					iLGenerator.Emit(OpCodes.Unbox_Any, parameterType);
				}
			}
			if (methodInfo.IsVirtual)
			{
				iLGenerator.Emit(OpCodes.Callvirt, methodInfo);
			}
			else
			{
				iLGenerator.Emit(OpCodes.Call, methodInfo);
			}
			if (methodInfo.ReturnType != typeof(void))
			{
				if (methodInfo.ReturnType.IsValueType)
				{
					iLGenerator.Emit(OpCodes.Box, methodInfo.ReturnType);
				}
			}
			else
			{
				iLGenerator.Emit(OpCodes.Ldnull);
			}
			iLGenerator.Emit(OpCodes.Ret);
		}
		compiledMethodHandler.MethodHandler = (RegisteredMethodHandler)compiledMethodHandler.DynamicMethod.CreateDelegate(typeof(RegisteredMethodHandler));
		m_compiledMethodHandlers[methodInfo] = compiledMethodHandler;
	}

	internal object InvokeMethod(MethodInfo methodInfo, object[] values)
	{
		if (!m_compiledMethodHandlers.ContainsKey(methodInfo))
		{
			return methodInfo.Invoke(null, values);
		}
		return m_compiledMethodHandlers[methodInfo].MethodHandler(values);
	}

	private static List<MethodInfo> FindMethodOverloads(MethodInfo targetMethodInfo)
	{
		List<MethodInfo> list = new List<MethodInfo>();
		MethodInfo[] methods = targetMethodInfo.DeclaringType.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
		foreach (MethodInfo methodInfo in methods)
		{
			if (string.Compare(methodInfo.Name, targetMethodInfo.Name, ignoreCase: false) == 0 && methodInfo != targetMethodInfo)
			{
				list.Add(methodInfo);
			}
		}
		return list;
	}

	private static object[] ConvertParametersForMethodInfo(MethodInfo methodInfo, IEnumerable<object> parms)
	{
		ParameterInfo[] parameters = methodInfo.GetParameters();
		object[] array = new object[parameters.Length];
		int num = 0;
		foreach (object parm in parms)
		{
			if (typeof(Delegate).IsAssignableFrom(parameters[num].ParameterType) && parm is LuaFunction)
			{
				array[num] = DelegateFactory.GetDelegate(parameters[num].ParameterType, (LuaFunction)parm);
			}
			else
			{
				array[num] = ConversionHelper.ChangeType(parm, parameters[num].ParameterType);
			}
			num++;
			if (num >= parameters.Length)
			{
				break;
			}
		}
		return array;
	}

	private MethodInfo FindBestMethodOverload(ReadOnlyCollection<object> parms)
	{
		if (MethodOverloads.Count == 0)
		{
			return Method;
		}
		List<ScoredMethodInfo> list = new List<ScoredMethodInfo>();
		foreach (MethodInfo methodOverload in MethodOverloads)
		{
			ParameterInfo[] parameters = methodOverload.GetParameters();
			if (parms.Count != parameters.Length)
			{
				continue;
			}
			int num = 0;
			for (int i = 0; i < parameters.Length; i++)
			{
				Type type = parms[i].GetType();
				if (type == typeof(double))
				{
					if (parameters[i].ParameterType == typeof(double))
					{
						num += 5;
					}
					else if (parameters[i].ParameterType == typeof(decimal))
					{
						num += 4;
					}
					else if (parameters[i].ParameterType == typeof(float))
					{
						num += 3;
					}
					else if (parameters[i].ParameterType == typeof(long))
					{
						num += 2;
					}
					else if (parameters[i].ParameterType == typeof(int) || parameters[i].ParameterType == typeof(ushort) || parameters[i].ParameterType == typeof(short) || parameters[i].ParameterType == typeof(byte))
					{
						num++;
					}
				}
				else if (type == typeof(bool) && parameters[i].ParameterType == typeof(bool))
				{
					num++;
				}
				else if (type == typeof(string) && parameters[i].ParameterType == typeof(string))
				{
					num++;
				}
				else if (type == typeof(LuaFunction) && typeof(Delegate).IsAssignableFrom(parameters[i].ParameterType))
				{
					num++;
				}
				else if (type == typeof(LuaObject) && parameters[i].ParameterType == typeof(LuaObject))
				{
					num++;
				}
			}
			if (num > 0)
			{
				list.Add(new ScoredMethodInfo
				{
					Score = num,
					MethodInfo = methodOverload
				});
			}
		}
		list.Sort((ScoredMethodInfo x, ScoredMethodInfo y) => y.Score.CompareTo(x.Score));
		if (list.Count <= 0)
		{
			return Method;
		}
		return list[0].MethodInfo;
	}
}
