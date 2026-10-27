using System;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Core;

namespace Redbox.HAL.IPC.Framework;

public class CommandService
{
	public static CommandService Instance => Singleton<CommandService>.Instance;

	public CommandResult Execute(ISession session, string input)
	{
		using ExecutionTimer executionTimer = new ExecutionTimer();
		CommandResult commandResult = new CommandResult();
		CommandContext commandContext = new CommandContext
		{
			Session = session,
			MessageSink = session
		};
		try
		{
			if (!GetTokenizer(input, commandResult, commandContext, out var tokenizer))
			{
				return commandResult;
			}
			Token mnemonic = tokenizer.Tokens.GetMnemonic();
			CommandInstance command = CommandRepository.GetCommand(mnemonic.Value);
			if (command == null)
			{
				commandResult.Errors.Add(Error.NewError("S514", $"Command {mnemonic.Value} not recognized.", "Use HELP command to learn more about available commands."));
				commandResult.ExtendedErrorMessage = $"Command '{mnemonic.Value}' not recognized. Use HELP to learn more about available commands.";
				return commandResult;
			}
			if (command.FormMethodCache.Count == 0)
			{
				commandResult.Errors.Add(Error.NewError("S322", $"Command {mnemonic.Value} does not have any valid executable forms.", "Please contact the support personnel for this command service and notify them of this error."));
			}
			else
			{
				TokenList symbols = tokenizer.Tokens.GetSymbols();
				if ((symbols.Count == 0 && command.HasDefault()) || command.HasOnlyDefault())
				{
					command.InvokeDefault(commandResult, commandContext, tokenizer);
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
				if (!method.ValidateParameters(commandResult, tokenizer))
				{
					return commandResult;
				}
				method.Invoke(commandResult, commandContext, tokenizer, command.GetInstance());
			}
		}
		catch (Exception e)
		{
			commandResult.Errors.Add(Error.NewError("S999", "An unhandled exception was raised in CommandService.Execute.", e));
		}
		finally
		{
			commandResult.ExecutionTime = executionTimer.Elapsed;
			commandResult.Success = !commandContext.Errors.ContainsError() && !commandResult.Errors.ContainsError();
			commandResult.Messages.AddRange(commandContext.Messages);
			commandResult.Errors.AddRange(commandContext.Errors);
			Statistics.Instance.TrackCommandStatistics(commandResult.ExecutionTime);
			commandContext.Messages.Clear();
			commandContext.Errors.Clear();
		}
		return commandResult;
	}

	private CommandService()
	{
		LogHelper.Instance.Log("Initializing CommandService.");
		foreach (string item in CommandRepository.DiscoverInstalledCommands())
		{
			LogHelper.Instance.Log("Loaded command: " + item);
		}
	}

	private static bool GetTokenizer(string input, CommandResult result, CommandContext context, out CommandTokenizer tokenizer)
	{
		input = ServiceLocator.Instance.GetService<IRuntimeService>().ExpandConstantMacros(input);
		tokenizer = new CommandTokenizer(0, input);
		tokenizer.Tokenize();
		if (tokenizer.Tokens.HasOnlyComments())
		{
			return false;
		}
		if (tokenizer.Errors.ContainsError() || tokenizer.Tokens.GetMnemonic() == null)
		{
			result.Errors.AddRange(tokenizer.Errors);
			result.ExtendedErrorMessage = $"Parsing of input '{input}' failed. Correct the command syntax and try again.";
			return false;
		}
		foreach (Token symbol in tokenizer.Tokens.GetSymbols())
		{
			context.Parameters[symbol.Value] = symbol.Value;
		}
		foreach (Token keyValuePair2 in tokenizer.Tokens.GetKeyValuePairs())
		{
			if (keyValuePair2.ConvertValue() is KeyValuePair keyValuePair)
			{
				context.Parameters[$"{keyValuePair.Key}:"] = keyValuePair.Value;
			}
		}
		return true;
	}
}
