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
		CompiledMethodHandler compiledMethodHandler = m_compiledMethodHandlers[methodInfo];
		return compiledMethodHandler.MethodHandler(values);
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
		object[] converted = new object[parameters.Length];
		int index = 0;
		foreach (object parm in parms)
		{
			if (typeof(Delegate).IsAssignableFrom(parameters[index].ParameterType) && parm is LuaFunction)
			{
				converted[index] = DelegateFactory.GetDelegate(parameters[index].ParameterType, (LuaFunction)parm);
			}
			else
			{
				converted[index] = ConversionHelper.ChangeType(parm, parameters[index].ParameterType);
			}
			index++;
			if (index >= parameters.Length)
			{
				break;
			}
		}
		return converted;
	}

	private MethodInfo FindBestMethodOverload(ReadOnlyCollection<object> parms)
	{
		if (MethodOverloads.Count == 0)
		{
			return Method;
		}
		List<ScoredMethodInfo> scoredMethodInfoList = new List<ScoredMethodInfo>();
		foreach (MethodInfo methodOverload in MethodOverloads)
		{
			ParameterInfo[] parameters = methodOverload.GetParameters();
			if (parms.Count != parameters.Length)
			{
				continue;
			}
			int score = 0;
			for (int i = 0; i < parameters.Length; i++)
			{
				Type type = parms[i].GetType();
				if (type == typeof(double))
				{
					if (parameters[i].ParameterType == typeof(double))
					{
						score += 5;
					}
					else if (parameters[i].ParameterType == typeof(decimal))
					{
						score += 4;
					}
					else if (parameters[i].ParameterType == typeof(float))
					{
						score += 3;
					}
					else if (parameters[i].ParameterType == typeof(long))
					{
						score += 2;
					}
					else if (parameters[i].ParameterType == typeof(int) || parameters[i].ParameterType == typeof(ushort) || parameters[i].ParameterType == typeof(short) || parameters[i].ParameterType == typeof(byte))
					{
						score++;
					}
				}
				else if (type == typeof(bool) && parameters[i].ParameterType == typeof(bool))
				{
					score++;
				}
				else if (type == typeof(string) && parameters[i].ParameterType == typeof(string))
				{
					score++;
				}
				else if (type == typeof(LuaFunction) && typeof(Delegate).IsAssignableFrom(parameters[i].ParameterType))
				{
					score++;
				}
				else if (type == typeof(LuaObject) && parameters[i].ParameterType == typeof(LuaObject))
				{
					score++;
				}
			}
			if (score > 0)
			{
				scoredMethodInfoList.Add(new ScoredMethodInfo
				{
					Score = score,
					MethodInfo = methodOverload
				});
			}
		}
		scoredMethodInfoList.Sort((ScoredMethodInfo x, ScoredMethodInfo y) => y.Score.CompareTo(x.Score));
		if (scoredMethodInfoList.Count <= 0)
		{
			return Method;
		}
		return scoredMethodInfoList[0].MethodInfo;
	}
}
