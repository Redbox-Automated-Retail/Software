using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using Redbox.Core;

namespace Redbox.GetOpts;

public class CommandLine : IDisposable
{
	internal const char SingleDash = '-';

	internal const string DoubleDash = "--";

	internal const char ColonSeparator = ':';

	internal const char EqualSeparator = '=';

	internal const char FlagEnabled = '+';

	internal const char FlagDisabled = '-';

	private UsageAttribute? m_usage;

	private Stack<string>? m_errors;

	private OptionValueList? m_options;

	private readonly object m_container;

	private readonly IConsole m_console;

	private List<string>? m_bareArguments;

	private List<string>? m_providedOptions;

	public IList<string> BareArguments
	{
		get
		{
			if (m_bareArguments == null)
			{
				m_bareArguments = new List<string>();
			}
			return m_bareArguments;
		}
	}

	public Stack<string> Errors
	{
		get
		{
			if (m_errors == null)
			{
				m_errors = new Stack<string>();
			}
			return m_errors;
		}
	}

	public bool HelpRequested { get; internal set; }

	internal List<string> ProvidedOptions
	{
		get
		{
			if (m_providedOptions == null)
			{
				m_providedOptions = new List<string>();
			}
			return m_providedOptions;
		}
	}

	internal OptionValueList Options
	{
		get
		{
			if (m_options == null)
			{
				m_options = OptionValue.GetOptionValues(m_container);
			}
			return m_options;
		}
	}

	public static CommandLine ParseTo(object container)
	{
		return ParseTo(container, new DefaultConsole(), null);
	}

	public static CommandLine ParseTo(object container, IConsole console, string[]? arguments)
	{
		CommandLine commandLine = new CommandLine(container, console);
		commandLine.GetUsageAttribute();
		string[] args = arguments ?? GetCommandLineArguments();
		ReadOnlyCollection<string> requiredOptions = commandLine.Options.GetRequiredOptions();
		if (args.Length - 1 <= 0)
		{
			if (requiredOptions.Count > 0)
			{
				commandLine.Errors.Push("Required parameters are missing.");
			}
			return commandLine;
		}
		int index = 0;
		OptionValue? optionValue = null;
		do
		{
			string argument = args[index];
			if (argument == "--help" || argument == "-?")
			{
				commandLine.HelpRequested = true;
				commandLine.WriteUsage(includeHelp: true);
				break;
			}
			if (argument.StartsWith('-'.ToString()) && argument != '-'.ToString() && argument != "--")
			{
				if (!commandLine.ProcessStandardForm(ref optionValue, argument))
				{
					break;
				}
			}
			else if (optionValue == null)
			{
				commandLine.BareArguments.Add(argument);
			}
			else
			{
				string? key = argument;
				object? value = argument;
				if ((optionValue.IsDictionary() && !commandLine.ProcessDictionaryValues(argument, (string?)value, out key, out value)) || !commandLine.SetOptionValue(optionValue, value, key))
				{
					break;
				}
				commandLine.ProvidedOptions.Add(optionValue.GetName());
				optionValue = null;
			}
			index++;
		}
		while (index != args.Length);
		if (new List<string>(commandLine.ProvidedOptions.Intersect(requiredOptions)).Count != requiredOptions.Count)
		{
			commandLine.Errors.Push("Required parameters are missing.");
		}
		return commandLine;
	}

	public void WriteUsage(bool includeHelp)
	{
		Assembly entryAssembly = Assembly.GetEntryAssembly()!; // GetCustomAttribute threw on null before too
		AssemblyTitleAttribute? assemblyTitleAttribute = (AssemblyTitleAttribute?)Attribute.GetCustomAttribute(entryAssembly, typeof(AssemblyTitleAttribute));
		AssemblyCopyrightAttribute? assemblyCopyrightAttribute = (AssemblyCopyrightAttribute?)Attribute.GetCustomAttribute(entryAssembly, typeof(AssemblyCopyrightAttribute));
		AssemblyTrademarkAttribute? assemblyTrademarkAttribute = (AssemblyTrademarkAttribute?)Attribute.GetCustomAttribute(entryAssembly, typeof(AssemblyTrademarkAttribute));
		if (assemblyTitleAttribute != null)
		{
			m_console.Write(assemblyTitleAttribute.Title);
		}
		m_console.Write(" " + entryAssembly.GetName().Version);
		m_console.Write(Environment.NewLine);
		if (assemblyCopyrightAttribute != null && assemblyCopyrightAttribute.Copyright.Length > 0)
		{
			m_console.Write(assemblyCopyrightAttribute.Copyright + Environment.NewLine);
		}
		if (assemblyTrademarkAttribute != null && assemblyTrademarkAttribute.Trademark.Length > 0)
		{
			m_console.Write(assemblyTrademarkAttribute.Trademark + Environment.NewLine);
		}
		if (m_usage != null)
		{
			m_console.WriteLine("\nUsage: {0}\n", m_usage.Template);
		}
		foreach (string error in Errors)
		{
			m_console.WriteLine(error);
		}
		if (Errors.Count > 0)
		{
			m_console.WriteLine("\nTry --help or -? for more information.\n");
		}
		if (includeHelp)
		{
			m_console.WriteLine("Options:\n");
			Options.WriteOptions(m_console);
		}
	}

	public void Dispose()
	{
		if (m_console != null)
		{
			m_console.Dispose();
		}
	}

	public static string[] GetCommandLineArguments()
	{
		return Environment.GetCommandLineArgs();
	}

	internal CommandLine(object container, IConsole console)
	{
		m_container = container;
		m_console = console;
	}

	internal void GetUsageAttribute()
	{
		m_usage = (UsageAttribute?)Attribute.GetCustomAttribute(m_container.GetType(), typeof(UsageAttribute));
	}

	internal static bool GetFlagNameAndValue(string parameter, out string? name)
	{
		int dashCount = GetDashCount(parameter);
		if (parameter.Length == dashCount)
		{
			name = null;
			return false;
		}
		name = parameter.Substring(dashCount, parameter.Length - (dashCount + 1));
		if (parameter[parameter.Length - 1] == '+')
		{
			return true;
		}
		if (parameter[parameter.Length - 1] == '-')
		{
			return false;
		}
		name = null;
		return false;
	}

	internal static object? GetValues(string? parameter, out string? name, char[] separators)
	{
		if (parameter == null)
		{
			name = null;
			return null;
		}
		object? result = null;
		int dashCount = GetDashCount(parameter);
		string text = parameter.Substring(dashCount);
		int separatorIndex = -1;
		foreach (char value in separators)
		{
			separatorIndex = text.IndexOf(value);
			if (separatorIndex != -1)
			{
				break;
			}
		}
		if (separatorIndex != -1)
		{
			name = text.Substring(0, separatorIndex);
			result = text.Substring(separatorIndex + 1, text.Length - (separatorIndex + 1));
		}
		else
		{
			name = text;
		}
		return result;
	}

	internal bool SetOptionValue(OptionValue optionValue, object? value, string? argument)
	{
		try
		{
			optionValue.SetValue(argument, value);
			return true;
		}
		catch (Exception ex)
		{
			Errors.Push("Parameter '" + argument + "': " + ex.Message);
			return false;
		}
	}

	internal static int GetDashCount(IEnumerable<char> parameter)
	{
		int dashCount = 0;
		using (IEnumerator<char> enumerator = parameter.GetEnumerator())
		{
			while (enumerator.MoveNext() && enumerator.Current == '-')
			{
				dashCount++;
			}
		}
		return dashCount;
	}

	internal bool ProcessStandardForm(ref OptionValue? optionValue, string argument)
	{
		int dashCount = GetDashCount(argument);
		if (optionValue != null)
		{
			Errors.Push("The parameter '" + optionValue.GetName() + "' was not properly set.");
			return false;
		}
		bool flagNameAndValue = GetFlagNameAndValue(argument, out var name);
		object? value = ((name != null) ? ((object)flagNameAndValue) : GetValues(argument, out name, new char[2] { ':', '=' }));
		if (name!.Length == 1 && dashCount > 1) // GetValues always names a non-null argument
		{
			Errors.Push("The parameter '" + argument + "' is short form and does not support the double dash prefix.");
			return false;
		}
		optionValue = Options.GetOptionValue(name);
		if (optionValue == null)
		{
			Errors.Push("The parameter '" + argument + "' is not valid.");
			return false;
		}
		if (optionValue.IsDictionary() && !ProcessDictionaryValues(argument, (string?)value, out name, out value))
		{
			return false;
		}
		if (value != null)
		{
			if (!SetOptionValue(optionValue, value, name))
			{
				return false;
			}
			ProvidedOptions.Add(optionValue.GetName());
			optionValue = null;
		}
		else if (optionValue.IsFlag())
		{
			optionValue.SetValue(null, true);
			ProvidedOptions.Add(optionValue.GetName());
			optionValue = null;
		}
		return true;
	}

	internal bool ProcessDictionaryValues(string argument, string? parameterValue, out string? key, out object? value)
	{
		value = GetValues(parameterValue, out key, new char[1] { '=' });
		if (value != null && key != null)
		{
			return true;
		}
		if (key != null)
		{
			Errors.Push("The syntax of parameter '" + argument + "' is incorrect.");
			return false;
		}
		return true;
	}
}
