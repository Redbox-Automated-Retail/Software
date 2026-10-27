using System;
using System.Collections.Generic;
using Redbox.Core;
using Redbox.IPC.Framework;
using Redbox.UpdateManager.ComponentModel;

namespace Redbox.UpdateManager.Command;

[Command("schedule-event")]
internal class ScheduleEventCommand
{
	[CommandForm(Name = "list")]
	[Usage("SCHEDULE-EVENT list")]
	public void List(CommandContext context)
	{
		ITaskScheduler service = ServiceLocator.Instance.GetService<ITaskScheduler>();
		context.Errors.AddRange(ConvertToIPCErrors(service.List(out var tasks)));
		if (!context.Errors.ContainsError())
		{
			context.Messages.Add(tasks.ToJson());
		}
	}

	[CommandForm(Name = "isRunning")]
	[Usage("SCHEDULE-EVENT isRunning")]
	public void IsRunning(CommandContext context)
	{
		ITaskScheduler service = ServiceLocator.Instance.GetService<ITaskScheduler>();
		context.Messages.Add(service.IsRunning.ToJson());
	}

	[CommandForm(Name = "create")]
	[Usage("SCHEDULE-EVENT create name: payload: payloadType:{script, shell, work} [startOffset:(sec)] [startTime:] [endTime:] [[repeatInterval:] [cronExpression:]]")]
	public void Create(CommandContext context, [CommandKeyValue(IsRequired = true)] string name, [CommandKeyValue(IsRequired = true)] string payload, [CommandKeyValue(IsRequired = true)] string payloadType, [CommandKeyValue(IsRequired = false)] int startOffset, [CommandKeyValue(IsRequired = false)] string startTime, [CommandKeyValue(IsRequired = false)] string endTime, [CommandKeyValue(IsRequired = false)] string cronExpression, [CommandKeyValue(IsRequired = false)] string repeatInterval)
	{
		ITaskScheduler service = ServiceLocator.Instance.GetService<ITaskScheduler>();
		TimeSpan? startOffset2 = null;
		if (startOffset > 0)
		{
			startOffset2 = new TimeSpan(0, 0, startOffset);
		}
		DateTime? startTime2 = null;
		if (!string.IsNullOrEmpty(startTime))
		{
			if (!DateTime.TryParse(startTime, out var result))
			{
				context.Errors.Add(Redbox.IPC.Framework.Error.NewError("C434", "Could not parse startTime.", "Please enter a time that can be parsed."));
				return;
			}
			startTime2 = result;
		}
		DateTime? endTime2 = null;
		if (!string.IsNullOrEmpty(endTime))
		{
			if (!DateTime.TryParse(endTime, out var result2))
			{
				context.Errors.Add(Redbox.IPC.Framework.Error.NewError("C433", "Could not parse endTime.", "Please enter a time that can be parsed."));
				return;
			}
			endTime2 = result2;
		}
		if ((!string.IsNullOrEmpty(repeatInterval) && !string.IsNullOrEmpty(cronExpression)) || (string.IsNullOrEmpty(repeatInterval) && string.IsNullOrEmpty(cronExpression)))
		{
			context.Errors.Add(Redbox.IPC.Framework.Error.NewError("C435", "Must specify either repeatInterval or cronExpression.", "Must specify exactly one of repeatInterval or cronExpression, not both."));
			return;
		}
		if (!string.IsNullOrEmpty(repeatInterval))
		{
			if (!TimeSpan.TryParse(repeatInterval, out var result3))
			{
				context.Errors.Add(Redbox.IPC.Framework.Error.NewError("C437", "Could not parse repeatInterval.", "Please enter a timespan that can be parsed."));
				return;
			}
			context.Errors.AddRange(ConvertToIPCErrors(service.ScheduleSimpleJob(name, payload, payloadType, startOffset2, startTime2, endTime2, result3)));
		}
		else
		{
			context.Errors.AddRange(ConvertToIPCErrors(service.ScheduleCronJob(name, payload, payloadType, startOffset2, startTime2, endTime2, cronExpression)));
		}
		if (!context.Errors.ContainsError())
		{
			service.Restart();
			context.Messages.Add("Success");
		}
	}

	[CommandForm(Name = "delete")]
	[Usage("SCHEDULE-EVENT delete name:")]
	public void Delete(CommandContext context, [CommandKeyValue(IsRequired = true)] string name)
	{
		ITaskScheduler service = ServiceLocator.Instance.GetService<ITaskScheduler>();
		context.Errors.AddRange(ConvertToIPCErrors(service.Delete(name)));
	}

	[CommandForm(Name = "clear")]
	[Usage("SCHEDULE-EVENT clear")]
	public void Delete(CommandContext context)
	{
		ITaskScheduler service = ServiceLocator.Instance.GetService<ITaskScheduler>();
		context.Errors.AddRange(ConvertToIPCErrors(service.Clear()));
	}

	[CommandForm(Name = "stop")]
	[Usage("SCHEDULE-EVENT stop")]
	public void Stop(CommandContext context)
	{
		ITaskScheduler service = ServiceLocator.Instance.GetService<ITaskScheduler>();
		context.Errors.AddRange(ConvertToIPCErrors(service.Stop()));
	}

	[CommandForm(Name = "start")]
	[Usage("SCHEDULE-EVENT start")]
	public void Start(CommandContext context)
	{
		ITaskScheduler service = ServiceLocator.Instance.GetService<ITaskScheduler>();
		context.Errors.AddRange(ConvertToIPCErrors(service.Start()));
	}

	[CommandForm(Name = "restart")]
	[Usage("SCHEDULE-EVENT restart")]
	public void Restart(CommandContext context)
	{
		ITaskScheduler service = ServiceLocator.Instance.GetService<ITaskScheduler>();
		context.Errors.AddRange(ConvertToIPCErrors(service.Restart()));
	}

	[CommandForm(Name = "force")]
	[Usage("SCHEDULE-EVENT force name:")]
	public void Force(CommandContext context, string name)
	{
		ITaskScheduler service = ServiceLocator.Instance.GetService<ITaskScheduler>();
		context.Errors.AddRange(ConvertToIPCErrors(service.ForceTask(name, out var success)));
		if (!context.Errors.ContainsError())
		{
			context.Messages.Add(success.ToJson());
		}
	}

	private static IEnumerable<Redbox.IPC.Framework.Error> ConvertToIPCErrors(List<Redbox.UpdateManager.ComponentModel.Error> errors)
	{
		Redbox.IPC.Framework.ErrorList ipcErrors = new Redbox.IPC.Framework.ErrorList();
		errors.ForEach(delegate(Redbox.UpdateManager.ComponentModel.Error e)
		{
			ipcErrors.Add(e.IsWarning ? Redbox.IPC.Framework.Error.NewWarning(e.Code, e.Description, e.Details) : Redbox.IPC.Framework.Error.NewError(e.Code, e.Description, e.Details));
		});
		return ipcErrors;
	}
}
