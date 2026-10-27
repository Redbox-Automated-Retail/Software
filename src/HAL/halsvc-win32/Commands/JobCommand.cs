using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Component.Model.Attributes;
using Redbox.HAL.IPC.Framework;
using Redbox.IPC.Framework;

namespace Redbox.HAL.Service.Win32.Commands;

[Command("job")]
public sealed class JobCommand
{
	[CommandForm(Name = "get")]
	[Usage("JOB get job: job-id")]
	public void Get(CommandContext context, [CommandKeyValue(IsRequired = true, KeyName = "job")] string jobId)
	{
		IExecutionContext job = CommandHelper.GetJob(jobId, context.Errors);
		if (job != null)
		{
			CommandHelper.FormatJob(job, context);
		}
	}

	[CommandForm(Name = "list")]
	[Usage("JOB list")]
	public void List(CommandContext context)
	{
		List<IExecutionContext> jobList = ServiceLocator.Instance.GetService<IExecutionService>().GetJobList();
		using (new DisposeableList<IExecutionContext>(jobList))
		{
			jobList.Sort((IExecutionContext x, IExecutionContext y) => x.Priority.CompareTo(y.Priority));
			jobList.ForEach(delegate(IExecutionContext each)
			{
				CommandHelper.FormatJob(each, context);
			});
		}
	}

	[CommandForm(Name = "set-label")]
	[Usage("JOB set-label job: job-id value: label")]
	public void SetLabel(CommandContext context, [CommandKeyValue(IsRequired = true, KeyName = "job")] string jobId, string value)
	{
		IExecutionContext job = CommandHelper.GetJob(jobId, context.Errors);
		if (job != null)
		{
			job.Label = value;
		}
	}

	[CommandForm(Name = "get-errors")]
	[Usage("JOB get-errors job: job-id")]
	public void GetErrors(CommandContext context, [CommandKeyValue(IsRequired = true, KeyName = "job")] string jobId)
	{
		IExecutionContext job = CommandHelper.GetJob(jobId, context.Errors);
		if (job != null)
		{
			ProtocolHelper.FormatErrors(job.Result.Errors, context);
		}
	}

	[CommandForm(Name = "get-messages")]
	[Usage("JOB get-messages job: job-id")]
	public void GetMessages(CommandContext context, [CommandKeyValue(IsRequired = true, KeyName = "job")] string jobId)
	{
		CommandHelper.GetJob(jobId, context.Errors)?.ForeachMessage(delegate(string msg)
		{
			ProtocolHelper.FormatEventMessage(msg, context);
		});
	}

	[CommandForm(Name = "get-results")]
	[Usage("JOB get-results job: job-id")]
	public void GetResults(CommandContext context, [CommandKeyValue(IsRequired = true, KeyName = "job")] string jobId)
	{
		CommandHelper.GetJob(jobId, context.Errors)?.ForeachResult(delegate(string result)
		{
			ProtocolHelper.FormatEventMessage(result, context);
		});
	}

	[CommandForm(Name = "disconnect")]
	[Usage("JOB disconnect job: job-id")]
	public void Disconnect(CommandContext context, [CommandKeyValue(IsRequired = true, KeyName = "job")] string jobId)
	{
		CommandHelper.GetJob(jobId, context.Errors)?.Disconnect();
	}

	[CommandForm(Name = "connect")]
	[Usage("JOB connect job: job-id")]
	public void Connect(CommandContext context, [CommandKeyValue(IsRequired = true, KeyName = "job")] string jobId)
	{
		IExecutionContext executionContext = CommandHelper.GetJob(jobId, context.Errors);
		if (executionContext != null)
		{
			executionContext.Connect(context.MessageSink);
			context.Session.Disconnect += delegate
			{
				executionContext.Disconnect();
			};
		}
	}

	[CommandForm(Name = "pend")]
	[Usage("JOB pend job: job-id")]
	public void Pend(CommandContext context, [CommandKeyValue(IsRequired = true, KeyName = "job")] string jobId)
	{
		CommandHelper.GetJob(jobId, context.Errors)?.Pend();
	}

	[CommandForm(Name = "signal")]
	[Usage("JOB signal job: job-id value: signalValue")]
	public void Signal(CommandContext context, [CommandKeyValue(IsRequired = true, KeyName = "job")] string jobId, string value)
	{
		CommandHelper.GetJob(jobId, context.Errors)?.Signal(value);
	}

	[CommandForm(Name = "trash")]
	[Usage("JOB trash job: job-id")]
	public void Trash(CommandContext context, [CommandKeyValue(IsRequired = true, KeyName = "job")] string jobId)
	{
		CommandHelper.GetJob(jobId, context.Errors)?.Trash();
	}

	[CommandForm(Name = "resume")]
	[Usage("JOB resume job: job-id")]
	public void Resume(CommandContext context, [CommandKeyValue(IsRequired = true, KeyName = "job")] string jobId)
	{
		CommandHelper.GetJob(jobId, context.Errors)?.Resume();
	}

	[CommandForm(Name = "suspend")]
	[Usage("JOB suspend job: job-id")]
	public void Suspend(CommandContext context, [CommandKeyValue(IsRequired = true, KeyName = "job")] string jobId)
	{
		CommandHelper.GetJob(jobId, context.Errors)?.Suspend();
	}

	[CommandForm(Name = "suspend-all")]
	[Usage("JOB suspend-all")]
	public void SuspendAll(CommandContext context)
	{
		context.Errors.Add(Error.NewError("C999", "Unsupported Client API", "The SuspendAll client API is no longer supported."));
	}

	[CommandForm(Name = "resume-all")]
	[Usage("JOB resume-all")]
	public void ResumeAll(CommandContext context)
	{
		context.Errors.Add(Error.NewError("C999", "Unsupported Client API", "The ResumeAll client API is no longer supported."));
	}

	[CommandForm(Name = "terminate")]
	[Usage("JOB terminate job: job-id")]
	public void Terminate(CommandContext context, [CommandKeyValue(IsRequired = true, KeyName = "job")] string jobId)
	{
		CommandHelper.GetJob(jobId, context.Errors)?.Terminate();
	}

	[CommandForm(Name = "set-priority")]
	[Usage("JOB set-priority job: job-id value: priority")]
	public void SetPriority(CommandContext context, [CommandKeyValue(IsRequired = true, KeyName = "job")] string jobId, ProgramPriority value)
	{
		IExecutionContext job = CommandHelper.GetJob(jobId, context.Errors);
		if (job != null)
		{
			job.Priority = value;
		}
	}

	[CommandForm(Name = "set-start-time")]
	[Usage("JOB set-start-time job: job-id value: startTime")]
	public void SetStartTime(CommandContext context, [CommandKeyValue(IsRequired = true, KeyName = "job")] string jobId, DateTime value)
	{
		IExecutionContext job = CommandHelper.GetJob(jobId, context.Errors);
		if (job != null)
		{
			job.StartTime = value;
		}
	}

	[CommandForm(Name = "collect-garbage")]
	[Usage("JOB collect-garbage force: true|false")]
	public void CollectGarbage(CommandContext context, [CommandKeyValue(IsRequired = true, KeyName = "force")] bool force)
	{
		ServiceLocator.Instance.GetService<IExecutionService>().CleanupJobs(force);
	}

	[CommandForm(Name = "scheduler-status")]
	[Usage("JOB scheduler-status")]
	public void GetSchedulerStatus(CommandContext context)
	{
		IExecutionContext activeContext = ServiceLocator.Instance.GetService<IExecutionService>().GetActiveContext();
		context.Messages.Add((activeContext != null) ? activeContext.ID : "idle");
	}

	[CommandForm(Name = "wait-for-completion")]
	[Usage("JOB wait-for-completion job: job-id")]
	public void WaitForCompletion(CommandContext context, [CommandKeyValue(IsRequired = true, KeyName = "job")] string jobId)
	{
		CommandHelper.GetJob(jobId, context.Errors)?.WaitForCompletion();
	}

	[CommandForm(Name = "schedule")]
	[Usage("JOB schedule name: program-name [debugging: flag] [priority: priority] [startTime: startTime] [label: label] [resume-now] [no-duplicates]")]
	public void Schedule(CommandContext context, [CommandKeyValue(IsRequired = true)] string name, bool? debugging, ProgramPriority? priority, DateTime? startTime, string label)
	{
		bool flag = context.Parameters.ContainsKey("resume-now");
		bool num = context.Parameters.ContainsKey("no-duplicates");
		IExecutionService service = ServiceLocator.Instance.GetService<IExecutionService>();
		if (num && label != null)
		{
			IExecutionContext[] byLabel = service.GetByLabel(label);
			if (byLabel.Length != 0)
			{
				context.Errors.Add(Error.NewWarning("S998", $"A job with the label '{label}' already exists and the no-duplicates flag was specified.", "Issue the JOB schedule command again passing in a non-duplicate label: or remove the no-duplicates parameter."));
				if (flag)
				{
					Array.ForEach(byLabel, delegate(IExecutionContext each)
					{
						each.Resurrect();
					});
				}
				return;
			}
		}
		IExecutionContext executionContext = ScheduleJob(name, label, startTime, priority ?? ProgramPriority.Normal, debugging == true);
		context.Errors.AddRange(executionContext.Result.Errors);
		if (!context.Errors.ContainsError() & flag)
		{
			executionContext.Resume();
		}
		CommandHelper.FormatJob(executionContext, context);
	}

	[CommandForm(Name = "execute-immediate")]
	[Usage("JOB execute-immediate statement: statement-line")]
	public void ExecuteImmediate([CommandKeyValue(IsRequired = true)] CommandContext context, string statement)
	{
		OnExecuteImmediate(Encoding.ASCII.GetBytes(statement), context);
	}

	[CommandForm(Name = "execute-immediate-base64")]
	[Usage("JOB execute-immediate-base64 statement: base64-encoded-statement")]
	public void ExecuteImmediateBase64(CommandContext context, [CommandKeyValue(IsRequired = true, BinaryEncoding = BinaryEncoding.Base64)] byte[] statement)
	{
		OnExecuteImmediate(statement, context);
	}

	[CommandForm(Name = "init-status")]
	[Usage("JOB init-status")]
	public void StartupInitSucceeded(CommandContext context)
	{
		IExecutionService service = ServiceLocator.Instance.GetService<IExecutionService>();
		context.Messages.Add(service.GetInitJobStatus().ToString().ToUpper());
	}

	[CommandForm(Name = "quick-return-status")]
	[Usage("JOB quick-return-status")]
	public void QuickReturnIsRunning(CommandContext context)
	{
		context.Messages.Add("INACTIVE");
	}

	private static IExecutionContext ScheduleJob(string programName, string label, DateTime? startTime, ProgramPriority priority, bool enableDebugging)
	{
		return ServiceLocator.Instance.GetService<IExecutionService>().ScheduleJob(programName, label, startTime, priority);
	}

	private static void OnExecuteImmediate(byte[] statement, CommandContext context)
	{
		IExecutionService service = ServiceLocator.Instance.GetService<IExecutionService>();
		if (service.IsImmediateJobPending)
		{
			context.Errors.Add(Error.NewError("S001", "An immediate command is already scheduled to execute.  Only one immediate job can be scheduled at a time.", "Use the JOB wait-for-completion command and then resubmit the immediate job."));
			return;
		}
		using MemoryStream stream = new MemoryStream(statement);
		IExecutionResult executionResult = service.CompileImmediate(stream);
		if (executionResult.Errors.ContainsError())
		{
			context.Errors.AddRange(executionResult.Errors);
			return;
		}
		IExecutionContext executionContext = service.ExecuteImmediate();
		executionContext.WaitForCompletion();
		CommandHelper.FormatJob(executionContext, context);
		context.Errors.AddRange(executionContext.Result.Errors);
	}
}
