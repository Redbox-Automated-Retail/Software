using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using Newtonsoft.Json;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Component.Model.Extensions;
using Redbox.HAL.Component.Model.Threading;
using Redbox.HAL.Component.Model.Timers;

namespace Redbox.HAL.Script.Framework;

[Serializable]
public class ExecutionContext : IMessageSink, ISerializable, IExecutionContext, IHardwareCorrectionObserver
{
	internal readonly ExecutionStatus Status = new ExecutionStatus();

	internal readonly Registers Registers;

	internal readonly ApplicationLog AppLog;

	internal readonly IDictionary<string, ProgramInstruction> ProgramCache = new Dictionary<string, ProgramInstruction>();

	internal readonly Deque<ScriptMessage> Messages = new Deque<ScriptMessage>();

	internal bool ShouldCleanup = true;

	private DateTime? LastEventTime;

	private bool IsCooperative;

	private ProgramPriority m_priority;

	private string m_contextDirectory;

	private Action<string> SignalHandler;

	private IMessageSink m_sink;

	private ConnectionState m_connectionState = ConnectionState.Disconnected;

	private bool m_disposed;

	private string m_label;

	private ExecutionResult m_result;

	private DateTime? m_startTime;

	private bool IsHydrating;

	private readonly IDictionary<string, object> SymbolTable = new Dictionary<string, object>();

	private readonly EventWaitHandle CompletionWaitHandle = new EventWaitHandle(initialState: false, EventResetMode.AutoReset);

	private readonly ReaderWriterLockSlim SignalsLock = new ReaderWriterLockSlim();

	private readonly List<string> CachedSignals = new List<string>();

	private readonly ReaderWriterLockSlim ContextLock = new ReaderWriterLockSlim();

	private readonly Deque<object> DataStack = new Deque<object>();

	private readonly List<string> CachedResults = new List<string>();

	private readonly ReaderWriterLockSlim ObserverLock = new ReaderWriterLockSlim();

	private const string ResultPrefix = "RESULT";

	private const string EmptyItemToken = "~";

	public IFormattedLog ContextLog => AppLog;

	public string ID { get; private set; }

	public string ProgramName { get; private set; }

	public bool IsComplete
	{
		get
		{
			if (!(Status == ExecutionContextStatus.Errored) && !(Status == ExecutionContextStatus.Completed) && !(Status == ExecutionContextStatus.Garbage))
			{
				return Status == ExecutionContextStatus.Stopped;
			}
			return true;
		}
	}

	public IMessageSink MessageSink => m_sink;

	public bool IsImmediate { get; internal set; }

	public DateTime CreatedOn { get; private set; }

	public bool IsConnected => m_connectionState == ConnectionState.Connected;

	public IExecutionResult Result => m_result;

	public DateTime? ExecutionStart { get; private set; }

	public DateTime? StartTime
	{
		get
		{
			return m_startTime;
		}
		set
		{
			using (new WithWriteLock(ContextLock))
			{
				m_startTime = value;
				SaveUnderLock();
			}
		}
	}

	public ProgramPriority Priority
	{
		get
		{
			if (!IsComplete)
			{
				return m_priority;
			}
			return ProgramPriority.Lowest;
		}
		set
		{
			using (new WithWriteLock(ContextLock))
			{
				m_priority = value;
				SaveUnderLock();
			}
		}
	}

	public string Label
	{
		get
		{
			return m_label;
		}
		set
		{
			using (new WithWriteLock(ContextLock))
			{
				m_label = value;
				SaveUnderLock();
			}
		}
	}

	public int ResultCount => CachedResults.Count;

	public int StackDepth => DataStack.Count;

	internal string ParentID { get; set; }

	internal bool EligibleForScheduling => Status == ExecutionContextStatus.Pending;

	internal ExecutionContextStatus? DeferredStatusChangeRequested { get; set; }

	internal string ContextDirectory
	{
		get
		{
			if (string.IsNullOrEmpty(m_contextDirectory))
			{
				string path = ServiceLocator.Instance.GetService<IRuntimeService>().RuntimePath("data\\contexts");
				m_contextDirectory = Path.Combine(path, ID);
			}
			return m_contextDirectory;
		}
	}

	internal event OnHardwareCorrectionStart StartHardwareCorrectionEvent;

	internal event OnHardwareCorrectionEnd EndHardwareCorrectionEvent;

	internal event OnJobComplete JobCompleteEvent;

	public void Dispose()
	{
		if (!m_disposed)
		{
			m_disposed = true;
			try
			{
				Directory.Delete(ContextDirectory, recursive: true);
			}
			catch (Exception e)
			{
				LogHelper.Instance.Log($"Unable to delete context directory {ContextDirectory}", e);
			}
			CachedResults.Clear();
			DataStack.Clear();
			Messages.Clear();
			SymbolTable.Clear();
			CompletionWaitHandle.Close();
		}
	}

	public void HardwareCorrectionStart(HardwareCorrectionEventArgs s)
	{
		StartHardwareCorrectionEvent?.Invoke(s);
	}

	public void HardwareCorrectionEnd(HardwareCorrectionEventArgs s)
	{
		EndHardwareCorrectionEvent?.Invoke(s);
	}

	public bool Send(string message)
	{
		using (new WithWriteLock(ContextLock))
		{
			return SendUnderLock(message);
		}
	}

	public void CreateResult(string code, string msg, string id)
	{
		CreateResult(StackEnd.Bottom, code, msg, null, null, id, null, null);
	}

	public string CreateJSONResult(string code, string id, IDictionary<string, object> jsonMessage)
	{
		string message = ((jsonMessage != null && jsonMessage.Count > 0) ? JsonConvert.SerializeObject(jsonMessage) : string.Empty);
		return CreateResult(StackEnd.Bottom, code, message, null, null, id, null, null);
	}

	public void ForeachResult(Action<string> action)
	{
		using (new WithReadLock(ContextLock))
		{
			CachedResults.ForEach(action);
		}
	}

	public void ClearResults()
	{
		using (new WithWriteLock(ContextLock))
		{
			CachedResults.Clear();
			SaveUnderLock();
		}
	}

	public void Trash()
	{
		LogHelper.Instance.Log("Trash context {0} ( {1} )", ID, ProgramName);
		using (new WithWriteLock(ContextLock))
		{
			ChangeStatusUnderLock(ExecutionContextStatus.Garbage);
			if (CachedResults.Count > 0)
			{
				CachedResults.Clear();
			}
			SaveUnderLock();
		}
	}

	public void Terminate()
	{
		if (!(Status == ExecutionContextStatus.Completed) && !(Status == ExecutionContextStatus.Garbage) && !(Status == ExecutionContextStatus.Errored) && !(Status == ExecutionContextStatus.Stopped))
		{
			ChangeStatus(ExecutionContextStatus.Stopped);
		}
	}

	public void Resume()
	{
		m_result.Errors.RemoveCode("E201");
		if (Status != ExecutionContextStatus.Suspended)
		{
			LogHelper.Instance.Log($"Cannot resume context with status {Status.ToString()}", LogEntryType.Error);
		}
		else
		{
			ChangeStatus(ExecutionContextStatus.Pending);
		}
	}

	public void Resurrect()
	{
		m_result.Errors.RemoveCode("E201");
		ChangeStatus(ExecutionContextStatus.Pending);
	}

	public void Pend()
	{
		if (IsImmediate || Status == ExecutionContextStatus.Suspended)
		{
			ChangeStatus(ExecutionContextStatus.Pending);
		}
	}

	public void Suspend()
	{
		if (IsImmediate || Status == ExecutionContextStatus.Pending || Status == ExecutionContextStatus.Running)
		{
			ChangeStatus(ExecutionContextStatus.Suspended);
			return;
		}
		LogHelper.Instance.Log(LogEntryType.Debug, "Cannot suspend job that isn't in Pending or Running status.", LogEntryType.Error);
	}

	public bool WaitForCompletion()
	{
		if (IsComplete)
		{
			return true;
		}
		while (true)
		{
			try
			{
				if (CompletionWaitHandle.WaitOne(50, exitContext: false) || IsComplete)
				{
					return true;
				}
				Thread.Sleep(0);
			}
			catch (ObjectDisposedException)
			{
				return true;
			}
		}
	}

	public bool Connect(IMessageSink sink)
	{
		using (new WithWriteLock(ContextLock))
		{
			m_sink = sink;
			if (m_sink == null)
			{
				m_connectionState = ConnectionState.Disconnected;
			}
			else
			{
				m_connectionState = ConnectionState.Connected;
				FlushUnderLock();
				SaveUnderLock();
			}
		}
		return true;
	}

	public bool Disconnect()
	{
		return Connect(null);
	}

	public ExecutionContextStatus GetStatus()
	{
		return Status.Value;
	}

	public object Pop(StackEnd end)
	{
		object obj = null;
		using (new WithWriteLock(ContextLock))
		{
			obj = OnPop(end);
			SaveUnderLock();
			return obj;
		}
	}

	public void Push(object instance, StackEnd end)
	{
		using (new WithWriteLock(ContextLock))
		{
			OnPush(instance, end);
			SaveUnderLock();
		}
	}

	public void Push(List<ILocation> locations)
	{
		using (new WithWriteLock(ContextLock))
		{
			locations.ForEach(delegate(ILocation location)
			{
				OnPush(location.Slot, StackEnd.Top);
				OnPush(location.Deck, StackEnd.Top);
			});
			OnPush(locations.Count, StackEnd.Top);
			SaveUnderLock();
		}
	}

	public void ClearStack()
	{
		using (new WithWriteLock(ContextLock))
		{
			DataStack.Clear();
			SaveUnderLock();
		}
	}

	public void ForeachStackItem(Action<object> action)
	{
		using (new WithReadLock(ContextLock))
		{
			foreach (object item in DataStack)
			{
				action(item);
			}
		}
	}

	public void Signal(string signal)
	{
		using (new WithUpgradeableReadLock(SignalsLock))
		{
			if (SignalHandler != null)
			{
				SignalHandler(signal);
				return;
			}
			using (new WithWriteLock(SignalsLock))
			{
				CachedSignals.Add(signal);
			}
		}
	}

	public void ForeachMessage(Action<string> action)
	{
		using (new WithReadLock(ContextLock))
		{
			foreach (ScriptMessage message in Messages)
			{
				action(message.ToString());
			}
		}
	}

	public void ForeachSymbol(Action<IContextSymbol> action)
	{
		using (new WithReadLock(ContextLock))
		{
			foreach (string key in SymbolTable.Keys)
			{
				ContextSymbol contextSymbol = new ContextSymbol(key)
				{
					Value = SymbolTable[key]
				};
				if (!contextSymbol.IsReserved)
				{
					action(contextSymbol);
				}
			}
		}
	}

	public void ClearSymbolTable()
	{
		using (new WithWriteLock(ContextLock))
		{
			SymbolTable.Clear();
			SaveUnderLock();
		}
	}

	protected ExecutionContext(SerializationInfo info, StreamingContext context)
	{
		IsHydrating = true;
		bool flag = true;
		try
		{
			flag = (IsCooperative = info.GetBoolean("IsCooperative"));
		}
		catch (SerializationException e)
		{
			LogHelper.Instance.Log("Unable to hydrate context due to serialization exception", e);
			IsCooperative = true;
		}
		m_connectionState = ConnectionState.Disconnected;
		m_result = new ExecutionResult();
		Registers = new Registers();
		m_label = info.GetString("Label");
		ProgramName = info.GetString("ProgramName");
		ID = info.GetString("ID");
		string value = info.GetString("Priority");
		m_priority = Enum<ProgramPriority>.ParseIgnoringCase(value, ProgramPriority.Normal);
		string value2 = info.GetString("Status");
		Status.NextValue = Enum<ExecutionContextStatus>.ParseIgnoringCase(value2, ExecutionContextStatus.Stopped);
		if (Status.NextValue == ExecutionContextStatus.Running)
		{
			Status.NextValue = ExecutionContextStatus.Suspended;
		}
		Status.Promote();
		CreatedOn = info.GetDateTime("CreatedOn");
		if (info.GetBoolean("HasStartTime"))
		{
			m_startTime = info.GetDateTime("StartTime");
		}
		if (info.GetBoolean("HasExecutionTime"))
		{
			string text = info.GetString("ExecutionTime");
			if (!string.IsNullOrEmpty(text))
			{
				m_result.ExecutionTime = TimeSpan.Parse(text);
			}
		}
		if (info.GetBoolean("HasExecutionStartTime"))
		{
			ExecutionStart = info.GetDateTime("ExecutionStartTime");
		}
		string text2 = info.GetString("Errors");
		if (!string.IsNullOrEmpty(text2))
		{
			string[] array = text2.Split(";".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
			if (array != null && array.Length != 0)
			{
				m_result.Errors.AddRange(ErrorList.NewFromStrings(array));
			}
		}
		string text3 = info.GetString("Messages");
		if (!string.IsNullOrEmpty(text3))
		{
			Array.ForEach(text3.Split(";".ToCharArray(), StringSplitOptions.RemoveEmptyEntries), delegate(string item)
			{
				if (!string.IsNullOrEmpty(item))
				{
					Messages.PushBottom(ScriptMessage.Parse(item));
				}
			});
		}
		string text4 = info.GetString("Results");
		if (!string.IsNullOrEmpty(text4))
		{
			Array.ForEach(text4.Split(";".ToCharArray(), StringSplitOptions.RemoveEmptyEntries), delegate(string item)
			{
				if (!string.IsNullOrEmpty(item))
				{
					CachedResults.Add(item);
				}
			});
		}
		ParentID = info.GetString("ParentID");
		if (flag)
		{
			string[] array2 = info.GetString("Programs").Split(";".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
			BinaryFormatter binaryFormatter = new BinaryFormatter();
			string[] array3 = array2;
			foreach (string text5 in array3)
			{
				string path = Path.Combine(ContextDirectory, text5);
				try
				{
					using FileStream serializationStream = File.OpenRead(path);
					ProgramCache[text5] = (ProgramInstruction)binaryFormatter.Deserialize(serializationStream);
				}
				catch (Exception e2)
				{
					LogHelper.Instance.Log("Unable to deserialize program.", e2);
				}
			}
		}
		if (info.GetBoolean("HasSymbolTable"))
		{
			SymbolTable = (IDictionary<string, object>)info.GetValue("SymbolTable", typeof(Dictionary<string, object>));
		}
		string[] array4 = info.GetString("Stack").Split(";".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
		for (int num2 = array4.Length - 1; num2 >= 0; num2--)
		{
			string[] array5 = array4[num2].Split("=".ToCharArray(), 2, StringSplitOptions.RemoveEmptyEntries);
			if (array5.Length >= 2)
			{
				OnPush(ConversionHelper.ChangeType(array5[1], Type.GetType(array5[0])), StackEnd.Top);
			}
		}
		if (flag)
		{
			string[] array6 = info.GetString("CallFrames").Split(";".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
			for (int num3 = array6.Length - 1; num3 >= 0; num3--)
			{
				string[] array7 = array6[num3].Split('|');
				string text6 = array7[3];
				int parentContextLineNumber = int.Parse(array7[4]);
				CallFrame callFrame = new CallFrame
				{
					PC = int.Parse(array7[0]),
					X = int.Parse(array7[1]),
					Y = int.Parse(array7[2]),
					ProgramName = text6
				};
				ProgramInstruction program = (ProgramCache.ContainsKey(text6) ? ProgramCache[text6] : null);
				callFrame.LoadInstructionPointer(parentContextLineNumber, program);
				Registers.CallStack.Push(callFrame);
			}
		}
		AppLog = ApplicationLog.Deserialize(info, this);
		IsHydrating = false;
	}

	internal void Reset()
	{
		Suspend();
		CompletionWaitHandle.Reset();
		m_result.Reset();
		Registers.Reset();
	}

	internal void Save()
	{
		using (new WithWriteLock(ContextLock))
		{
			SaveUnderLock();
		}
	}

	internal object GetSymbolValue(string name, ErrorList errors)
	{
		string text = name.ToUpper();
		if (!SymbolTable.TryGetValue(text, out var value))
		{
			errors.Add(Error.NewError("E002", $"The symbol {text} is not defined.", $"Line {Registers.ActiveFrame.IP.LineNumber}: {Registers.ActiveFrame.IP}"));
			return null;
		}
		return value;
	}

	internal void SetSymbolValue(string name, object value)
	{
		SymbolTable[name.ToUpper()] = value;
	}

	internal void Execute()
	{
		ExecutionTimer executionTimer = new ExecutionTimer();
		ExecutionContextStatus executionContextStatus = ExecutionContextStatus.Completed;
		try
		{
			if (!ExecutionStart.HasValue)
			{
				ExecutionStart = DateTime.Now;
			}
			ChangeStatusUnderLock(ExecutionContextStatus.Running);
			SendStatusMessage(ExecutionContextStatus.Running);
			AppLog.Open();
			if (Registers.ActiveFrame == null)
			{
				Registers.PushCallFrame(ProgramName, ProgramCache[ProgramName]);
			}
			while (!Status.ShouldPromote())
			{
				if (Registers.ActiveFrame.IP != null)
				{
					Registers.ActiveFrame.IP.Execute(m_result, this);
				}
				if (m_result.Errors.ContainsError())
				{
					Status.NextValue = (executionContextStatus = ExecutionContextStatus.Errored);
					if (!Registers.ActiveFrame.IP.ExitsContext)
					{
						LogHelper.Instance.Log("Job {0} had errors processing instruction at line {1}.", ID, Registers.ActiveFrame.IP.LineNumber);
					}
					continue;
				}
				if (m_result.RestartAtCurrentIP)
				{
					m_result.RestartAtCurrentIP = false;
					continue;
				}
				if (m_result.SwitchToContext != null)
				{
					Registers.PushCallFrame(m_result.SwitchToContext.GetProgram().Operands[0].Value, m_result.SwitchToContext);
					m_result.SwitchToContext = null;
					continue;
				}
				while (!Registers.ActiveFrame.FetchNextInstruction())
				{
					Registers.PopCallFrame();
					if (Registers.ActiveFrame == null)
					{
						Status.NextValue = (executionContextStatus = ExecutionContextStatus.Completed);
						break;
					}
				}
			}
			Status.Promote();
			m_result.UpdateExecutionTime(executionTimer.Elapsed);
			SendStatusMessage(Status.Value);
		}
		catch (Exception ex)
		{
			m_result.Errors.Add(Error.NewError("E001", "An unhandled exception was raised by the execution engine.", ex.ToString()));
			LogHelper.Instance.Log($"An instruction in program {ProgramName} ( ID = {ID} ) threw an unhandled exception.", ex);
			executionContextStatus = ExecutionContextStatus.Errored;
			Status.Promote(ExecutionContextStatus.Errored);
			m_result.UpdateExecutionTime(executionTimer.Elapsed);
		}
		finally
		{
			Save();
			executionTimer.Dispose();
			if (IsComplete)
			{
				CompletionWaitHandle.Set();
				if (!IsImmediate && ShouldCleanup)
				{
					CleanupHardware();
					string msg = $"Job {ProgramName} ID {ID} ends with status {executionContextStatus.ToString()}";
					LogHelper.Instance.Log(msg);
					AppLog.Write(msg);
					CompletionWaitHandle.Close();
				}
				NotifyJobComplete();
			}
			AppLog.Close();
		}
	}

	internal void PushTop(object instance)
	{
		OnPush(instance, StackEnd.Top);
	}

	internal void PushBottom(object instance)
	{
		OnPush(instance, StackEnd.Bottom);
	}

	internal T PopTop<T>()
	{
		return ConversionHelper.ChangeType<T>(OnPop(StackEnd.Top));
	}

	internal object PopTop()
	{
		return OnPop(StackEnd.Top);
	}

	internal object PopBottom()
	{
		return OnPop(StackEnd.Bottom);
	}

	internal void RegisterHandler(Action<string> handler)
	{
		if (handler == null)
		{
			return;
		}
		using (new WithUpgradeableReadLock(SignalsLock))
		{
			if (SignalHandler != null)
			{
				return;
			}
			using (new WithWriteLock(SignalsLock))
			{
				SignalHandler = handler;
				CachedSignals.ForEach(delegate(string each)
				{
					SignalHandler(each);
				});
				CachedSignals.Clear();
			}
		}
	}

	internal void CopyProgramAndReferences(string programName, ProgramInstruction programInstruction)
	{
		ProgramName = programName;
		IsCooperative = programInstruction.IsCooperative;
		ProgramCache[ProgramName] = (ProgramInstruction)programInstruction.Clone();
		CopyReferences(ProgramCache[ProgramName]);
	}

	internal bool FlushMessages()
	{
		return FlushUnderLock();
	}

	internal void PromoteDeferredStatusChange()
	{
		Status.NextValue = DeferredStatusChangeRequested ?? ExecutionContextStatus.Pending;
		DeferredStatusChangeRequested = null;
	}

	internal void CleanupHardware()
	{
		IControlSystem service = ServiceLocator.Instance.GetService<IControlSystem>();
		service.SetSensors(on: false);
		service.StopRoller();
		service.ToggleRingLight(on: false, null);
	}

	internal void OnMoveVendError(ErrorCodes error)
	{
		OnMoveVendError(error, null);
	}

	internal void OnMoveVendError(ErrorCodes error, string barcode)
	{
		string message = $"MOVEVEND returned error {error}";
		CreateInfoResult("MachineError", message, barcode);
		Result.Errors.Add(Error.NewError("E999", "Execution context error.", "MACHINE ERROR"));
	}

	internal string CreateCameraCaptureErrorResult()
	{
		return CreateResult(StackEnd.Bottom, "CameraCaptureFailure", "The CAMERA CAPTURE instruction failed.", null, null, null, null, null);
	}

	internal string CreateItemStuckResult(string id)
	{
		return CreateResult(StackEnd.Bottom, "ItemStuckInGripper", "There is an item stuck in the gripper", null, null, id, null, null);
	}

	internal string CreateMoveErrorResult()
	{
		return CreateMoveErrorResult(null, null);
	}

	internal string CreateMoveErrorResult(int? deck, int? slot)
	{
		return CreateResult(StackEnd.Bottom, "MachineError", "There was an error executing the MOVE instruction.", deck, slot, null, null, null);
	}

	internal void CreateLookupErrorResult(string id)
	{
		CreateLookupErrorResult("BarcodeNotFound", id);
	}

	internal void CreateLookupErrorResult(string code, string id)
	{
		CreateInfoResult(code, "The LOOKUP request failed because the item was not found.", id);
	}

	internal void CreateNoBarcodeReadResult()
	{
		CreateNoBarcodeReadResult(null);
	}

	internal void CreateNoBarcodeReadResult(string matrix)
	{
		CreateInfoResult("BadIdRead", "There were no barcodes found.", matrix);
	}

	internal void CreateDuplicateItemResult(string matrix)
	{
		CreateInfoResult("DuplicateDetected", "The read ID is a duplicate.", matrix);
	}

	internal void CreateMachineFullResult()
	{
		CreateMachineFullResult(null);
	}

	internal void CreateMachineFullResult(string id)
	{
		CreateInfoResult("MachineFull", "The machine is full.", id);
	}

	internal string CreateInfoResult(StackEnd end, string code, string message)
	{
		return CreateResult(end, code, message, null, null, null, null, null);
	}

	internal string CreateInfoResult(string code, string message)
	{
		return CreateResult(StackEnd.Bottom, code, message, null, null, null, null, null);
	}

	internal string CreateInfoResult(string code, string message, string id)
	{
		return CreateResult(StackEnd.Bottom, code, message, null, null, id, null, null);
	}

	internal string CreateResult(string code, string message, int? deck, int? slot, string item, DateTime? returnTime, string previousID)
	{
		return CreateResult(StackEnd.Bottom, code, message, deck, slot, item, returnTime, previousID);
	}

	internal string CreateResult(StackEnd end, string code, string message, int? deck, int? slot, string item, DateTime? returnTime, string previousID)
	{
		if (IsImmediate)
		{
			return string.Empty;
		}
		using (new WithWriteLock(ContextLock))
		{
			string text = "~";
			if (deck.HasValue && slot.HasValue)
			{
				text = $"{deck},{slot}";
			}
			string text2 = "~";
			if (returnTime.HasValue)
			{
				text2 = returnTime.ToString();
			}
			Guid guid = Guid.NewGuid();
			DateTime now = DateTime.Now;
			string text3 = item ?? "~";
			string text4 = previousID ?? "~";
			if (message.Contains("'"))
			{
				message = message.Replace('\'', ' ');
			}
			string text5 = string.Format("{0}|{1}|{2}|{3}|{4}|{5}|{6}|{7}|{8}", "RESULT", guid, now, code, text, text3, message, text2, text4);
			CachedResults.Add(text5);
			return text5;
		}
	}

	public ExecutionContext(string id, string programName, DateTime? startTime, ProgramPriority p, string label)
	{
		IsHydrating = true;
		ID = id;
		CreatedOn = DateTime.Now;
		ProgramName = programName;
		Registers = new Registers();
		m_result = new ExecutionResult();
		m_connectionState = ConnectionState.Disconnected;
		AppLog = new ApplicationLog(this);
		m_startTime = startTime;
		m_priority = p;
		m_label = label;
		IsHydrating = false;
	}

	void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
	{
		if (IsImmediate)
		{
			return;
		}
		info.AddValue("IsCooperative", IsCooperative);
		info.AddValue("ID", ID);
		info.AddValue("Label", Label);
		info.AddValue("ProgramName", ProgramName);
		info.AddValue("Priority", Priority.ToString());
		info.AddValue("Status", Status.ToString());
		info.AddValue("CreatedOn", CreatedOn);
		if (StartTime.HasValue)
		{
			info.AddValue("StartTime", StartTime);
			info.AddValue("HasStartTime", value: true);
		}
		else
		{
			info.AddValue("HasStartTime", value: false);
		}
		if (ExecutionStart.HasValue)
		{
			info.AddValue("ExecutionStartTime", ExecutionStart.Value);
			info.AddValue("HasExecutionStartTime", value: true);
		}
		else
		{
			info.AddValue("HasExecutionStartTime", value: false);
		}
		StringBuilder errorItems = new StringBuilder();
		m_result.Errors.ForEach(delegate(Error each)
		{
			errorItems.Append($"{each}|{each.Details};");
		});
		info.AddValue("Errors", errorItems.ToString());
		if (m_result.ExecutionTime.HasValue)
		{
			info.AddValue("ExecutionTime", m_result.ExecutionTime.ToString());
			info.AddValue("HasExecutionTime", value: true);
		}
		else
		{
			info.AddValue("HasExecutionTime", value: false);
		}
		StringBuilder stringBuilder = new StringBuilder();
		foreach (ScriptMessage message in Messages)
		{
			stringBuilder.Append($"{message.ToString()};");
		}
		info.AddValue("Messages", stringBuilder.ToString());
		StringBuilder stringBuilder2 = new StringBuilder();
		foreach (string cachedResult in CachedResults)
		{
			stringBuilder2.Append($"{cachedResult.ToString()};");
		}
		info.AddValue("Results", stringBuilder2.ToString());
		info.AddValue("ParentID", ParentID);
		if (SymbolTable.Keys.Count > 0)
		{
			info.AddValue("SymbolTable", SymbolTable, SymbolTable.GetType());
			info.AddValue("HasSymbolTable", value: true);
		}
		else
		{
			info.AddValue("HasSymbolTable", value: false);
		}
		StringBuilder stringBuilder3 = new StringBuilder();
		foreach (object item in DataStack)
		{
			stringBuilder3.Append($"{item.GetType().FullName}={item};");
		}
		info.AddValue("Stack", stringBuilder3);
		if (IsCooperative)
		{
			StringBuilder stringBuilder4 = new StringBuilder();
			foreach (CallFrame item2 in Registers.CallStack)
			{
				string value = $"{item2.PC}|{item2.X}|{item2.Y}|{item2.ProgramName}|{((item2.ParentContext != null) ? item2.ParentContext.LineNumber : (-1))};";
				stringBuilder4.Append(value);
			}
			info.AddValue("CallFrames", stringBuilder4.ToString());
			BinaryFormatter binaryFormatter = new BinaryFormatter();
			StringBuilder stringBuilder5 = new StringBuilder();
			foreach (string key in ProgramCache.Keys)
			{
				try
				{
					stringBuilder5.Append(key + ";");
					using FileStream serializationStream = File.OpenWrite(Path.Combine(ContextDirectory, key));
					binaryFormatter.Serialize(serializationStream, ProgramCache[key]);
				}
				catch (Exception e)
				{
					LogHelper.Instance.Log($"Unable to serialize program {key}.", e);
				}
			}
			info.AddValue("Programs", stringBuilder5.ToString());
		}
		AppLog.Serialize(info);
	}

	private void SendStatusMessage(ExecutionContextStatus status)
	{
		using (new WithWriteLock(ContextLock))
		{
			SendUnderLock($">STATUS CHANGE< {status}");
		}
	}

	private void CopyReferences(Instruction instruction)
	{
		if (instruction == null || instruction.Instructions.Count == 0)
		{
			return;
		}
		foreach (Instruction instruction2 in instruction.Instructions)
		{
			if (instruction2.Instructions.Count > 0)
			{
				CopyReferences(instruction2);
			}
		}
	}

	private void ChangeStatus(ExecutionContextStatus status)
	{
		using (new WithWriteLock(ContextLock))
		{
			ChangeStatusUnderLock(status);
			SaveUnderLock();
		}
	}

	private void ChangeStatusUnderLock(ExecutionContextStatus status)
	{
		if (Status == ExecutionContextStatus.Running)
		{
			if (!DeferredStatusChangeRequested.HasValue)
			{
				DeferredStatusChangeRequested = status;
			}
		}
		else
		{
			Status.Promote(status);
		}
	}

	private bool SendUnderLock(string message)
	{
		DateTime dateTime = DateTime.Now;
		if (LastEventTime.HasValue && dateTime.Second <= LastEventTime.Value.Second)
		{
			dateTime = LastEventTime.Value.AddSeconds(1.0);
		}
		LastEventTime = dateTime;
		Messages.PushTop(new ScriptMessage(dateTime, message));
		return FlushUnderLock();
	}

	private bool FlushUnderLock()
	{
		if (m_connectionState != ConnectionState.Connected)
		{
			return false;
		}
		int num = 0;
		while (Messages.Count > 0)
		{
			string text = Messages.PopTop().ToString();
			LogHelper.Instance.Log(LogEntryType.Debug, "Job ID = {0}, Message = {1}", ID, text);
			try
			{
				MessageSink.Send(text);
				num++;
			}
			catch (Exception ex)
			{
				LogHelper.Instance.Log(LogEntryType.Error, "Job {0} ID {1} failed to send message: exception message = {2}", ProgramName, ID, ex.Message);
			}
		}
		return num > 0;
	}

	private void SaveUnderLock()
	{
		if (IsImmediate)
		{
			return;
		}
		if (IsHydrating)
		{
			LogHelper.Instance.Log("[ExecutionContext] !! Save under lock while hydrating !!");
		}
		try
		{
			if (!Directory.Exists(ContextDirectory))
			{
				Directory.CreateDirectory(ContextDirectory);
			}
			using FileStream serializationStream = File.OpenWrite(Path.Combine(ContextDirectory, "ctx.data"));
			new BinaryFormatter().Serialize(serializationStream, this);
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("ExecutionContext.Save threw an unhandled exception.", e);
		}
	}

	private void OnPush(object instance, StackEnd end)
	{
		if (StackEnd.Top == end)
		{
			DataStack.PushTop(instance);
		}
		else if (end == StackEnd.Bottom)
		{
			DataStack.PushBottom(instance);
		}
	}

	private object OnPop(StackEnd end)
	{
		if (StackEnd.Top != end)
		{
			return DataStack.PopBottom();
		}
		return DataStack.PopTop();
	}

	private void NotifyJobComplete()
	{
		JobCompleteEvent?.Invoke(Status.Value);
	}
}
