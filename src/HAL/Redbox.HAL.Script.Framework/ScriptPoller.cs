using System;
using System.Threading;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Component.Model.Threading;

namespace Redbox.HAL.Script.Framework;

internal abstract class ScriptPoller : IConfigurationObserver
{
	protected readonly ManualResetEvent PollEvent = new ManualResetEvent(initialState: false);

	private readonly object SyncLock = new object();

	private readonly AtomicFlag RunFlag = new AtomicFlag();

	protected abstract string ThreadName { get; }

	protected abstract string JobName { get; }

	protected virtual string JobLabel => string.Empty;

	protected virtual bool IsConfigured => true;

	protected abstract int PollSleep { get; }

	protected abstract ProgramPriority Priority { get; }

	protected Thread PollerThread { get; private set; }

	internal bool Running
	{
		get
		{
			return RunFlag.IsSet;
		}
		set
		{
			if (value)
			{
				RunFlag.Set();
			}
			else
			{
				RunFlag.Clear();
			}
		}
	}

	internal string PollerName { get; set; }

	internal bool ConfigNotifications { get; set; }

	public void NotifyConfigurationLoaded()
	{
		LogHelper.Instance.Log("[{0}Poller] Configuration loaded.", PollerName);
		OnConfigurationLoad();
	}

	public void NotifyConfigurationChangeStart()
	{
		LogHelper.Instance.Log("[{0}Poller] Configuration change start.", PollerName);
		Shutdown();
	}

	public void NotifyConfigurationChangeEnd()
	{
		LogHelper.Instance.Log("[{0}Poller] Configuration change end.", PollerName);
		OnConfigurationChangeEnd();
		if (ExecutionEngine.Instance.Mode == EngineModes.Normal)
		{
			Start();
		}
	}

	protected ExecutionContextStatus ScheduleAndBlockOnJob()
	{
		ExecutionContext executionContext = ExecutionEngine.Instance.ScheduleJobInternal(JobName, JobLabel, null, Priority);
		executionContext.Pend();
		executionContext.WaitForCompletion();
		ExecutionContextStatus status = executionContext.GetStatus();
		executionContext.Trash();
		return status;
	}

	protected virtual void OnConfigurationLoad()
	{
	}

	protected virtual void OnConfigurationChangeEnd()
	{
	}

	protected virtual bool OnPollStart()
	{
		return true;
	}

	protected virtual bool CoreExecute()
	{
		ExecutionContextStatus executionContextStatus = ScheduleAndBlockOnJob();
		return ExecutionContextStatus.Completed == executionContextStatus;
	}

	protected ScriptPoller()
	{
		ServiceLocator.Instance.GetService<IExecutionService>().EngineModeChange += es_EngineModeChange;
	}

	internal void Initialize()
	{
		if (ConfigNotifications)
		{
			ServiceLocator.Instance.GetService<IConfigurationService>().FindConfiguration(Configurations.Controller).AddObserver(this);
		}
	}

	internal void Start()
	{
		lock (SyncLock)
		{
			if (IsConfigured && !Running)
			{
				PollEvent.Reset();
				Running = true;
				PollerThread = new Thread(Execute)
				{
					Name = ThreadName,
					IsBackground = true
				};
				PollerThread.SetApartmentState(ApartmentState.STA);
				PollerThread.Start();
			}
		}
	}

	internal void Shutdown()
	{
		Shutdown(joinThread: true);
	}

	internal void Shutdown(bool joinThread)
	{
		lock (SyncLock)
		{
			if (IsConfigured && Running)
			{
				Running = false;
				PollEvent.Set();
				if (joinThread && PollerThread != null && !PollerThread.Join(5000))
				{
					LogHelper.Instance.Log("[ScriptPoller] Shutdown: thread '{0}' did not join.", ThreadName);
				}
				PollerThread = null;
			}
		}
	}

	private void es_EngineModeChange(object sender, EngineModeChangeEventArgs args)
	{
		if (IsConfigured)
		{
			if (args.NewMode == EngineModes.Normal)
			{
				Start();
			}
			else
			{
				Shutdown(joinThread: false);
			}
		}
	}

	private void Execute()
	{
		try
		{
			if (OnPollStart())
			{
				LogHelper.Instance.Log("[ScriptPoller] Start of the {0} thread.", ThreadName);
				while (Running && CoreExecute() && !PollEvent.WaitOne(PollSleep))
				{
				}
			}
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log($"{ThreadName} thread exiting due to exception.", e);
		}
		finally
		{
			LogHelper.Instance.Log("[ScriptPoller] {0} thread has ended.", ThreadName);
			Running = false;
		}
	}
}
