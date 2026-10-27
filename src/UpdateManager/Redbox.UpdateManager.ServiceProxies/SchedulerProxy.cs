using System;
using System.Collections.Generic;
using System.Text;
using Redbox.Core;
using Redbox.IPC.Framework;
using Redbox.UpdateManager.ComponentModel;

namespace Redbox.UpdateManager.ServiceProxies;

public class SchedulerProxy : ITaskScheduler
{
	private string m_url;

	public static SchedulerProxy Instance => Singleton<SchedulerProxy>.Instance;

	public bool IsRunning
	{
		get
		{
			bool result = false;
			ClientCommandResult clientCommandResult = ExecuteCommandStringWithReturnValue("schedule-event isRunning");
			if (!clientCommandResult.Errors.ContainsError())
			{
				result = clientCommandResult.CommandMessages[0].ToObject<bool>();
			}
			return result;
		}
	}

	public void Initialize(string url)
	{
		m_url = url;
		ServiceLocator.Instance.AddService(typeof(ITaskScheduler), this);
	}

	public Redbox.UpdateManager.ComponentModel.ErrorList Stop()
	{
		return ExecuteCommandString("schedule-event stop");
	}

	public Redbox.UpdateManager.ComponentModel.ErrorList Start()
	{
		return ExecuteCommandString("schedule-event start");
	}

	public Redbox.UpdateManager.ComponentModel.ErrorList Restart()
	{
		return ExecuteCommandString("schedule-event restart");
	}

	public Redbox.UpdateManager.ComponentModel.ErrorList Delete(string name)
	{
		string command = $"SCHEDULE-EVENT delete name:'{name}'";
		return ExecuteCommandString(command);
	}

	public Redbox.UpdateManager.ComponentModel.ErrorList Clear()
	{
		return ExecuteCommandString("SCHEDULE-EVENT clear");
	}

	public Redbox.UpdateManager.ComponentModel.ErrorList List(out List<ITask> tasks)
	{
		tasks = new List<ITask>();
		ClientCommandResult clientCommandResult = ExecuteCommandStringWithReturnValue("schedule-event list");
		if (clientCommandResult.Success)
		{
			foreach (Task item in clientCommandResult.CommandMessages[0].ToObject<List<Task>>())
			{
				tasks.Add(item);
			}
		}
		return ConvertToComponentErrors(clientCommandResult.Errors);
	}

	public Redbox.UpdateManager.ComponentModel.ErrorList ForceTask(string name, out bool success)
	{
		success = false;
		string command = $"SCHEDULE-EVENT force name:'{name}'";
		ClientCommandResult clientCommandResult = ExecuteCommandStringWithReturnValue(command);
		if (clientCommandResult.Success)
		{
			success = clientCommandResult.CommandMessages[0].ToObject<bool>();
		}
		return ConvertToComponentErrors(clientCommandResult.Errors);
	}

	public Redbox.UpdateManager.ComponentModel.ErrorList ScheduleCronJob(string name, string payload, string payloadTypeString, TimeSpan? startOffset, DateTime? startTime, DateTime? endTime, string cronExpression)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendFormat("SCHEDULE-EVENT create name:'{0}' payload:'{1}' payloadType:'{2}'  cronExpression:'{3}'", name, payload, payloadTypeString, cronExpression);
		if (startOffset.HasValue)
		{
			stringBuilder.AppendFormat(" startOffset:'{0}'", startOffset.Value.TotalSeconds);
		}
		if (startTime.HasValue)
		{
			stringBuilder.AppendFormat(" startTime:'{0}'", startTime.Value);
		}
		if (endTime.HasValue)
		{
			stringBuilder.AppendFormat(" endTime:'{0}'", endTime.Value);
		}
		return ConvertToComponentErrors(ExecuteCommandStringWithReturnValue(stringBuilder.ToString()).Errors);
	}

	public Redbox.UpdateManager.ComponentModel.ErrorList ScheduleCronJob(string name, string payload, PayloadType payloadType, TimeSpan? startOffset, DateTime? startTime, DateTime? endTime, string cronExpression)
	{
		return ScheduleCronJob(name, payload, Enum.GetName(typeof(PayloadType), payloadType), startOffset, startTime, endTime, cronExpression);
	}

	public Redbox.UpdateManager.ComponentModel.ErrorList ScheduleSimpleJob(string name, string payload, string payloadTypeString, TimeSpan? startOffset, DateTime? startTime, DateTime? endTime, TimeSpan repeatInterval)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendFormat("SCHEDULE-EVENT create name:'{0}' payload:'{1}' payloadType:'{2}' repeatInterval:'{3}'", name, payload, payloadTypeString, repeatInterval);
		if (startOffset.HasValue)
		{
			stringBuilder.AppendFormat(" startOffset:'{0}'", startOffset.Value.TotalSeconds);
		}
		if (startTime.HasValue)
		{
			stringBuilder.AppendFormat(" startTime:'{0}'", startTime.Value);
		}
		if (endTime.HasValue)
		{
			stringBuilder.AppendFormat(" endTime:'{0}'", endTime.Value);
		}
		return ConvertToComponentErrors(ExecuteCommandStringWithReturnValue(stringBuilder.ToString()).Errors);
	}

	public Redbox.UpdateManager.ComponentModel.ErrorList ScheduleSimpleJob(string name, string payload, PayloadType payloadType, TimeSpan? startOffset, DateTime? startTime, DateTime? endTime, TimeSpan repeatInterval)
	{
		return ScheduleSimpleJob(name, payload, Enum.GetName(typeof(PayloadType), payloadType), startOffset, startTime, endTime, repeatInterval);
	}

	private SchedulerProxy()
	{
	}

	private Redbox.UpdateManager.ComponentModel.ErrorList ExecuteCommandString(string command)
	{
		using UpdateManagerService updateManagerService = UpdateManagerService.GetService(m_url);
		return ConvertToComponentErrors(updateManagerService.ExecuteCommandString(command).Errors);
	}

	private ClientCommandResult ExecuteCommandStringWithReturnValue(string command)
	{
		using UpdateManagerService updateManagerService = UpdateManagerService.GetService(m_url);
		return updateManagerService.ExecuteCommandString(command);
	}

	private static Redbox.UpdateManager.ComponentModel.ErrorList ConvertToComponentErrors(List<Redbox.IPC.Framework.Error> ipcErrors)
	{
		Redbox.UpdateManager.ComponentModel.ErrorList errors = new Redbox.UpdateManager.ComponentModel.ErrorList();
		ipcErrors.ForEach(delegate(Redbox.IPC.Framework.Error e)
		{
			errors.Add(e.IsWarning ? Redbox.UpdateManager.ComponentModel.Error.NewWarning(e.Code, e.Description, e.Details) : Redbox.UpdateManager.ComponentModel.Error.NewError(e.Code, e.Description, e.Details));
		});
		return errors;
	}
}
