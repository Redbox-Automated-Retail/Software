using System;
using System.Collections.Generic;
using System.IO;
using Redbox.HAL.Client;
using Redbox.HAL.Client.Services;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Core;

namespace HALUtilities;

internal class ScriptMode : IDisposable
{
	private static class ImmediateCommands
	{
		public const string ExitCommand = "EXIT";

		public const string ImmediateCommandPrompt = "> ";
	}

	private readonly IConsole m_console;

	private readonly HardwareService Service;

	private readonly ClientHelper Helper;

	public void Dispose()
	{
		m_console.Dispose();
		Helper.Dispose();
	}

	internal ScriptMode(HardwareService service)
	{
		m_console = new DefaultConsole();
		Service = service;
		Helper = new ClientHelper(service);
	}

	internal int Run(string[] programArgs)
	{
		string text = null;
		ImmediateCommand.Service = Service;
		for (int i = 0; i < programArgs.Length; i++)
		{
			if (programArgs[i].StartsWith("--script"))
			{
				text = CommandLineOption.GetOptionVal(programArgs[i], "=", string.Empty);
				if (text == string.Empty)
				{
					m_console.WriteLine("An invalid script option was passed; please correct. Thank you.");
					return 1;
				}
				if (!File.Exists(text))
				{
					m_console.WriteLine("The specified file '{0}' doesn't exist. Try again.", text);
					return 1;
				}
				ExecuteScriptCommands(text);
				return 0;
			}
		}
		m_console.WriteLine("HAL Interactive Console, use ? for help.");
		bool flag = true;
		while (flag)
		{
			m_console.Write("> ");
			string statement = Console.ReadLine();
			flag = ExecuteStatement(statement);
			if (Service.GetJob(Constants.ExecutionContexts.ImmediateModeContext, out var job).Success && job.GetStack(out var stack).Success)
			{
				FormatStack(stack, m_console);
			}
		}
		return 0;
	}

	private static void ShowResults(HardwareCommandResult result, IConsole console)
	{
		if (result == null)
		{
			return;
		}
		if (!result.Success && result.Errors.Count > 0)
		{
			console.WriteLine("\nClient Errors:");
			result.Errors.ForEach(delegate(Error each)
			{
				console.WriteLine(each);
				console.WriteLine("Details: {0}\n", each.Details);
			});
		}
		if (result.CommandMessages.Count > 0)
		{
			console.WriteLine("Command Messages:");
			for (int num = 0; num < result.CommandMessages.Count; num++)
			{
				console.WriteLine(result.CommandMessages[num]);
			}
		}
		console.Write("Command ");
		console.Write(result.Success ? "successfully executed" : "failed during execution");
		console.WriteLine(", execution time = {0}", result.ExecutionTime);
	}

	private void FormatStack(Stack<string> stack, IConsole console)
	{
		int count = stack.Count;
		foreach (string item in stack)
		{
			console.WriteLine("{0}: {1}", count--, item);
		}
	}

	private bool ProcessJob(HardwareJob job, IConsole console, out ProgramResult[] programResults)
	{
		programResults = new ProgramResult[0];
		console.WriteLine("Job '{0}' successfully scheduled, resuming...", job.ID);
		job.Connect();
		HardwareCommandResult hardwareCommandResult = job.Resume();
		ShowResults(hardwareCommandResult, console);
		if (!hardwareCommandResult.Success)
		{
			return false;
		}
		console.WriteLine("Waiting for job '{0}' to complete...", job.ID);
		Helper.WaitForJob(job, out var _);
		job.Disconnect();
		ShowResults(hardwareCommandResult, console);
		if (!hardwareCommandResult.Success)
		{
			return false;
		}
		console.WriteLine("-----------------------------------------------------");
		console.WriteLine("Job results:");
		hardwareCommandResult = job.GetErrors(out var errors);
		ShowResults(hardwareCommandResult, console);
		if (!hardwareCommandResult.Success)
		{
			return false;
		}
		errors.ForEach(delegate(Error each)
		{
			console.WriteLine(each);
			console.WriteLine("Details: {0}\n", each.Details);
		});
		if (errors.ContainsError())
		{
			return false;
		}
		hardwareCommandResult = job.GetStack(out var stack);
		ShowResults(hardwareCommandResult, console);
		if (!hardwareCommandResult.Success)
		{
			return false;
		}
		hardwareCommandResult = job.GetResults(out programResults);
		ShowResults(hardwareCommandResult, console);
		if (!hardwareCommandResult.Success)
		{
			return false;
		}
		Array.ForEach(programResults, delegate(ProgramResult each)
		{
			console.WriteLine(each);
		});
		FormatStack(stack, console);
		return true;
	}

	private void ExecuteScriptCommands(string file)
	{
		using TextReader textReader = new StreamReader(file);
		while (true)
		{
			string text = textReader.ReadLine();
			if (text == null)
			{
				break;
			}
			ExecuteStatement(text);
		}
	}

	private bool ExecuteStatement(string statement)
	{
		if (string.IsNullOrEmpty(statement))
		{
			return true;
		}
		if (statement.StartsWith("EXIT", StringComparison.InvariantCultureIgnoreCase))
		{
			return false;
		}
		ImmediateCommand command = ImmediateCommand.GetCommand(statement);
		if (command == null)
		{
			return true;
		}
		ImmediateCommandResult immediateCommandResult = command.Execute();
		if (immediateCommandResult == null)
		{
			return true;
		}
		ShowResults(immediateCommandResult.CommandResult, m_console);
		if (immediateCommandResult.Message != null)
		{
			m_console.Write(immediateCommandResult.Message);
		}
		return true;
	}
}
