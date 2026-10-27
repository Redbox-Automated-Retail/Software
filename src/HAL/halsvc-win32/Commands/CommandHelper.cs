using Redbox.HAL.Component.Model;
using Redbox.HAL.IPC.Framework;

namespace Redbox.HAL.Service.Win32.Commands;

public static class CommandHelper
{
	public static void FormatJob(IExecutionContext context, CommandContext result)
	{
		result.Messages.Add(string.Format("{0}|{1}|{2}|{3}|{4}|{5}|{6}|{7}|0|0", context.ID, context.Label ?? "(not labeled)", context.ProgramName, context.Priority, context.StartTime.HasValue ? context.StartTime.ToString() : "N/A", context.GetStatus(), context.Result.ExecutionTime.HasValue ? context.Result.ExecutionTime.ToString() : "Not Started", (!context.IsConnected) ? ConnectionState.Disconnected : ConnectionState.Connected));
	}

	public static IExecutionContext GetJob(string jobId, ErrorList errors)
	{
		if (string.IsNullOrEmpty(jobId))
		{
			errors.Add(Error.NewError("S001", "The job: parameter is required.", "Correct your command and resubmit."));
			return null;
		}
		IExecutionContext job = ServiceLocator.Instance.GetService<IExecutionService>().GetJob(jobId);
		if (job == null)
		{
			errors.Add(Error.NewError("S001", $"The job for ID '{jobId}' does not exist.", "Correct your command and resubmit."));
		}
		return job;
	}
}
