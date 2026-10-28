using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using Redbox.HAL.Core;
using Redbox.IPC.Framework;

namespace Redbox.HAL.IPC.Framework;

[Command("command-repository")]
[Description("The COMMAND-REPOSITORY command scans the execution directory of the cache service for .NET assembly files (.dll and .exe) that contain types marked with the CommandAttribute class.")]
public class CommandRepository
{
	private static readonly IDictionary<string, Type> m_commands = new Dictionary<string, Type>();

	private static readonly IDictionary<Type, CommandInstance> m_instanceCache = new Dictionary<Type, CommandInstance>();

	internal static ICollection<string> AllCommands => m_commands.Keys;

	public void Default(CommandContext context)
	{
		List<string> list = DiscoverInstalledCommands();
		foreach (string item in list)
		{
			context.Messages.Add("Installed Command '" + item + "'.");
		}
	}

	internal static List<string> DiscoverInstalledCommands()
	{
		using ExecutionTimer executionTimer = new ExecutionTimer();
		List<string> list = new List<string>();
		string directoryName = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
		List<string> list2 = new List<string>();
		list2.AddRange(Directory.GetFiles(directoryName, "*.dll"));
		foreach (string item in list2)
		{
			try
			{
				Assembly assembly = Assembly.LoadFrom(item);
				list.AddRange(FindCommands(assembly.GetTypes()));
			}
			catch (BadImageFormatException)
			{
			}
			catch (Exception e)
			{
				LogHelper.Instance.Log($"Unable to load assembly '{item}' to scan for commands.", e);
			}
		}
		LogHelper.Instance.Log($"Time to scan for commands: {executionTimer.Elapsed}");
		return list;
	}

	internal static CommandInstance GetCommand(string name)
	{
		if (string.IsNullOrEmpty(name))
		{
			return null;
		}
		if (!m_commands.TryGetValue(name.ToUpper(), out var value))
		{
			return null;
		}
		if (!m_instanceCache.TryGetValue(value, out var value2))
		{
			CommandInstance commandInstance = new CommandInstance();
			commandInstance.CommandType = value;
			value2 = commandInstance;
			if (Attribute.GetCustomAttribute(value, typeof(DescriptionAttribute)) is DescriptionAttribute descriptionAttribute)
			{
				value2.CommandDescription = descriptionAttribute.Description;
			}
			MethodInfo[] methods = value.GetMethods();
			foreach (MethodInfo methodInfo in methods)
			{
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
				FormMethod formMethod = new FormMethod();
				formMethod.Method = methodInfo;
				formMethod.Usage = ((usageAttribute != null) ? usageAttribute.Template : string.Empty);
				formMethod.Description = ((descriptionAttribute2 != null) ? descriptionAttribute2.Description : string.Empty);
				FormMethod formMethod2 = formMethod;
				int num = 0;
				ParameterInfo[] array = parameters;
				foreach (ParameterInfo parameterInfo in array)
				{
					if (parameterInfo.ParameterType == typeof(CommandContext))
					{
						continue;
					}
					FormMethodParameter formMethodParameter = new FormMethodParameter();
					formMethodParameter.Parameter = parameterInfo;
					formMethodParameter.Index = num++;
					FormMethodParameter formMethodParameter2 = formMethodParameter;
					CommandKeyValueAttribute commandKeyValueAttribute = Attribute.GetCustomAttribute(parameterInfo, typeof(CommandKeyValueAttribute)) as CommandKeyValueAttribute;
					formMethodParameter2.KeyName = parameterInfo.Name.ToUpper();
					if (commandKeyValueAttribute != null)
					{
						formMethodParameter2.MetaData = commandKeyValueAttribute;
						if (commandKeyValueAttribute.KeyName != null)
						{
							formMethodParameter2.KeyName = commandKeyValueAttribute.KeyName.ToUpper();
						}
						formMethod2.ParameterCache[formMethodParameter2.KeyName] = formMethodParameter2;
					}
					else
					{
						formMethod2.ParameterCache[formMethodParameter2.KeyName] = formMethodParameter2;
					}
				}
				value2.FormMethodCache[commandFormAttribute.Name.ToUpper()] = formMethod2;
			}
			MethodInfo method = value.GetMethod("Default", new Type[1] { typeof(CommandContext) });
			if (method != null)
			{
				value2.FormMethodCache["Default"] = new FormMethod
				{
					Method = method
				};
			}
			m_instanceCache[value] = value2;
		}
		return value2;
	}

	private static List<string> FindCommands(IEnumerable<Type> types)
	{
		List<string> list = new List<string>();
		foreach (Type type in types)
		{
			CommandAttribute[] array = (CommandAttribute[])type.GetCustomAttributes(typeof(CommandAttribute), inherit: false);
			CommandAttribute[] array2 = array;
			foreach (CommandAttribute commandAttribute in array2)
			{
				if (commandAttribute.Name != null)
				{
					string text = commandAttribute.Name.ToUpper();
					list.Add(text);
					m_commands[text] = type;
				}
			}
		}
		return list;
	}
}
