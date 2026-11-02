using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Component.Model.Attributes;
using Redbox.HAL.Component.Model.Timers;
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
		foreach (string item in DiscoverInstalledCommands())
		{
			context.Messages.Add("Installed Command '" + item + "'.");
		}
	}

	internal static List<string> DiscoverInstalledCommands()
	{
		using ExecutionTimer executionTimer = new ExecutionTimer();
		List<string> commands = new List<string>();
		string directoryName = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;
		List<string> assemblyFiles = new List<string>();
		assemblyFiles.AddRange(Directory.GetFiles(directoryName, "*.dll"));
		foreach (string item in assemblyFiles)
		{
#if !NETFRAMEWORK
			// A self-contained .NET service has the whole runtime next to it, and an .exe's code is in <name>.dll; .NET
			// Framework scanned neither (only libraries are .dll there).
			if (IsRuntimeAssembly(Path.GetFileName(item)) || File.Exists(Path.ChangeExtension(item, ".runtimeconfig.json")) || !IsNetAssembly(item))
			{
				continue;
			}
#endif
			try
			{
				Assembly assembly = Assembly.LoadFrom(item);
				commands.AddRange(FindCommands(assembly.GetTypes()));
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
		return commands;
	}

	internal static CommandInstance? GetCommand(string name)
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
			instance = new CommandInstance
			{
				CommandType = commandType
			};
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
				UsageAttribute? usageAttribute = Attribute.GetCustomAttribute(methodInfo, typeof(UsageAttribute)) as UsageAttribute;
				DescriptionAttribute? methodDescription = Attribute.GetCustomAttribute(methodInfo, typeof(DescriptionAttribute)) as DescriptionAttribute;
				FormMethod formMethod = new FormMethod
				{
					Method = methodInfo,
					Usage = ((usageAttribute != null) ? usageAttribute.Template : string.Empty),
					Description = ((methodDescription != null) ? methodDescription.Description : string.Empty)
				};
				int parameterIndex = 0;
				foreach (ParameterInfo parameterInfo in parameters)
				{
					if (parameterInfo.ParameterType == typeof(CommandContext))
					{
						continue;
					}
					FormMethodParameter formMethodParameter = new FormMethodParameter
					{
						Parameter = parameterInfo,
						Index = parameterIndex++
					};
					CommandKeyValueAttribute? commandKeyValueAttribute = Attribute.GetCustomAttribute(parameterInfo, typeof(CommandKeyValueAttribute)) as CommandKeyValueAttribute;
					formMethodParameter.KeyName = parameterInfo.Name!.ToUpper();
					if (commandKeyValueAttribute != null)
					{
						formMethodParameter.MetaData = commandKeyValueAttribute;
						if (commandKeyValueAttribute.KeyName != null)
						{
							formMethodParameter.KeyName = commandKeyValueAttribute.KeyName.ToUpper();
						}
						formMethod.ParameterCache[formMethodParameter.KeyName] = formMethodParameter;
					}
					else
					{
						formMethod.ParameterCache[formMethodParameter.KeyName] = formMethodParameter;
					}
				}
				instance.FormMethodCache[commandFormAttribute.Name!.ToUpper()] = formMethod;
			}
			MethodInfo? method = commandType.GetMethod("Default", new Type[1] { typeof(CommandContext) });
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

	internal static void Register(IEnumerable<Type> types)
	{
		FindCommands(types);
		LogHelper.Instance.Log("Installed commands:");
		foreach (string allCommand in AllCommands)
		{
			LogHelper.Instance.Log(" Command '{0}'.", allCommand);
		}
	}

#if !NETFRAMEWORK
	// Whether a file is a .NET assembly, from its PE header alone. The command scan calls this before Assembly.LoadFrom: on .NET,
	// LoadFrom maps a file that is not IL-only with the system loader before it finds there is no .NET metadata, which runs a
	// native DLL's start-up code (one of the ClearImage DLLs in halservice\bin crashes there). .NET Framework never ran it.
	private static bool IsNetAssembly(string path)
	{
		try
		{
			using FileStream stream = File.OpenRead(path);
			using System.Reflection.PortableExecutable.PEReader reader = new System.Reflection.PortableExecutable.PEReader(stream);
			return reader.HasMetadata;
		}
		catch (Exception)
		{
			return false;
		}
	}

	private static bool IsRuntimeAssembly(string fileName)
	{
		string[] prefixes = new string[6] { "System.", "Microsoft.", "mscorlib.", "netstandard.", "WindowsBase.", "api-ms-" };
		return Array.Exists(prefixes, (string prefix) => fileName.StartsWith(prefix, StringComparison.OrdinalIgnoreCase));
	}

#endif
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
