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
		ITaskScheduler taskScheduler = ServiceLocator.Instance.GetService<ITaskScheduler>();
		context.Errors.AddRange(ConvertToIPCErrors(taskScheduler.List(out var tasks)));
		if (!context.Errors.ContainsError())
		{
			context.Messages.Add(tasks.ToJson());
		}
	}

	[CommandForm(Name = "isRunning")]
	[Usage("SCHEDULE-EVENT isRunning")]
	public void IsRunning(CommandContext context)
	{
		ITaskScheduler taskScheduler = ServiceLocator.Instance.GetService<ITaskScheduler>();
		context.Messages.Add(taskScheduler.IsRunning.ToJson());
	}

	[CommandForm(Name = "create")]
	[Usage("SCHEDULE-EVENT create name: payload: payloadType:{script, shell, work} [startOffset:(sec)] [startTime:] [endTime:] [[repeatInterval:] [cronExpression:]]")]
	public void Create(CommandContext context, [CommandKeyValue(IsRequired = true)] string name, [CommandKeyValue(IsRequired = true)] string payload, [CommandKeyValue(IsRequired = true)] string payloadType, [CommandKeyValue(IsRequired = false)] int startOffset, [CommandKeyValue(IsRequired = false)] string startTime, [CommandKeyValue(IsRequired = false)] string endTime, [CommandKeyValue(IsRequired = false)] string cronExpression, [CommandKeyValue(IsRequired = false)] string repeatInterval)
	{
		ITaskScheduler taskScheduler = ServiceLocator.Instance.GetService<ITaskScheduler>();
		TimeSpan? offset = null;
		if (startOffset > 0)
		{
			offset = new TimeSpan(0, 0, startOffset);
		}
		DateTime? startAt = null;
		if (!string.IsNullOrEmpty(startTime))
		{
			if (!DateTime.TryParse(startTime, out var parsedStart))
			{
				context.Errors.Add(Redbox.IPC.Framework.Error.NewError("C434", "Could not parse startTime.", "Please enter a time that can be parsed."));
				return;
			}
			startAt = parsedStart;
		}
		DateTime? endAt = null;
		if (!string.IsNullOrEmpty(endTime))
		{
			if (!DateTime.TryParse(endTime, out var parsedEnd))
			{
				context.Errors.Add(Redbox.IPC.Framework.Error.NewError("C433", "Could not parse endTime.", "Please enter a time that can be parsed."));
				return;
			}
			endAt = parsedEnd;
		}
		if ((!string.IsNullOrEmpty(repeatInterval) && !string.IsNullOrEmpty(cronExpression)) || (string.IsNullOrEmpty(repeatInterval) && string.IsNullOrEmpty(cronExpression)))
		{
			context.Errors.Add(Redbox.IPC.Framework.Error.NewError("C435", "Must specify either repeatInterval or cronExpression.", "Must specify exactly one of repeatInterval or cronExpression, not both."));
			return;
		}
		if (!string.IsNullOrEmpty(repeatInterval))
		{
			if (!TimeSpan.TryParse(repeatInterval, out var interval))
			{
				context.Errors.Add(Redbox.IPC.Framework.Error.NewError("C437", "Could not parse repeatInterval.", "Please enter a timespan that can be parsed."));
				return;
			}
			context.Errors.AddRange(ConvertToIPCErrors(taskScheduler.ScheduleSimpleJob(name, payload, payloadType, offset, startAt, endAt, interval)));
		}
		else
		{
			context.Errors.AddRange(ConvertToIPCErrors(taskScheduler.ScheduleCronJob(name, payload, payloadType, offset, startAt, endAt, cronExpression)));
		}
		if (!context.Errors.ContainsError())
		{
			taskScheduler.Restart();
			context.Messages.Add("Success");
		}
	}

	[CommandForm(Name = "delete")]
	[Usage("SCHEDULE-EVENT delete name:")]
	public void Delete(CommandContext context, [CommandKeyValue(IsRequired = true)] string name)
	{
		ITaskScheduler taskScheduler = ServiceLocator.Instance.GetService<ITaskScheduler>();
		context.Errors.AddRange(ConvertToIPCErrors(taskScheduler.Delete(name)));
	}

	[CommandForm(Name = "clear")]
	[Usage("SCHEDULE-EVENT clear")]
	public void Delete(CommandContext context)
	{
		ITaskScheduler taskScheduler = ServiceLocator.Instance.GetService<ITaskScheduler>();
		context.Errors.AddRange(ConvertToIPCErrors(taskScheduler.Clear()));
	}

	[CommandForm(Name = "stop")]
	[Usage("SCHEDULE-EVENT stop")]
	public void Stop(CommandContext context)
	{
		ITaskScheduler taskScheduler = ServiceLocator.Instance.GetService<ITaskScheduler>();
		context.Errors.AddRange(ConvertToIPCErrors(taskScheduler.Stop()));
	}

	[CommandForm(Name = "start")]
	[Usage("SCHEDULE-EVENT start")]
	public void Start(CommandContext context)
	{
		ITaskScheduler taskScheduler = ServiceLocator.Instance.GetService<ITaskScheduler>();
		context.Errors.AddRange(ConvertToIPCErrors(taskScheduler.Start()));
	}

	[CommandForm(Name = "restart")]
	[Usage("SCHEDULE-EVENT restart")]
	public void Restart(CommandContext context)
	{
		ITaskScheduler taskScheduler = ServiceLocator.Instance.GetService<ITaskScheduler>();
		context.Errors.AddRange(ConvertToIPCErrors(taskScheduler.Restart()));
	}

	[CommandForm(Name = "force")]
	[Usage("SCHEDULE-EVENT force name:")]
	public void Force(CommandContext context, string name)
	{
		ITaskScheduler taskScheduler = ServiceLocator.Instance.GetService<ITaskScheduler>();
		context.Errors.AddRange(ConvertToIPCErrors(taskScheduler.ForceTask(name, out var success)));
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
