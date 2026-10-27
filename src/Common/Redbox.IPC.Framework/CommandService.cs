using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Text;
using System.Threading;
using Redbox.Command.Tokenizer;
using Redbox.Core;
using Redbox.Macros;
using Redbox.Tokenizer.Framework;

namespace Redbox.IPC.Framework;

public class CommandService
{
	private readonly Dictionary<string, List<byte[]>> m_accessFilter;

	private static readonly PropertyDictionary m_properties = new PropertyDictionary();

	public static CommandService Instance => Singleton<CommandService>.Instance;

	public PropertyDictionary Properties => m_properties;

	public void SetAccesFilter(string ruleFile)
	{
		if (!File.Exists(ruleFile))
		{
			return;
		}
		using StreamReader streamReader = new StreamReader(ruleFile);
		string text;
		while ((text = streamReader.ReadLine()) != null)
		{
			string[] array = text.Split(new string[1] { "," }, StringSplitOptions.RemoveEmptyEntries);
			string command = array[0].Trim();
			string filter = array[1].Trim();
			SetAccessFilter(command, filter);
		}
	}

	public void SetAccessFilter(string command, List<string> filters)
	{
		command = command.ToLower();
		if (!m_accessFilter.ContainsKey(command))
		{
			m_accessFilter[command] = new List<byte[]>();
		}
		m_accessFilter[command].AddRange(filters.ConvertAll(delegate(string str)
		{
			if (str == "*")
			{
				return new byte[0];
			}
			string[] array = str.Split(new string[1] { "." }, StringSplitOptions.RemoveEmptyEntries);
			List<byte> list = new List<byte>();
			for (int i = 0; i < array.Length && !(array[i] == "*"); i++)
			{
				list.Add(Convert.ToByte(array[i]));
			}
			return list.ToArray();
		}));
		m_accessFilter[command].Sort((byte[] lhs, byte[] rhs) => lhs.Length.CompareTo(rhs.Length));
	}

	public void SetAccessFilter(string command, string filter)
	{
		SetAccessFilter(command, new List<string> { filter });
	}

	public string GetServerBusyResult(ExecutionTimer timer, int maximumThreads)
	{
		Interlocked.Increment(ref Statistics.Instance.NumberServerBusyResponses);
		CommandResult commandResult = new CommandResult();
		LogHelper.Instance.Log("Server too busy to handle request, maximum available threads: {0}.", maximumThreads);
		commandResult.Errors.Add(Error.NewError("J777", "Server too busy to service request.", "Resubmit the command request when the server isn't so busy."));
		commandResult.ExecutionTime = timer.Elapsed;
		commandResult.Success = false;
		ProtocolHelper.FormatErrors(commandResult.Errors, commandResult.Messages);
		return commandResult.ToString();
	}

	public CommandResult Execute(ISession session, string input)
	{
		return Execute(session, string.Empty, input);
	}

	public CommandResult Execute(ISession session, string originIP, string input)
	{
		return Execute(session, string.Empty, input, null, enableFilters: false, null);
	}

	public CommandResult Execute(ISession session, string originIP, string input, List<string> sourceFilters, bool enableFilters, Action<string, string> paramAction)
	{
		using ExecutionTimer executionTimer = new ExecutionTimer();
		CommandResult commandResult = new CommandResult();
		CommandContext commandContext = new CommandContext
		{
			Session = session,
			MessageSink = session
		};
		string text = null;
		try
		{
			if (!GetTokenizer(input, commandResult, commandContext, out var tokenizer))
			{
				return commandResult;
			}
			SimpleToken mnemonic = tokenizer.Tokens.GetMnemonic();
			CommandInstance command = CommandRepository.GetCommand(mnemonic.Value);
			if (command == null)
			{
				LogHelper.Instance.Log($"Command {mnemonic.Value} not recognized.");
				commandResult.Errors.Add(Error.NewError("S514", $"Invalid Command: {mnemonic.Value}", ""));
				return commandResult;
			}
			if (!command.IsInFilter(enableFilters, sourceFilters))
			{
				LogHelper.Instance.Log($"Command {mnemonic.Value} is not in filters.");
				commandResult.Errors.Add(Error.NewError("S514", $"Invalid Command: {mnemonic.Value}", ""));
			}
			if (command.FormMethodCache.Count == 0)
			{
				LogHelper.Instance.Log($"Command {mnemonic.Value} does not have any valid executable forms.");
				commandResult.Errors.Add(Error.NewError("S514", $"Invalid Command: {mnemonic.Value}", ""));
				return commandResult;
			}
			if (!string.IsNullOrEmpty(originIP) && !AllowOriginForCommand(originIP, mnemonic.Value))
			{
				string message = $"Command {mnemonic.Value} does allow execution from {originIP}.";
				LogHelper.Instance.Log(message);
				commandResult.Errors.Add(Error.NewError("S514", $"Invalid Command: {mnemonic.Value}", ""));
				return commandResult;
			}
			SimpleTokenList symbols = tokenizer.Tokens.GetSymbols();
			if ((symbols.Count == 0 && command.HasDefault()) || command.HasOnlyDefault())
			{
				LogHelper.Instance.Log(input, LogEntryType.Info);
				command.InvokeDefault(commandResult, commandContext, tokenizer, enableFilters, sourceFilters);
				return commandResult;
			}
			if (symbols.Count == 0)
			{
				commandResult.Errors.Add(Error.NewError("S001", $"No form symbol was specified for the {mnemonic.Value.ToUpper()} comand.", $"Use the HELP command to learn how to properly invoke the {mnemonic.Value.ToUpper()} command and its forms."));
				return commandResult;
			}
			FormMethod method = command.GetMethod(symbols[0].Value);
			if (method == null)
			{
				commandResult.Errors.Add(Error.NewError("S001", $"An unknown form was used for the {mnemonic.Value.ToUpper()} comand.", $"Use the HELP command to learn how to properly invoke the {mnemonic.Value.ToUpper()} command and its forms."));
				return commandResult;
			}
			if (!method.ValidateParameters(commandResult, tokenizer, paramAction))
			{
				if (method.Loggable)
				{
					LogHelper.Instance.Log(input, LogEntryType.Info);
				}
				return commandResult;
			}
			if (method.Loggable)
			{
				LogHelper.Instance.Log(input, LogEntryType.Info);
			}
			text = $"{mnemonic.Value.ToUpper()} {symbols[0].Value.ToLower()}";
			PerformanceCounterHelper.Instance.IncrementCommandPerSecond(text);
			method.Invoke(commandResult, commandContext, tokenizer, command.GetInstance(), enableFilters, sourceFilters);
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("CommandService.Execute error", e);
			commandResult.Errors.Add(Error.NewError("S999", "An unhandled exception was raised in CommandService.Execute.", e));
		}
		finally
		{
			commandResult.ExecutionTime = executionTimer.Elapsed;
			commandResult.Success = !commandContext.Errors.ContainsError() && !commandResult.Errors.ContainsError();
			commandResult.Messages.AddRange(commandContext.Messages);
			commandResult.Errors.AddRange(commandContext.Errors);
			Statistics.Instance.TrackCommandStatistics(commandResult.ExecutionTime);
			if (text != null)
			{
				PerformanceCounterHelper.Instance.IncrementCommandExecutionTime(text, executionTimer.ElapsedTicks);
			}
			LogHelper.Instance.Log(commandResult.ToString(IsResultLoggingEnabled()));
			if (commandResult.Errors.Count > 0)
			{
				StringBuilder builder = new StringBuilder();
				commandResult.Errors.ForEach(delegate(Error error)
				{
					builder.AppendFormat("{0} {1}", error.Code, error.Description);
					builder.AppendFormat(error.Details);
				});
				LogHelper.Instance.Log(builder.ToString());
			}
		}
		return commandResult;
	}

	private CommandService()
	{
		LogHelper.Instance.Log("Initializing CommandService.", LogEntryType.Info);
		foreach (string item in CommandRepository.DiscoverInstalledCommands())
		{
			LogHelper.Instance.Log("Loaded command: " + item, LogEntryType.Info);
		}
		m_accessFilter = new Dictionary<string, List<byte[]>>();
	}

	private bool IsResultLoggingEnabled()
	{
		if (ConfigurationManager.AppSettings["IpcLogResults"] != null)
		{
			return bool.Parse(ConfigurationManager.AppSettings["IpcLogResults"]);
		}
		return false;
	}

	private bool AllowOriginForCommand(string origin, string command)
	{
		LogHelper.Instance.Log($"Checking access rules for {origin}.", LogEntryType.Debug);
		command = command.ToLower();
		if (!m_accessFilter.ContainsKey(command))
		{
			return true;
		}
		string[] array = origin.Split('.');
		byte[] array2 = new byte[array.Length];
		for (int i = 0; i < array.Length; i++)
		{
			array2[i] = Convert.ToByte(array[i]);
		}
		foreach (byte[] item in m_accessFilter[command])
		{
			bool flag = true;
			for (int j = 0; j < item.Length; j++)
			{
				if (item[j] != array2[j])
				{
					flag = false;
					break;
				}
			}
			if (flag)
			{
				return true;
			}
		}
		return false;
	}

	private static bool GetTokenizer(string input, CommandResult result, CommandContext context, out CommandTokenizer tokenizer)
	{
		input = m_properties.ExpandProperties(input, Location.UnknownLocation);
		tokenizer = new CommandTokenizer(0, input);
		tokenizer.Tokenize();
		if (tokenizer.Tokens.HasOnlyComments())
		{
			return false;
		}
		if (tokenizer.Errors.ContainsError() || tokenizer.Tokens.GetMnemonic() == null)
		{
			foreach (Redbox.Tokenizer.Framework.Error error in tokenizer.Errors)
			{
				result.Errors.Add(error.IsWarning ? Error.NewWarning(error.Code, error.Description, error.Details) : Error.NewError(error.Code, error.Description, error.Details));
			}
			result.ExtendedErrorMessage = $"Parsing of input '{input}' failed. Correct the command syntax and try again.";
			return false;
		}
		foreach (SimpleToken symbol in tokenizer.Tokens.GetSymbols())
		{
			context.Parameters[symbol.Value] = symbol.Value;
		}
		foreach (SimpleToken keyValuePair2 in tokenizer.Tokens.GetKeyValuePairs())
		{
			if (keyValuePair2.ConvertValue() is KeyValuePair keyValuePair)
			{
				context.Parameters[$"{keyValuePair.Key}:"] = keyValuePair.Value;
			}
		}
		return true;
	}
}
