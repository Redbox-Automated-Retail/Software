using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using System.Timers;
using Microsoft.Win32;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Component.Model.Extensions;
using Redbox.HAL.Component.Model.Threading;
using Redbox.HAL.Controller.Framework;
using Redbox.HAL.Core;
using Redbox.HAL.Script.Framework.Services;

namespace Redbox.HAL.Script.Framework;

public sealed class ExecutionEngine : IExecutionService, IConfigurationObserver
{
	internal readonly string DataDirectory;

	private ExecutionContext ActiveContext;

	private bool SchedulerEnabled;

	private bool ContextSwitchRequested;

	private Thread SchedulerThread;

	private TimeSpan? RebootWindowStart;

	private readonly PollerManager PollerManager = new PollerManager();

	private readonly ReaderWriterLockSlim ContextLock = new ReaderWriterLockSlim();

	private readonly ExecutionContextList Contexts = new ExecutionContextList();

	private readonly object ModeLock = new object();

	private readonly ManualResetEvent SchedulerResetEvent = new ManualResetEvent(initialState: false);

	private readonly ProgramCache ProgramCache = new ProgramCache();

	private readonly ExecutionContext ImmediateModeContext;

	private readonly string ScriptsPath;

	private readonly IdHelper IdGenerator = new IdHelper();

	private readonly string[] SingletonJobs = new string[1] { "clean-vmz" };

	private readonly RedboxTimer RebootWakeupTimer;

	private readonly string[] NonSchedulableJobsWithinReboot = new string[7] { "qlm-unload", "thin", "unload-thin", "sync", "sync-locations", "thin-vmz", "clean-vmz" };

	private readonly string[] NonRestartableJobs = new string[4] { "init", "return", "vend", "soft-sync" };

	private static readonly object InstanceLock = new object();

	public bool IsImmediateJobPending
	{
		get
		{
			if (!(ImmediateModeContext.Status == ExecutionContextStatus.Pending))
			{
				return ImmediateModeContext.Status == ExecutionContextStatus.Running;
			}
			return true;
		}
	}

	internal static ExecutionEngine Instance { get; private set; }

	internal ExecutionContextStatus InitStatus { get; private set; }

	internal TimeSpan? KioskRebootTime { get; private set; }

	internal EngineModes Mode { get; private set; }

	public event EventHandler<EngineModeChangeEventArgs> EngineModeChange;

	public void NotifyConfigurationLoaded()
	{
		int rebootExecutionPreventionWindow = ControllerConfiguration.Instance.RebootExecutionPreventionWindow;
		LogHelper.Instance.Log(LogEntryType.Debug, "[ExecutionEngine] NotifyConfigurationLoaded reboot window = {0}", rebootExecutionPreventionWindow);
		if (rebootExecutionPreventionWindow <= 0)
		{
			return;
		}
		try
		{
			using RegistryKey registryKey = Registry.LocalMachine.CreateSubKey("SOFTWARE\\Redbox\\REDS\\Kiosk Engine\\Store");
			string text = string.Empty;
			object value = registryKey.GetValue("RebootTime");
			LogHelper.Instance.Log(LogEntryType.Debug, "[ExecutionEngine] rtValue = {0}", value.ToString());
			if (value != null)
			{
				text = ConversionHelper.ChangeType<string>(value);
			}
			if (!string.IsNullOrEmpty(text))
			{
				try
				{
					KioskRebootTime = TimeSpan.Parse(text);
					RebootWindowStart = KioskRebootTime.Value.Subtract(new TimeSpan(0, rebootExecutionPreventionWindow, 0));
					LogHelper.Instance.Log("[ExecutionEngine] Configuration load KioskRebootTime = {0} RebootWindowStart = {1}", KioskRebootTime, RebootWindowStart);
					return;
				}
				catch (Exception e)
				{
					LogHelper.Instance.Log("[ExecutionEngine] Parsing kiosk reboot time failed.", e);
					KioskRebootTime = (RebootWindowStart = null);
					return;
				}
			}
		}
		catch (Exception e2)
		{
			LogHelper.Instance.Log("[ExecutionEngine] Unable to read secret option 'RebootTime'", e2);
		}
	}

	public void NotifyConfigurationChangeStart()
	{
	}

	public void NotifyConfigurationChangeEnd()
	{
	}

	public static IExecutionService NewEngine(string scriptsPath)
	{
		lock (InstanceLock)
		{
			if (Instance == null)
			{
				Instance = new ExecutionEngine(scriptsPath);
			}
			return Instance;
		}
	}

	public IExecutionContext ScheduleJob(string progName, string label, DateTime? startTime, ProgramPriority priority)
	{
		return ScheduleJobInternal(progName, label, startTime, priority);
	}

	public IExecutionContext GetActiveContext()
	{
		return ActiveContext;
	}

	public IExecutionContext GetJob(string jobId)
	{
		return Contexts.GetByID(jobId);
	}

	public IExecutionContext[] GetByLabel(string label)
	{
		return Contexts.GetByLabel(label);
	}

	public string[] GetProgramList()
	{
		List<string> list = new List<string>();
		List<string> list2 = ProgramCache.ProgramList();
		using (new DisposeableList<string>(list2))
		{
			foreach (string item in list2)
			{
				if (!item.StartsWith("$"))
				{
					list.Add($"{item}|{ProgramCache.GetProgram(item)}|{false}");
				}
			}
		}
		return list.ToArray();
	}

	public ExecutionContextStatus GetInitJobStatus()
	{
		return InitStatus;
	}

	public void RemoveProgram(string programName)
	{
		ProgramCache.RemoveProgram(programName);
	}

	public void CleanupJobs(bool force)
	{
		Contexts.CleanupGarbage(force);
	}

	public void Enter(EngineModes mode)
	{
		lock (ModeLock)
		{
			Mode |= mode;
			LogHelper.Instance.Log("[Execution Engine]: Entering mode {0}", mode.ToString());
			OnModeChanged();
		}
	}

	public void Exit(EngineModes mode)
	{
		lock (ModeLock)
		{
			Mode &= ~mode;
			LogHelper.Instance.Log("[Execution Engine]: Exiting mode {0}", mode.ToString());
			OnModeChanged();
		}
	}

	public void Initialize(ErrorList errors)
	{
		ControllerConfiguration.Instance.AddObserver(this);
		ServiceLocator.Instance.AddService<IBarcodeValidatorService>(new RedboxBarcodeValidator());
		ServiceLocator.Instance.AddService<IFraudService>(new FraudValidatorService());
		PollerManager.InitializePollers();
		ProgramCache.CompilePrograms(ScriptsPath, errors);
		LoadJobs();
	}

	public void Start()
	{
		RestartYoungest(5);
		CleanupJobs(force: false);
		SchedulerThread = new Thread(ProcessScheduler)
		{
			Name = "HAL Script Scheduler",
			IsBackground = true
		};
		SchedulerThread.SetApartmentState(ApartmentState.STA);
		SchedulerThread.Start();
		SchedulerEnabled = true;
		Version version = typeof(ExecutionEngine).Assembly.GetName().Version;
		LogHelper.Instance.Log("[ExecutionEngine] Version {0} initialized.", version);
		ScheduleInit(isBootstrap: true);
	}

	public void Shutdown()
	{
		PollerManager.StopPollers();
		SchedulerEnabled = false;
		SuspendAll(force: true);
		while (ActiveContext != null)
		{
			Thread.Sleep(0);
		}
		LogHelper.Instance.Log("ExecutionEngine.Shutdown() complete.");
	}

	public void Suspend()
	{
		PollerManager.StopPollers();
		SuspendAll(force: true);
	}

	public void Restart()
	{
		ScheduleInit(isBootstrap: false);
		RestartYoungest(5);
		PollerManager.StartPollers();
	}

	public IExecutionResult CompileImmediate(Stream s)
	{
		return Compile(s, "$$$immediate$$$");
	}

	public IExecutionResult Compile(Stream stream, string programName)
	{
		return ProgramCache.CompileInner(stream, programName);
	}

	public IExecutionContext ExecuteImmediate()
	{
		ImmediateModeContext.Reset();
		CompiledProgram program = ProgramCache.GetProgram("$$$immediate$$$");
		if (program == null || program.CompilerResult.Errors.ContainsError())
		{
			ImmediateModeContext.Result.Errors.Add(Error.NewError("E001", string.Format("The program '{0}' was not found in the program cache.", "$$$immediate$$$"), "Recompile the program which will place it in the program cache."));
			return ImmediateModeContext;
		}
		ImmediateModeContext.CopyProgramAndReferences("$$$immediate$$$", program.ParseTree);
		ImmediateModeContext.Priority = ProgramPriority.Immediate;
		ImmediateModeContext.Pend();
		return ImmediateModeContext;
	}

	public IExecutionResult Compile(string programSource, string programName)
	{
		return ProgramCache.CompileStringSource(programName, programSource);
	}

	public List<IExecutionContext> GetJobList()
	{
		List<IExecutionContext> rv = new List<IExecutionContext>();
		Contexts.ForEach(delegate(ExecutionContext each)
		{
			rv.Add(each);
		});
		return rv;
	}

	public void SuspendAll(bool force)
	{
		Contexts.ForEach(delegate(ExecutionContext each)
		{
			if (!each.IsComplete)
			{
				each.Suspend();
				if (force)
				{
					if (each.Status == ExecutionContextStatus.Running)
					{
						each.PromoteDeferredStatusChange();
					}
					else
					{
						each.Save();
					}
				}
			}
		});
	}

	public string GetActiveContextProgramName()
	{
		if (ActiveContext == null || ActiveContext.Registers.ActiveFrame == null)
		{
			return "Idle";
		}
		return ActiveContext.Registers.ActiveFrame.ProgramName;
	}

	public void PushValue(IExecutionContext context, string value, StackEnd end, ErrorList errors)
	{
		Tokenizer tokenizer = new Tokenizer(1, string.Format("PUSH " + value));
		tokenizer.Tokenize();
		if (tokenizer.Errors.ContainsError())
		{
			errors.AddRange(tokenizer.Errors);
			return;
		}
		foreach (Token token in tokenizer.Tokens)
		{
			if (token.Type == TokenType.NumericLiteral || token.Type == TokenType.StringLiteral)
			{
				object instance = token.ConvertValue();
				context.Push(instance, end);
			}
		}
	}

	internal NativeJobAdapter MakeJobFromOperand(ExecutionContext ctx, ExecutionResult result, string key)
	{
		return ProgramCache.MakeJobFromOperand(ctx, result, key);
	}

	internal void PerformContextSwitch(bool runtimeOnly)
	{
		ActiveContext.PromoteDeferredStatusChange();
		if (!runtimeOnly)
		{
			ActiveContext.Save();
			ContextSwitchRequested = true;
			SchedulerResetEvent.Set();
		}
	}

	internal ExecutionContext ScheduleJobInternal(string progName, string label, DateTime? startTime, ProgramPriority priority)
	{
		string text = progName;
		ExecutionContext executionContext = new ExecutionContext(GetUniqueId(), text, startTime, priority, label);
		CompiledProgram program = ProgramCache.GetProgram(progName);
		if (program == null)
		{
			executionContext.Result.Errors.Add(Error.NewError("E001", $"The program '{text}' was not found in the program cache.", "Recompile the program which will place it in the program cache."));
			return executionContext;
		}
		using (new WithUpgradeableReadLock(ContextLock))
		{
			if (IsSingletonJob(progName))
			{
				ExecutionContext executionContext2 = Contexts.Find((ExecutionContext each) => each.ProgramName.Equals(progName, StringComparison.CurrentCultureIgnoreCase) && !each.IsComplete);
				if (executionContext2 != null)
				{
					string text2 = $"[ExecutionEngine] The program '{text}' is marked singleton, instance currently running - deny new job.";
					LogHelper.Instance.Log(text2);
					executionContext.Result.Errors.Add(Error.NewError("E001", text2, "Reschedule the program ."));
					return executionContext;
				}
			}
			executionContext.CopyProgramAndReferences(text, program.ParseTree);
			using (new WithWriteLock(ContextLock))
			{
				Contexts.Add(executionContext);
			}
		}
		executionContext.Save();
		return executionContext;
	}

	internal void ProcessScheduler()
	{
		while (true)
		{
			Thread.Sleep(0);
			if (!CanSchedule())
			{
				SuspendCritical(force: true);
			}
			if (SchedulerEnabled)
			{
				if (ContextSwitchRequested || ActiveContext == null)
				{
					ActiveContext = Contexts.GetEligibleExecutionContext();
					ContextSwitchRequested = false;
					SchedulerResetEvent.Reset();
				}
				if (ActiveContext != null && ActiveContext.Status != ExecutionContextStatus.Running)
				{
					ActiveContext.Execute();
					ActiveContext = null;
				}
			}
			SchedulerResetEvent.WaitOne(250, exitContext: false);
		}
	}

	private ExecutionEngine(string scriptsPath)
	{
		Mode = EngineModes.Normal;
		ScriptsPath = scriptsPath;
		RebootWakeupTimer = new RedboxTimer("Reboot Wakeup", OnRebootWakeup);
		IRuntimeService service = ServiceLocator.Instance.GetService<IRuntimeService>();
		DataDirectory = service.RuntimePath("data");
		ImmediateModeContext = new ExecutionContext("IMMEDIATE", "$$$immediate$$$", null, ProgramPriority.Immediate, "HAL Script Immediate Mode")
		{
			IsImmediate = true
		};
		Contexts.Add(ImmediateModeContext);
		InitStatus = ExecutionContextStatus.Pending;
	}

	private bool IsSingletonJob(string name)
	{
		return Array.FindIndex(SingletonJobs, (string singleton) => singleton.Equals(name, StringComparison.CurrentCultureIgnoreCase)) != -1;
	}

	private void SuspendCritical(bool force)
	{
		Contexts.FindAll((ExecutionContext each) => !each.IsComplete && each.Status != ExecutionContextStatus.Suspended).ForEach(delegate(ExecutionContext each)
		{
			if (!string.IsNullOrEmpty(Array.Find(NonSchedulableJobsWithinReboot, (string job) => job == each.ProgramName)))
			{
				each.Suspend();
				if (force)
				{
					if (each.Status == ExecutionContextStatus.Running)
					{
						each.PromoteDeferredStatusChange();
					}
					else
					{
						each.Save();
					}
				}
			}
		});
	}

	private bool CanSchedule()
	{
		if (!KioskRebootTime.HasValue || !RebootWindowStart.HasValue)
		{
			return true;
		}
		TimeSpan timeOfDay = DateTime.Now.TimeOfDay;
		if (timeOfDay >= RebootWindowStart.Value && timeOfDay <= KioskRebootTime.Value)
		{
			if (!RebootWakeupTimer.Started)
			{
				TimeSpan span = KioskRebootTime.Value.Add(new TimeSpan(0, 5, 0));
				RebootWakeupTimer.ScheduleAtNext(span);
				LogHelper.Instance.Log("Time {0} is within reboot window of {1}; disabling scheduler.", FormatTime(timeOfDay), FormatTime(KioskRebootTime.Value));
				PollerManager.StopPollers();
			}
			return false;
		}
		return true;
	}

	private string FormatTime(TimeSpan t)
	{
		return $"{DateTime.Now.ToShortDateString()} {t.ToString()}";
	}

	private string GetUniqueId()
	{
		using (new WithReadLock(ContextLock))
		{
			string iD = ImmediateModeContext.ID;
			for (int i = 0; i < 8; i++)
			{
				string text = IdGenerator.GeneratePseudoUniqueId(8);
				if (text != iD && Contexts.GetByID(text) == null)
				{
					return text;
				}
			}
		}
		throw new ApplicationException("Unable to generate a unique ID for ExecutionContext.");
	}

	private void OnRebootWakeup(object source, ElapsedEventArgs e)
	{
		RebootWakeupTimer.Disable();
		RestartYoungest(1);
		PollerManager.StartPollers();
	}

	private void ScheduleInit(bool isBootstrap)
	{
		InitStatus = ExecutionContextStatus.Pending;
		ExecutionContext executionContext = ScheduleJobInternal("init", "HAL Service Startup: Init", DateTime.Now, ProgramPriority.High);
		executionContext.JobCompleteEvent += InitJob_JobCompleteEvent;
		LogHelper.Instance.Log("[ExecutionEngine] Schedule init job {0}", executionContext.ID);
		if (isBootstrap)
		{
			executionContext.SetSymbolValue("__EE_BOOTSTRAP_INIT", true);
		}
		executionContext.Pend();
	}

	private void InitJob_JobCompleteEvent(ExecutionContextStatus status)
	{
		PollerManager.StartPollers();
		InitStatus = status;
	}

	private void RestartYoungest(int delay)
	{
		string[] nonSchedulableJobsWithinReboot = NonSchedulableJobsWithinReboot;
		foreach (string name in nonSchedulableJobsWithinReboot)
		{
			ExecutionContext[] byName = Contexts.GetByName(name);
			if (byName.Length == 0)
			{
				continue;
			}
			Array.Sort(byName, (ExecutionContext left, ExecutionContext right) => left.CreatedOn.CompareTo(right.CreatedOn));
			ExecutionContext executionContext = null;
			for (int num = byName.Length - 1; num >= 0; num--)
			{
				executionContext = byName[num];
				if (!executionContext.IsComplete)
				{
					_ = DateTime.Now;
					if (!executionContext.StartTime.HasValue || executionContext.StartTime.Value < DateTime.Now.AddMinutes(delay))
					{
						executionContext.StartTime = DateTime.Now.AddMinutes(delay);
					}
					LogHelper.Instance.Log("[Execution Engine] Restarting job {0} ( ID = {1} )", executionContext.ProgramName, executionContext.ID);
					executionContext.Status.NextValue = ExecutionContextStatus.Pending;
					executionContext.Status.Promote();
					break;
				}
			}
		}
	}

	private void LoadJobs()
	{
		string path = Path.Combine(DataDirectory, "contexts");
		try
		{
			if (!Directory.Exists(path))
			{
				LogHelper.Instance.Log("[ExecutionEngine] Contexts directory doesn't exist; creating.", LogEntryType.Debug);
				Directory.CreateDirectory(path);
			}
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("[ExecutionEngine] Unable to create contexts directory.", e);
		}
		Array.ForEach(Directory.GetDirectories(path), delegate(string contextPath)
		{
			string path2 = Path.Combine(contextPath, "ctx.data");
			ExecutionContext executionContext = null;
			try
			{
				using (FileStream serializationStream = File.OpenRead(path2))
				{
					executionContext = (ExecutionContext)new BinaryFormatter().Deserialize(serializationStream);
				}
				if (executionContext != null)
				{
					if (executionContext.Status.Value == ExecutionContextStatus.Suspended && Array.IndexOf(NonRestartableJobs, executionContext.ProgramName) > -1)
					{
						LogHelper.Instance.Log("[ExecutionEngine] Job {0} ( {1} ) is in pending status at initialization; mark it garbage.", executionContext.ID, executionContext.ProgramName);
						executionContext.Status.NextValue = ExecutionContextStatus.Garbage;
						executionContext.Status.Promote();
					}
					Contexts.Add(executionContext);
				}
			}
			catch
			{
				LogHelper.Instance.Log("[ExecutionEngine] Unable to load context {0}; deleting.", contextPath);
				try
				{
					Directory.Delete(contextPath, recursive: true);
				}
				catch (Exception e2)
				{
					LogHelper.Instance.Log("[ExecutionEngine] Unable to delete context", e2);
				}
			}
		});
	}

	private void OnModeChanged()
	{
		EngineModeChange?.Invoke(this, new EngineModeChangeEventArgs(Mode));
	}
}
