using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Timers;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Rental.Model;

namespace Redbox.KioskEngine.Environment;

public class CallbackService : ICallbackService
{
	public class CallbackServiceStatistics : ICallbackServiceStatistics
	{
		public int NumberOfCallbacksExecuted { get; set; }

		public TimeSpan TotalCallbackDuration { get; set; }

		public TimeSpan MaxCallbackDuration { get; set; }

		public TimeSpan AverageCallbackDuration { get; set; }

		public TimeSpan TimeSinceLastLogged { get; set; }

		public string MaxCallbackDurationMessage { get; set; }
	}

	private class CallbackEntryWrapper
	{
		public ICallbackEntry CallbackEntry { get; set; }

		public DateTime? EnqueueDateTime { get; set; }

		public string CallBackName { get; set; }

		public string AddedFromStack { get; set; }

		public DateTime? DequeueDateTime { get; set; }

		public long ExecutionTime { get; set; }

		public bool Completed { get; set; }

		public string Stack { get; set; }

		public override string ToString()
		{
			return new
			{
				Name = CallBackName,
				EnqueueDT = EnqueueDateTime?.ToString("o"),
				DequeueDT = DequeueDateTime?.ToString("o"),
				ExecutionTime = ExecutionTime,
				Completed = Completed,
				Stack = Stack
			}.ToJson();
		}
	}

	private bool m_isRunning;

	private readonly object m_syncObject = new object();

	private readonly Queue<CallbackEntryWrapper> m_entries = new Queue<CallbackEntryWrapper>();

	private Timer _waitTimer;

	private Queue<CallbackEntryWrapper> _callbackTracking = new Queue<CallbackEntryWrapper>();

	private int _numberOfCallbacks;

	private TimeSpan _maxCallbackDuration;

	private TimeSpan _totalCallbackDuration;

	private string _maxCallbackDurationMessage;

	private DateTime _lastLoggedDateTime = DateTime.Now;

	private TimeSpan _loggingTargetInterval = TimeSpan.FromSeconds(20.0);

	public static CallbackService Instance => Singleton<CallbackService>.Instance;

	public int QueueCount
	{
		get
		{
			lock (m_syncObject)
			{
				return m_entries.Count;
			}
		}
	}

	public bool UseTimerToInvokeCallbacks => ServiceLocator.Instance.GetService<IConfiguration>()?.UseTimerToInvokeCallbackEntries ?? true;

	public TimeSpan LoggingTargetInterval => _loggingTargetInterval;

	public void Reset()
	{
		lock (m_syncObject)
		{
			LogHelper.Instance.Log($"Clear callback service queue.  Current queue count: {m_entries.Count}");
			m_entries.Clear();
			ResetStatistics();
		}
	}

	public void Flush()
	{
		lock (m_syncObject)
		{
			LogHelper.Instance.Log($"Flush callback service queue. Current queue count: {m_entries.Count}");
			while (InvokeNextCallback())
			{
			}
		}
	}

	public void Resume()
	{
		lock (m_syncObject)
		{
			LogHelper.Instance.Log($"Resume callback service queue.  Current queue count: {m_entries.Count}");
			m_isRunning = true;
			StartTimer();
		}
	}

	public void Suspend()
	{
		lock (m_syncObject)
		{
			LogHelper.Instance.Log($"Suspend callback service queue.  Current queue count: {m_entries.Count}");
			m_isRunning = false;
		}
	}

	public DateTime? GetEnqueuedDateTimeFromNextCallbackEntry()
	{
		DateTime? result = null;
		lock (m_syncObject)
		{
			if (m_entries.Count > 0)
			{
				CallbackEntryWrapper callbackEntryWrapper = m_entries.Peek();
				if (callbackEntryWrapper != null)
				{
					result = callbackEntryWrapper.EnqueueDateTime;
					return result;
				}
			}
		}
		return result;
	}

	public bool InvokeNextCallback()
	{
		try
		{
			lock (m_syncObject)
			{
				if (!m_isRunning)
				{
					return false;
				}
				if (m_entries.Count == 0)
				{
					return false;
				}
				CallbackEntryWrapper callbackEntryWrapper = m_entries.Dequeue();
				callbackEntryWrapper.DequeueDateTime = DateTime.Now;
				Stopwatch stopwatch = Stopwatch.StartNew();
				callbackEntryWrapper?.CallbackEntry.Invoke();
				stopwatch.Stop();
				callbackEntryWrapper.ExecutionTime = stopwatch.ElapsedMilliseconds;
				callbackEntryWrapper.Completed = true;
				if (callbackEntryWrapper.ExecutionTime < 2000)
				{
					callbackEntryWrapper.Stack = string.Empty;
				}
				UpdateStatistics(stopwatch.Elapsed, callbackEntryWrapper?.CallbackEntry?.Name);
				if (callbackEntryWrapper?.CallbackEntry is IDisposable disposable)
				{
					disposable.Dispose();
				}
				callbackEntryWrapper.CallbackEntry = null;
				TrimCallback();
			}
			if (GetTimeSinceLastLogged() >= LoggingTargetInterval)
			{
				LogStatistics();
				ResetStatistics();
			}
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("An unhandled exception was rasied by CallbackService.InvokeNextCallback.", e);
		}
		return true;
	}

	private void TrimCallback()
	{
		while (_callbackTracking.Count > 300)
		{
			_callbackTracking.Dequeue();
		}
	}

	private void UpdateStatistics(TimeSpan callbackDuration, string callbackMessage)
	{
		_numberOfCallbacks++;
		_totalCallbackDuration = _totalCallbackDuration.Add(callbackDuration);
		if (callbackDuration > _maxCallbackDuration)
		{
			_maxCallbackDuration = callbackDuration;
			_maxCallbackDurationMessage = callbackMessage;
		}
	}

	public ICallbackServiceStatistics GetStatics()
	{
		return new CallbackServiceStatistics
		{
			NumberOfCallbacksExecuted = _numberOfCallbacks,
			TotalCallbackDuration = _totalCallbackDuration,
			MaxCallbackDuration = _maxCallbackDuration,
			MaxCallbackDurationMessage = _maxCallbackDurationMessage,
			AverageCallbackDuration = GetAverageCallbackDuration(),
			TimeSinceLastLogged = GetTimeSinceLastLogged()
		};
	}

	public void EnqueueCallback(ICallbackEntry callbackEntry)
	{
		lock (m_syncObject)
		{
			if (!m_isRunning)
			{
				LogHelper.Instance.Log("Enqueing of callback entry rejected because the Callback queue is suspended.");
				return;
			}
			CallbackEntryWrapper item = new CallbackEntryWrapper
			{
				CallbackEntry = callbackEntry,
				EnqueueDateTime = DateTime.Now,
				CallBackName = callbackEntry.Name,
				Stack = System.Environment.StackTrace
			};
			m_entries.Enqueue(item);
			_callbackTracking.Enqueue(item);
			StartTimer();
		}
	}

	private CallbackService()
	{
	}

	public void LogCallbackTracking()
	{
		_callbackTracking.ForEach(delegate(CallbackEntryWrapper x)
		{
			LogHelper.Instance.Log(x.ToString());
		});
	}

	public void ResetStatistics()
	{
		_numberOfCallbacks = 0;
		_maxCallbackDuration = TimeSpan.FromMilliseconds(0.0);
		_totalCallbackDuration = TimeSpan.FromMilliseconds(0.0);
		_lastLoggedDateTime = DateTime.Now;
		_maxCallbackDurationMessage = null;
	}

	public void LogStatistics()
	{
		int queueCount = QueueCount;
		ICallbackServiceStatistics statics = GetStatics();
		LogHelper.Instance.Log($"CallbackService statistics: current queue count: {queueCount}; callbacks invoked since last check: {statics.NumberOfCallbacksExecuted}; Time since Last callback statistics logged: {statics.TimeSinceLastLogged}");
		LogHelper.Instance.Log($"CallbackService statistics: average execution time: {statics.AverageCallbackDuration}; total execution time: {statics.TotalCallbackDuration}; max execution time: {statics.MaxCallbackDuration}; max execution time callback name: {statics.MaxCallbackDurationMessage} ");
	}

	private TimeSpan GetAverageCallbackDuration()
	{
		TimeSpan result = TimeSpan.FromMilliseconds(0.0);
		if (_numberOfCallbacks > 0)
		{
			result = TimeSpan.FromMilliseconds(_totalCallbackDuration.TotalMilliseconds / (double)_numberOfCallbacks);
		}
		return result;
	}

	private TimeSpan GetTimeSinceLastLogged()
	{
		return DateTime.Now - _lastLoggedDateTime;
	}

	private void StartTimer()
	{
		if (UseTimerToInvokeCallbacks)
		{
			if (_waitTimer == null)
			{
				_waitTimer = new Timer(5.0);
				_waitTimer.Elapsed += _waitTimer_Elapsed;
				_waitTimer.AutoReset = false;
			}
			if (!_waitTimer.Enabled)
			{
				_waitTimer.Start();
			}
		}
	}

	private void _waitTimer_Elapsed(object sender, ElapsedEventArgs e)
	{
		ServiceLocator.Instance.GetService<IEngineApplication>()?.ThreadSafeHostUpdate(delegate
		{
			InvokeNextCallback();
		});
		lock (m_syncObject)
		{
			if (m_isRunning && m_entries.Count > 0)
			{
				StartTimer();
			}
		}
	}
}
