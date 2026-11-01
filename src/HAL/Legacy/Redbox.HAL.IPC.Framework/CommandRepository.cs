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
		List<string> commands = new List<string>();
		string directoryName = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
		List<string> assemblyFiles = new List<string>();
		assemblyFiles.AddRange(Directory.GetFiles(directoryName, "*.dll"));
		foreach (string file in assemblyFiles)
		{
			try
			{
				Assembly assembly = Assembly.LoadFrom(file);
				commands.AddRange(FindCommands(assembly.GetTypes()));
			}
			catch (BadImageFormatException)
			{
			}
			catch (Exception e)
			{
				LogHelper.Instance.Log($"Unable to load assembly '{file}' to scan for commands.", e);
			}
		}
		LogHelper.Instance.Log($"Time to scan for commands: {executionTimer.Elapsed}");
		return commands;
	}

	internal static CommandInstance GetCommand(string name)
	{
		if (string.IsNullOrEmpty(name))
		{
			return null;
		}
		if (!m_commands.TryGetValue(name.ToUpper(), out var commandType))
		{
			return null;
		}
		if (!m_instanceCache.TryGetValue(commandType, out var instance))
		{
			CommandInstance commandInstance = new CommandInstance();
			commandInstance.CommandType = commandType;
			instance = commandInstance;
			if (Attribute.GetCustomAttribute(commandType, typeof(DescriptionAttribute)) is DescriptionAttribute descriptionAttribute)
			{
				instance.CommandDescription = descriptionAttribute.Description;
			}
			MethodInfo[] methods = commandType.GetMethods();
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
				int parameterIndex = 0;
				foreach (ParameterInfo parameterInfo in parameters)
				{
					if (parameterInfo.ParameterType == typeof(CommandContext))
					{
						continue;
					}
					FormMethodParameter formMethodParameter = new FormMethodParameter();
					formMethodParameter.Parameter = parameterInfo;
					formMethodParameter.Index = parameterIndex++;
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
				instance.FormMethodCache[commandFormAttribute.Name.ToUpper()] = formMethod2;
			}
			MethodInfo method = commandType.GetMethod("Default", new Type[1] { typeof(CommandContext) });
			if (method != null)
			{
				instance.FormMethodCache["Default"] = new FormMethod
				{
					Method = method
				};
			}
			m_instanceCache[commandType] = instance;
		}
		return instance;
	}

	private static List<string> FindCommands(IEnumerable<Type> types)
	{
		List<string> list = new List<string>();
		foreach (Type type in types)
		{
			foreach (CommandAttribute commandAttribute in (CommandAttribute[])type.GetCustomAttributes(typeof(CommandAttribute), inherit: false))
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
