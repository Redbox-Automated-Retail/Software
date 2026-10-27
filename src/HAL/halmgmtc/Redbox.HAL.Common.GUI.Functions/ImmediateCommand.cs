using System;
using System.Collections.Generic;
using System.Text;
using Redbox.HAL.Client;

namespace Redbox.HAL.Common.GUI.Functions;

public class ImmediateCommand
{
	private List<string> m_arguments;

	private static IDictionary<string, ImmediateCommand> m_commands;

	public static HardwareService Service { get; set; }

	protected internal virtual string Token => string.Empty;

	protected virtual string Help => string.Empty;

	protected virtual string ExpectedArguments => string.Empty;

	internal static IDictionary<string, ImmediateCommand> Commands
	{
		get
		{
			if (m_commands == null)
			{
				m_commands = new Dictionary<string, ImmediateCommand>();
			}
			return m_commands;
		}
	}

	internal static bool InImmediateProgramMode { get; set; }

	internal List<string> Arguments
	{
		get
		{
			if (m_arguments == null)
			{
				m_arguments = new List<string>();
			}
			return m_arguments;
		}
	}

	public static ImmediateCommand GetCommand(string statement)
	{
		foreach (ImmediateCommand value in Commands.Values)
		{
			if (string.IsNullOrEmpty(value.Token) || !statement.StartsWith(value.Token, StringComparison.InvariantCultureIgnoreCase))
			{
				continue;
			}
			value.Arguments.Clear();
			if (!string.IsNullOrEmpty(value.Token))
			{
				int startIndex = statement.IndexOf(value.Token);
				string text = statement.Remove(startIndex, 1);
				if (!string.IsNullOrEmpty(text))
				{
					value.Arguments.Add(text);
				}
			}
			return value;
		}
		return new ExecuteStatementImmediateCommand
		{
			Arguments = { statement }
		};
	}

	public ImmediateCommandResult Execute()
	{
		ImmediateCommandResult result = new ImmediateCommandResult();
		OnExecute(result);
		return result;
	}

	public override string ToString()
	{
		return $"{Token,-4} {ExpectedArguments,-16} {Help}";
	}

	protected ImmediateCommand()
	{
	}

	protected virtual void OnExecute(ImmediateCommandResult result)
	{
	}

	internal static string FormatMessageFromJobResult(HardwareCommandResult result)
	{
		StringBuilder stringBuilder = new StringBuilder();
		for (int i = 0; i < result.CommandMessages.Count - 1; i++)
		{
			stringBuilder.AppendLine(result.CommandMessages[i]);
		}
		return stringBuilder.ToString();
	}

	static ImmediateCommand()
	{
		Type[] types = typeof(ImmediateCommand).Assembly.GetTypes();
		foreach (Type type in types)
		{
			if (type.IsSubclassOf(typeof(ImmediateCommand)) && Activator.CreateInstance(type) is ImmediateCommand immediateCommand)
			{
				Commands[immediateCommand.Token] = immediateCommand;
			}
		}
	}
}
