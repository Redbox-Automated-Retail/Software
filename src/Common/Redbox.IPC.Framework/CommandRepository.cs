using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using Redbox.Core;

namespace Redbox.IPC.Framework;

[Command("command-repository", Filter = "System|Command-Repository")]
[Description("The COMMAND-REPOSITORY command scans the execution directory of the cache service for .NET assembly files (.dll and .exe) that contain types marked with the CommandAttribute class.")]
public class CommandRepository
{
	private static readonly IDictionary<string, CommandInstance> m_commandInstanceCache = new Dictionary<string, CommandInstance>();

	public static string[] AssemblyPatterns { get; set; }

	internal static ICollection<string> AllCommands => m_commandInstanceCache.Keys;

	public void Default(CommandContext context)
	{
		foreach (string item in DiscoverInstalledCommands())
		{
			context.Messages.Add("Installed Command '" + item + "'.");
		}
	}

	public static void InstallPerformanceCounters(string groupName)
	{
		List<string> list = new List<string>();
		DiscoverInstalledCommands();
		foreach (KeyValuePair<string, CommandInstance> item in m_commandInstanceCache)
		{
			LogHelper.Instance.Log("Creating performance counters for command {0}.", item.Key);
			foreach (string key in item.Value.FormMethodCache.Keys)
			{
				list.Add($"{item.Key.ToUpper()} {key.ToLower()}");
			}
		}
		PerformanceCounterSetup.Initialize(groupName, list);
	}

	public static void UninstallPerformanceCounters(string groupName)
	{
		if (PerformanceCounterCategory.Exists(groupName))
		{
			PerformanceCounterCategory.Delete(groupName);
		}
	}

	internal static List<string> DiscoverInstalledCommands()
	{
		using ExecutionTimer executionTimer = new ExecutionTimer();
		List<string> list = new List<string>();
		string directoryName = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
		List<string> list2 = new List<string>();
		if (AssemblyPatterns == null || AssemblyPatterns.Length == 0)
		{
			list2.AddRange(Directory.GetFiles(directoryName, "*.dll"));
			list2.AddRange(Directory.GetFiles(directoryName, "*.exe"));
		}
		else
		{
			string[] assemblyPatterns = AssemblyPatterns;
			foreach (string text in assemblyPatterns)
			{
				if (text.IndexOf("*") == -1 && text.IndexOf("?") == -1)
				{
					list2.Add(Path.Combine(directoryName, text));
				}
				else
				{
					list2.AddRange(Directory.GetFiles(directoryName, text));
				}
			}
		}
		foreach (string item in list2)
		{
			if (!File.Exists(item))
			{
				continue;
			}
			try
			{
				Assembly assembly = Assembly.LoadFrom(item);
				if (assembly != null)
				{
					LogHelper.Instance.Log("Load commands from assembly: '{0}'", assembly);
					list.AddRange(FindCommands(assembly.GetTypes()));
				}
			}
			catch (Exception e)
			{
				LogHelper.Instance.Log($"Unable to load assembly '{item}' to scan for commands.", e);
			}
		}
		LogHelper.Instance.Log($"Time to scan for commands: {executionTimer.Elapsed}", LogEntryType.Info);
		return list;
	}

	internal static CommandInstance GetCommand(string name)
	{
		if (string.IsNullOrEmpty(name))
		{
			return null;
		}
		string key = name.ToUpper();
		if (!m_commandInstanceCache.ContainsKey(key))
		{
			return null;
		}
		return m_commandInstanceCache[key];
	}

	private static List<string> FindCommands(IEnumerable<Type> types)
	{
		List<string> list = new List<string>();
		foreach (Type type in types)
		{
			CommandAttribute[] array = (CommandAttribute[])type.GetCustomAttributes(typeof(CommandAttribute), inherit: false);
			foreach (CommandAttribute commandAttribute in array)
			{
				if (commandAttribute.Name != null)
				{
					string text = commandAttribute.Name.ToUpper();
					list.Add(text);
					CommandInstance commandInstance = CommandInstance.GetCommandInstance(type);
					m_commandInstanceCache[text] = commandInstance;
					if (!string.IsNullOrEmpty(commandAttribute.Filter))
					{
						commandAttribute.Filter.Trim().Split('|').ToList()
							.ForEach(commandInstance.Filters.Add);
					}
				}
			}
		}
		return list;
	}
}
