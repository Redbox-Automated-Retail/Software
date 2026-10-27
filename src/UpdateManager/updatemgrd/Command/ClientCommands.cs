using Redbox.Core;
using Redbox.IPC.Framework;
using Redbox.UpdateManager.ComponentModel;
using Redbox.UpdateManager.Environment;
using Redbox.UpdateService.Model;

namespace Redbox.UpdateManager.Command;

[Command("client")]
internal class ClientCommands
{
	[CommandForm(Name = "poll")]
	[Usage("client poll")]
	public void Poll(CommandContext context)
	{
		IUpdateService service = ServiceLocator.Instance.GetService<IUpdateService>();
		if (service != null)
		{
			context.Errors.AddRange(service.Poll().ToIPCErrors());
		}
	}

	[CommandForm(Name = "serverpoll")]
	[Usage("client serverpoll")]
	public void ServerPoll(CommandContext context)
	{
		IUpdateService service = ServiceLocator.Instance.GetService<IUpdateService>();
		if (service != null)
		{
			context.Errors.AddRange(service.ServerPoll().ToIPCErrors());
		}
	}

	[CommandForm(Name = "execute-script")]
	[Usage("client execute-script script: [reset:]")]
	public void ExecuteScript(CommandContext context, [CommandKeyValue(IsRequired = true)] string script, [CommandKeyValue(IsRequired = true)] bool? reset)
	{
		IUpdateService service = ServiceLocator.Instance.GetService<IUpdateService>();
		if (service == null)
		{
			return;
		}
		if (string.IsNullOrEmpty(script))
		{
			context.Errors.Add(Redbox.IPC.Framework.Error.NewError("CC999", "Client Command execute-script requires the script parameter.", "Add a script parameter and retry."));
			return;
		}
		Redbox.UpdateManager.ComponentModel.ErrorList errorList = service.ExecuteScript(script, out var result, reset ?? false);
		if (errorList.ContainsError())
		{
			context.Errors.AddRange(errorList.ToIPCErrors());
		}
		else
		{
			context.Messages.Add(result);
		}
	}

	[CommandForm(Name = "execute-scriptfile")]
	[Usage("client execute-scriptfile file: [reset:]")]
	public void ExecuteScriptFile(CommandContext context, [CommandKeyValue(IsRequired = true)] string file, [CommandKeyValue(IsRequired = true)] bool? reset)
	{
		IUpdateService service = ServiceLocator.Instance.GetService<IUpdateService>();
		if (service == null)
		{
			return;
		}
		if (string.IsNullOrEmpty(file))
		{
			context.Errors.Add(Redbox.IPC.Framework.Error.NewError("CC998", "Client Command execute-script requires the file path parameter.", "Add a script path parameter and retry."));
			return;
		}
		Redbox.UpdateManager.ComponentModel.ErrorList errorList = service.ExecuteScriptFile(file, out var result, reset ?? false);
		if (errorList.ContainsError())
		{
			context.Errors.AddRange(errorList.ToIPCErrors());
		}
		else
		{
			context.Messages.Add(result);
		}
	}

	[CommandForm(Name = "force-data-files")]
	[Usage("client force-data-files")]
	public void ForceDataFiles(CommandContext context)
	{
		IUpdateService service = ServiceLocator.Instance.GetService<IUpdateService>();
		if (service != null)
		{
			context.Errors.AddRange(service.ForcePublish("dvd-data").ToIPCErrors());
		}
	}

	[CommandForm(Name = "get-dvd-data-state")]
	[Usage("client get-dvd-data-state")]
	public void GetDvdDataState(CommandContext context)
	{
		IUpdateService service = ServiceLocator.Instance.GetService<IUpdateService>();
		if (service != null)
		{
			context.Errors.AddRange(service.GetSubscriptionState("dvd-data", out var lastRun, out var state).ToIPCErrors());
			if (!context.Errors.ContainsError())
			{
				context.Messages.Add(lastRun.ToJson());
				context.Messages.Add(state.ToJson());
			}
		}
	}

	[CommandForm(Name = "list-current-downloads")]
	[Usage("client list-current-downloads")]
	public void ListCurrentDownloads(CommandContext context)
	{
		ITransferService service = ServiceLocator.Instance.GetService<ITransferService>();
		if (service != null)
		{
			context.Errors.AddRange(service.GetJobs(out var jobs, allUsers: false).ToIPCErrors());
			if (!context.Errors.ContainsError())
			{
				context.Messages.Add(jobs.ToJson());
			}
		}
	}

	[CommandForm(Name = "send-status-message")]
	[Usage("client send-status-message type:(info, warning, error) key: description: data:")]
	public void SendStatusMessage(CommandContext context, [CommandKeyValue(IsRequired = true)] string type, [CommandKeyValue(IsRequired = true)] string key, [CommandKeyValue(IsRequired = true)] string description, [CommandKeyValue(IsRequired = true)] string data)
	{
		if (!Extensions.TryParse<StatusMessage.StatusMessageType>(type, out var result, ignoreCase: true))
		{
			result = StatusMessage.StatusMessageType.Info;
			context.Messages.Add("The type " + type + " is unparsable, defaulting to 'info'");
		}
		StatusMessageService.Instance.EnqueueMessage(result, key, description, data);
	}
}
