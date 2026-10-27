using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Core;
using Redbox.HAL.IPC.Framework;

namespace Redbox.HAL.Client;

public sealed class HardwareJob
{
	private readonly HardwareService Service;

	public string ID { get; internal set; }

	public string Label { get; internal set; }

	public string ProgramName { get; internal set; }

	public DateTime? StartTime { get; internal set; }

	public bool TraceExecution { get; internal set; }

	public bool EnableDebugging { get; internal set; }

	public TimeSpan? ExecutionTime { get; internal set; }

	public bool IsLocallyConnected { get; private set; }

	public HardwareJobStatus Status { get; internal set; }

	public HardwareJobPriority Priority { get; internal set; }

	public HardwareJobConnectionState ConnectionState { get; internal set; }

	public event HardwareEvent EventRaised;

	public event HardwareStatusChangeEvent StatusChanged;

	public HardwareCommandResult Signal(string msg)
	{
		return ExecuteCommand($"JOB signal job: '{ID}' value: '{msg}'");
	}

	public HardwareCommandResult Pend()
	{
		return ExecuteCommand($"JOB pend job: '{ID}'");
	}

	public HardwareCommandResult Trash()
	{
		return ExecuteCommand($"JOB trash job: '{ID}'");
	}

	public HardwareCommandResult SetPriority(HardwareJobPriority priority)
	{
		return ExecuteCommand($"JOB set-priority job: '{ID}' value: '{priority}'");
	}

	public HardwareCommandResult SetLabel(string label)
	{
		return ExecuteCommand($"JOB set-label job: '{ID}' value: '{label}'");
	}

	public HardwareCommandResult SetStartTime(DateTime startTime)
	{
		return ExecuteCommand($"JOB set-start-time job: '{ID}' value: '{startTime}'");
	}

	public HardwareCommandResult Resume()
	{
		return ExecuteCommand($"JOB resume job: '{ID}'");
	}

	public HardwareCommandResult Suspend()
	{
		return ExecuteCommand($"JOB suspend job: '{ID}'");
	}

	public HardwareCommandResult Connect()
	{
		IIpcClientSession session = Service.GetSession();
		session.ServerEvent += delegate(string message)
		{
			string[] array = message.Split("|".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
			DateTime result2 = DateTime.Now;
			if (array.Length > 1)
			{
				DateTime.TryParse(array[0], out result2);
			}
			string text = ((array.Length > 1) ? array[1] : array[0]);
			if (text.StartsWith(">STATUS CHANGE<"))
			{
				HardwareJobStatus status = Enum<HardwareJobStatus>.ParseIgnoringCase(text.Substring(text.IndexOf("<") + 1).Trim(), HardwareJobStatus.Completed);
				if (StatusChanged != null)
				{
					StatusChanged(this, status);
				}
			}
			if (EventRaised != null)
			{
				EventRaised(this, result2, text);
			}
		};
		HardwareCommandResult result = ClientCommand<HardwareCommandResult>.ExecuteCommand(session, $"JOB connect job: '{ID}'");
		IsLocallyConnected = true;
		Thread thread = new Thread(EventPoll);
		thread.Name = $"Hardware Job {ID} Event Thread";
		thread.IsBackground = true;
		thread.Start(session);
		return result;
	}

	public HardwareCommandResult Terminate()
	{
		return ExecuteCommand($"JOB terminate job: '{ID}'");
	}

	public void Disconnect()
	{
		IsLocallyConnected = false;
	}

	public HardwareCommandResult ClearStack()
	{
		return ExecuteCommand($"STACK clear job: '{ID}'");
	}

	public HardwareCommandResult ClearSymbols()
	{
		return ExecuteCommand($"SYMBOL clear job: '{ID}'");
	}

	public HardwareCommandResult GetErrors(out ErrorList errors)
	{
		HardwareCommandResult hardwareCommandResult = ExecuteCommand($"JOB get-errors job: '{ID}'");
		errors = hardwareCommandResult.Errors;
		return hardwareCommandResult;
	}

	public HardwareCommandResult GetMessages(out string[] messages)
	{
		List<string> list = new List<string>();
		HardwareCommandResult hardwareCommandResult = ExecuteCommand($"JOB get-messages job: '{ID}'");
		if (hardwareCommandResult.Success)
		{
			foreach (string commandMessage in hardwareCommandResult.CommandMessages)
			{
				list.Add(commandMessage);
			}
		}
		messages = list.ToArray();
		return hardwareCommandResult;
	}

	public HardwareCommandResult GetResults(out ProgramResult[] results)
	{
		HardwareCommandResult hardwareCommandResult = ExecuteCommand($"JOB get-results job: '{ID}'");
		List<ProgramResult> list = new List<ProgramResult>();
		try
		{
			if (hardwareCommandResult.Success)
			{
				foreach (string commandMessage in hardwareCommandResult.CommandMessages)
				{
					ProgramResult programResult = ProgramResult.FromString(commandMessage);
					if (programResult != null)
					{
						list.Add(programResult);
					}
				}
			}
			results = list.ToArray();
			return hardwareCommandResult;
		}
		finally
		{
			list.Clear();
		}
	}

	public HardwareCommandResult RemoveResults()
	{
		return ExecuteCommand($"STACK remove-results job: '{ID}'");
	}

	public HardwareCommandResult GetStack(out Stack<string> stack)
	{
		stack = new Stack<string>();
		HardwareCommandResult hardwareCommandResult = ExecuteCommand($"STACK show job: '{ID}'");
		if (hardwareCommandResult.Success)
		{
			for (int num = hardwareCommandResult.CommandMessages.Count - 1; num >= 0; num--)
			{
				stack.Push(hardwareCommandResult.CommandMessages[num].Substring(hardwareCommandResult.CommandMessages[num].IndexOf(":") + 1).Trim());
			}
		}
		return hardwareCommandResult;
	}

	public HardwareCommandResult GetSymbols(out IDictionary<string, string> symbols)
	{
		symbols = new Dictionary<string, string>();
		HardwareCommandResult hardwareCommandResult = ExecuteCommand($"SYMBOL show job: '{ID}'");
		if (hardwareCommandResult.Success)
		{
			foreach (string commandMessage in hardwareCommandResult.CommandMessages)
			{
				string[] array = commandMessage.Split(":".ToCharArray(), 2, StringSplitOptions.RemoveEmptyEntries);
				symbols[array[0]] = ((array.Length == 2) ? array[1].Trim() : string.Empty);
			}
		}
		return hardwareCommandResult;
	}

	public HardwareCommandResult Pop<T>(out T value)
	{
		value = default(T);
		HardwareCommandResult hardwareCommandResult = ExecuteCommand($"STACK pop job: '{ID}'");
		if (hardwareCommandResult.Success)
		{
			value = (T)ConversionHelper.ChangeType(hardwareCommandResult.CommandMessages[0], typeof(T));
		}
		return hardwareCommandResult;
	}

	public HardwareCommandResult Push(params object[] values)
	{
		if (values == null)
		{
			return new HardwareCommandResult
			{
				Success = false
			};
		}
		HardwareCommandResult hardwareCommandResult = null;
		foreach (object obj in values)
		{
			object obj2 = obj;
			if (obj2 is string)
			{
				obj2 = $"\"{obj}\"";
			}
			hardwareCommandResult = ExecuteCommand($"STACK push value: '{obj2}' job: '{ID}'");
			if (!hardwareCommandResult.Success)
			{
				return hardwareCommandResult;
			}
		}
		return hardwareCommandResult;
	}

	public HardwareCommandResult SetDebugState(bool enabled)
	{
		return ExecuteCommand($"DEBUG set job: '{ID}' enabled: {enabled}");
	}

	public HardwareCommandResult GetDebugState(out bool debuggerEnabled)
	{
		debuggerEnabled = false;
		HardwareCommandResult hardwareCommandResult = ExecuteCommand($"DEBUG set job: '{ID}'");
		if (hardwareCommandResult.Success && bool.TryParse(hardwareCommandResult.CommandMessages[0], out var result))
		{
			debuggerEnabled = result;
		}
		return hardwareCommandResult;
	}

	public HardwareCommandResult SetDebugTrace(bool enabled)
	{
		return ExecuteCommand($"DEBUG trace job: '{ID}' enabled: {enabled}");
	}

	public HardwareCommandResult GetDebugTrace(out bool traceEnabled)
	{
		traceEnabled = false;
		HardwareCommandResult hardwareCommandResult = ExecuteCommand($"DEBUG trace job: '{ID}'");
		if (hardwareCommandResult.Success && bool.TryParse(hardwareCommandResult.CommandMessages[0], out var result))
		{
			traceEnabled = result;
		}
		return hardwareCommandResult;
	}

	public HardwareCommandResult AddBreakPoint(string scriptName, int lineNumber)
	{
		return ExecuteCommand($"DEBUG add-breakpoint job: '{ID}' script: '{scriptName}' line: {lineNumber}");
	}

	public HardwareCommandResult RemoveBreakPoint(string scriptName, int lineNumber)
	{
		return ExecuteCommand($"DEBUG remove-breakpoint job: '{ID}' script: '{scriptName}' line: {lineNumber}");
	}

	public HardwareCommandResult ClearBreakPoints(string scriptName)
	{
		return ExecuteCommand($"DEBUG clear-breakpoints job: '{ID}' script: '{scriptName}'");
	}

	public HardwareCommandResult GetBreakPoints(string scriptName, out int[] lineNumbers)
	{
		lineNumbers = new int[0];
		HardwareCommandResult hardwareCommandResult = ExecuteCommand($"DEBUG get-breakpoints job: '{ID}' script: '{scriptName}'");
		if (hardwareCommandResult.Success)
		{
			lineNumbers = hardwareCommandResult.CommandMessages.ConvertAll((string each) => int.Parse(each)).ToArray();
		}
		return hardwareCommandResult;
	}

	public HardwareCommandResult BreakAll()
	{
		return ExecuteCommand($"DEBUG break job: '{ID}'");
	}

	public HardwareCommandResult Continue()
	{
		return ExecuteCommand($"DEBUG continue job: '{ID}'");
	}

	public HardwareCommandResult StepInto()
	{
		return ExecuteCommand($"DEBUG step-into job: '{ID}'");
	}

	public HardwareCommandResult StepOver()
	{
		return ExecuteCommand($"DEBUG step-over job: '{ID}'");
	}

	public HardwareCommandResult WaitForCompletion()
	{
		return ExecuteCommand($"JOB wait-for-completion job: '{ID}'");
	}

	public HardwareCommandResult WaitForCompletion(int timeout)
	{
		return Service.ExecuteCommand($"JOB wait-for-completion job: '{ID}'", timeout);
	}

	public override string ToString()
	{
		return $"{ID}";
	}

	public bool Merge(HardwareJob job)
	{
		bool result = false;
		if (job.Label != null && job.Label != Label)
		{
			Label = job.Label;
			result = true;
		}
		if (job.ProgramName != null && job.ProgramName != ProgramName)
		{
			ProgramName = job.ProgramName;
			result = true;
		}
		if (job.StartTime.HasValue && job.StartTime != StartTime)
		{
			StartTime = job.StartTime;
			result = true;
		}
		if (job.ExecutionTime.HasValue && job.ExecutionTime != ExecutionTime)
		{
			ExecutionTime = job.ExecutionTime;
			result = true;
		}
		if (job.Status != Status)
		{
			Status = job.Status;
			result = true;
		}
		if (job.Priority != Priority)
		{
			Priority = job.Priority;
			result = true;
		}
		if (job.ConnectionState != ConnectionState)
		{
			ConnectionState = job.ConnectionState;
			result = true;
		}
		if (job.EnableDebugging != EnableDebugging)
		{
			EnableDebugging = job.EnableDebugging;
			result = true;
		}
		if (job.TraceExecution != TraceExecution)
		{
			TraceExecution = job.TraceExecution;
			result = true;
		}
		return result;
	}

	internal static HardwareJob Parse(HardwareService service, string jobData)
	{
		ReadOnlyCollection<string> readOnlyCollection = ProtocolHelper.ParseProperties(jobData);
		if (readOnlyCollection.Count < 8)
		{
			return null;
		}
		HardwareJob hardwareJob = new HardwareJob(service)
		{
			ID = readOnlyCollection[0],
			Label = readOnlyCollection[1],
			ProgramName = readOnlyCollection[2],
			Priority = Enum<HardwareJobPriority>.ParseIgnoringCase(readOnlyCollection[3], HardwareJobPriority.Normal),
			Status = Enum<HardwareJobStatus>.ParseIgnoringCase(readOnlyCollection[5], HardwareJobStatus.Suspended),
			ConnectionState = Enum<HardwareJobConnectionState>.ParseIgnoringCase(readOnlyCollection[7], HardwareJobConnectionState.Disconnected)
		};
		if (readOnlyCollection.Count == 10)
		{
			hardwareJob.EnableDebugging = readOnlyCollection[8] == "1";
			hardwareJob.TraceExecution = readOnlyCollection[9] == "1";
		}
		if (DateTime.TryParse(readOnlyCollection[4], out var result))
		{
			hardwareJob.StartTime = result;
		}
		if (TimeSpan.TryParse(readOnlyCollection[6], out var result2))
		{
			hardwareJob.ExecutionTime = result2;
		}
		return hardwareJob;
	}

	internal void EventPoll(object o)
	{
		IIpcClientSession ipcClientSession = o as IIpcClientSession;
		while (IsLocallyConnected)
		{
			if (!ClientCommand<HardwareCommandResult>.ExecuteCommand(ipcClientSession, "JOB scheduler-status").Success && EventRaised != null)
			{
				EventRaised(this, DateTime.Now, ">IPC EXCEPTION<");
			}
			Thread.Sleep(250);
		}
		string command = $"JOB disconnect job: '{ID}'";
		ipcClientSession.Dispose();
		ExecuteCommand(command);
	}

	private HardwareCommandResult ExecuteCommand(string command)
	{
		return Service.ExecuteCommand(command);
	}

	private HardwareJob(HardwareService service)
	{
		Service = service;
	}
}
