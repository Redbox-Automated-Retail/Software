using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Core;

namespace Redbox.HAL.IPC.Framework;

internal class FormMethod
{
	public readonly IDictionary<string, FormMethodParameter> ParameterCache = new Dictionary<string, FormMethodParameter>();

	private DynamicMethod m_dynamicMethod;

	private FormMethodHandler m_methodHandler;

	private List<FormMethodParameter> m_orderedParameters;

	private List<FormMethodParameter> m_requiredParameters;

	public string Usage { get; set; }

	public MethodInfo Method { get; set; }

	public string Description { get; set; }

	public List<FormMethodParameter> OrderedParameters
	{
		get
		{
			if (m_orderedParameters == null)
			{
				m_orderedParameters = new List<FormMethodParameter>(ParameterCache.Values);
				m_orderedParameters.Sort((FormMethodParameter x, FormMethodParameter y) => x.Index.CompareTo(y.Index));
			}
			return m_orderedParameters;
		}
	}

	public List<FormMethodParameter> RequiredParameters
	{
		get
		{
			if (m_requiredParameters == null)
			{
				m_requiredParameters = new List<FormMethodParameter>();
				foreach (string key in ParameterCache.Keys)
				{
					FormMethodParameter formMethodParameter = ParameterCache[key];
					if (formMethodParameter.IsRequired())
					{
						m_requiredParameters.Add(formMethodParameter);
					}
				}
			}
			return m_requiredParameters;
		}
	}

	public void Invoke(CommandResult result, CommandContext context, CommandTokenizer tokenizer, object commandInstance)
	{
		object[] array = new object[OrderedParameters.Count + 1];
		array[0] = context;
		int num = 1;
		foreach (FormMethodParameter orderedParameter in OrderedParameters)
		{
			KeyValuePair keyValuePair = tokenizer.Tokens.GetKeyValuePair(orderedParameter.KeyName);
			if (keyValuePair == null)
			{
				array[num++] = null;
				continue;
			}
			try
			{
				array[num++] = orderedParameter.ConvertValue(keyValuePair.Value);
			}
			catch (Exception e)
			{
				result.Errors.Add(Error.NewError("S999", $"Unable to convert key '{keyValuePair.Key}:' to type '{orderedParameter.Parameter.ParameterType.Name}'.", e));
			}
		}
		if (result.Errors.ContainsError())
		{
			return;
		}
		try
		{
			InvokeMethod(commandInstance, array);
		}
		catch (Exception e2)
		{
			result.Errors.Add(Error.NewError("S999", $"Execution of command form method '{commandInstance.GetType().Name}.{Method.Name}' failed.", e2));
		}
	}

	public bool ValidateParameters(CommandResult result, CommandTokenizer tokenizer)
	{
		foreach (FormMethodParameter requiredParameter in RequiredParameters)
		{
			if (tokenizer.Tokens.GetKeyValuePair(requiredParameter.KeyName) == null)
			{
				result.Errors.Add(Error.NewError("S001", $"The named parameter '{requiredParameter.KeyName.ToLower()}:' is required.", "Submit the command specifying the missing named parameter."));
			}
		}
		return !result.Errors.ContainsError();
	}

	internal void InvokeMethod(object instance, object[] values)
	{
		if (m_methodHandler == null)
		{
			m_dynamicMethod = new DynamicMethod($"___{Method.Name}", Method.ReturnType, new Type[2]
			{
				typeof(object),
				typeof(object[])
			}, instance.GetType(), skipVisibility: true);
			ILGenerator iLGenerator = m_dynamicMethod.GetILGenerator();
			if (!Method.IsStatic)
			{
				iLGenerator.Emit(OpCodes.Ldarg_0);
			}
			iLGenerator.Emit(OpCodes.Ldarg_1);
			iLGenerator.Emit(OpCodes.Ldc_I4_0);
			iLGenerator.Emit(OpCodes.Ldelem_Ref);
			for (int i = 0; i < OrderedParameters.Count; i++)
			{
				iLGenerator.Emit(OpCodes.Ldarg_1);
				iLGenerator.Emit(OpCodes.Ldc_I4, i + 1);
				iLGenerator.Emit(OpCodes.Ldelem_Ref);
				Type parameterType = OrderedParameters[i].Parameter.ParameterType;
				if (parameterType.IsValueType)
				{
					iLGenerator.Emit(OpCodes.Unbox_Any, parameterType);
				}
			}
			if (Method.IsVirtual)
			{
				iLGenerator.Emit(OpCodes.Callvirt, Method);
			}
			else
			{
				iLGenerator.Emit(OpCodes.Call, Method);
			}
			iLGenerator.Emit(OpCodes.Ret);
			m_methodHandler = (FormMethodHandler)m_dynamicMethod.CreateDelegate(typeof(FormMethodHandler));
		}
		m_methodHandler(instance, values);
	}
}
