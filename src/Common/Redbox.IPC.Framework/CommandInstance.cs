using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using Redbox.Command.Tokenizer;
using Redbox.Core;

namespace Redbox.IPC.Framework;

internal class CommandInstance
{
	private List<string> _filters = new List<string>();

	public readonly IDictionary<string, FormMethod> FormMethodCache = new Dictionary<string, FormMethod>();

	public Type CommandType { get; set; }

	public string CommandDescription { get; set; }

	public List<string> Filters => _filters;

	public static CommandInstance GetCommandInstance(Type commandType)
	{
		CommandInstance commandInstance = new CommandInstance
		{
			CommandType = commandType
		};
		if (Attribute.GetCustomAttribute(commandType, typeof(DescriptionAttribute)) is DescriptionAttribute descriptionAttribute)
		{
			commandInstance.CommandDescription = descriptionAttribute.Description;
		}
		MethodInfo[] methods = commandType.GetMethods();
		foreach (MethodInfo methodInfo in methods)
		{
			NotLoggableAttribute notLoggableAttribute = Attribute.GetCustomAttribute(methodInfo, typeof(NotLoggableAttribute)) as NotLoggableAttribute;
			if (!(Attribute.GetCustomAttribute(methodInfo, typeof(CommandFormAttribute)) is CommandFormAttribute commandFormAttribute))
			{
				continue;
			}
			ParameterInfo[] parameters = methodInfo.GetParameters();
			if (parameters.Length == 0 || parameters[0].ParameterType != typeof(CommandContext))
			{
				continue;
			}
			UsageAttribute usageAttribute = Attribute.GetCustomAttribute(methodInfo, typeof(UsageAttribute)) as UsageAttribute;
			DescriptionAttribute descriptionAttribute2 = Attribute.GetCustomAttribute(methodInfo, typeof(DescriptionAttribute)) as DescriptionAttribute;
			FormMethod formMethod = new FormMethod
			{
				Method = methodInfo,
				Usage = ((usageAttribute != null) ? usageAttribute.Template : string.Empty),
				Description = ((descriptionAttribute2 != null) ? descriptionAttribute2.Description : string.Empty),
				Loggable = (notLoggableAttribute == null)
			};
			if (!string.IsNullOrEmpty(commandFormAttribute.Filter))
			{
				commandFormAttribute.Filter.Trim().Split('|').ToList()
					.ForEach(formMethod.Filters.Add);
			}
			int num = 0;
			ParameterInfo[] array = parameters;
			foreach (ParameterInfo parameterInfo in array)
			{
				if (parameterInfo.ParameterType == typeof(CommandContext))
				{
					continue;
				}
				FormMethodParameter formMethodParameter = new FormMethodParameter
				{
					Parameter = parameterInfo,
					Index = num++
				};
				CommandKeyValueAttribute commandKeyValueAttribute = Attribute.GetCustomAttribute(parameterInfo, typeof(CommandKeyValueAttribute)) as CommandKeyValueAttribute;
				formMethodParameter.KeyName = parameterInfo.Name.ToUpper();
				if (commandKeyValueAttribute != null)
				{
					formMethodParameter.MetaData = commandKeyValueAttribute;
					if (commandKeyValueAttribute.KeyName != null)
					{
						formMethodParameter.KeyName = commandKeyValueAttribute.KeyName.ToUpper();
					}
				}
				formMethod.ParameterCache[formMethodParameter.KeyName] = formMethodParameter;
			}
			formMethod.Compile(commandInstance.CommandType);
			commandInstance.FormMethodCache[commandFormAttribute.Name.ToUpper()] = formMethod;
		}
		MethodInfo method = commandType.GetMethod("Default", new Type[1] { typeof(CommandContext) });
		if (method != null)
		{
			FormMethod formMethod2 = new FormMethod
			{
				Method = method
			};
			formMethod2.Compile(commandInstance.CommandType);
			commandInstance.FormMethodCache["Default"] = formMethod2;
		}
		return commandInstance;
	}

	public object GetInstance()
	{
		return Activator.CreateInstance(CommandType);
	}

	public bool HasDefault()
	{
		return FormMethodCache.ContainsKey("Default");
	}

	public bool HasOnlyDefault()
	{
		if (FormMethodCache.Count == 1)
		{
			return HasDefault();
		}
		return false;
	}

	public void InvokeDefault(CommandResult result, CommandContext context, CommandTokenizer tokenizer, bool enable, List<string> sourceFilters)
	{
		if (HasDefault())
		{
			FormMethodCache["Default"].Invoke(result, context, tokenizer, GetInstance(), enable, sourceFilters);
		}
	}

	public FormMethod GetMethod(string formName)
	{
		if (string.IsNullOrEmpty(formName))
		{
			return null;
		}
		string key = formName.ToUpper();
		if (FormMethodCache.ContainsKey(key))
		{
			return FormMethodCache[key];
		}
		return null;
	}

	public bool IsInFilter(bool enable, List<string> sourceFilters)
	{
		if (!enable)
		{
			return true;
		}
		if (sourceFilters == null || sourceFilters.Count == 0)
		{
			return false;
		}
		if (_filters.Count == 0)
		{
			return false;
		}
		return sourceFilters.Intersect(_filters, StringComparer.CurrentCultureIgnoreCase).Count() > 0;
	}
}
