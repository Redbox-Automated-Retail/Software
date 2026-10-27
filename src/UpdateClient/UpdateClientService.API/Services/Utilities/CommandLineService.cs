using System;
using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Redbox.NetCore.Logging.Extensions;

namespace UpdateClientService.API.Services.Utilities;

public class CommandLineService : ICommandLineService
{
	private readonly ILogger<CommandLineService> _logger;

	public CommandLineService(ILogger<CommandLineService> logger)
	{
		_logger = logger;
	}

	public bool TryExecutePowerShellScriptFromFile(string path)
	{
		Process process = new Process
		{
			StartInfo = new ProcessStartInfo
			{
				UseShellExecute = false,
				CreateNoWindow = true,
				WindowStyle = ProcessWindowStyle.Hidden,
				FileName = "powershell",
				Arguments = "-executionpolicy bypass -file \"" + path + "\""
			}
		};
		return Execute(process);
	}

	public bool TryExecutePowerShellScript(string script, TimeSpan? timeout = null)
	{
		Process process = new Process
		{
			StartInfo = new ProcessStartInfo
			{
				UseShellExecute = false,
				CreateNoWindow = true,
				WindowStyle = ProcessWindowStyle.Hidden,
				FileName = "powershell",
				Arguments = script
			}
		};
		return Execute(process, timeout);
	}

	public bool TryExecuteCommand(string fileName, string arguments)
	{
		Process process = new Process
		{
			StartInfo = new ProcessStartInfo
			{
				UseShellExecute = false,
				CreateNoWindow = true,
				WindowStyle = ProcessWindowStyle.Hidden,
				FileName = fileName,
				Arguments = arguments
			}
		};
		return Execute(process);
	}

	private bool Execute(Process process, TimeSpan? timeout = null)
	{
		process.StartInfo.RedirectStandardOutput = true;
		process.StartInfo.RedirectStandardError = true;
		process.Start();
		string text = process.StandardOutput.ReadToEnd();
		string text2 = process.StandardError.ReadToEnd();
		if (!process.WaitForExit((int)(timeout?.TotalMilliseconds ?? TimeSpan.FromMinutes(5.0).TotalMilliseconds)))
		{
			text2 += $"Process did not complete within {timeout}.";
		}
		if (!string.IsNullOrWhiteSpace(text))
		{
			_logger.LogInfoWithSource(text, "Execute", "/sln/src/UpdateClientService.API/Services/Utilities/CommandLineService.cs");
		}
		if (!string.IsNullOrWhiteSpace(text2))
		{
			_logger.LogErrorWithSource("An error occurred while running a command. Error: " + text2, "Execute", "/sln/src/UpdateClientService.API/Services/Utilities/CommandLineService.cs");
		}
		return string.IsNullOrWhiteSpace(text2);
	}
}
