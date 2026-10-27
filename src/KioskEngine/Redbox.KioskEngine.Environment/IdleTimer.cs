using System;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;

namespace Redbox.KioskEngine.Environment;

public class IdleTimer : IIdleTimer
{
	private string _name;

	private bool _isEnabled;

	private bool _isSuspended;

	private bool _isRegistered;

	private bool _isTimerStarted;

	public string Name => _name;

	public int? Timeout
	{
		get
		{
			return (ServiceLocator.Instance.GetService<ITimerService>()?.GetTimer(Name))?.Period;
		}
		set
		{
			ITimer timer = ServiceLocator.Instance.GetService<ITimerService>().GetTimer(Name);
			if (timer != null)
			{
				timer.Period = value;
				timer.DueTime = value;
			}
		}
	}

	public IdleTimer(string name)
	{
		_name = name;
	}

	public void Initialize(int timeout, Action action)
	{
		ServiceLocator.Instance.GetService<ITimerService>()?.CreateTimer(Name, timeout, timeout, delegate
		{
			action();
		});
	}

	public void Start()
	{
		if (!_isEnabled)
		{
			LogHelper.Instance.Log("idle timer {0} started.", Name);
			_isEnabled = true;
			IInputService service = ServiceLocator.Instance.GetService<IInputService>();
			service?.RegisterMouseClickHandler(Name, delegate
			{
				Reset();
			});
			service?.RegisterKeyPressHandler(Name, delegate
			{
				Reset();
			});
			Reset();
		}
	}

	public void Stop()
	{
		if (_isEnabled)
		{
			LogHelper.Instance.Log("idle timer {0} stopped.", Name);
			StopIdleTimer();
			IInputService service = ServiceLocator.Instance.GetService<IInputService>();
			service?.RemoveKeyPressHandler(Name);
			service?.RemoveMouseClickHandler(Name);
			service?.RemoveIdleHandler(Name);
			_isRegistered = false;
			_isEnabled = false;
		}
	}

	public void Suspend()
	{
		if (_isEnabled)
		{
			_isSuspended = true;
			Stop();
		}
	}

	public void Resume()
	{
		if (_isSuspended)
		{
			_isSuspended = false;
			Start();
		}
	}

	public void Reset()
	{
		StopIdleTimer();
		if (!_isRegistered)
		{
			_isRegistered = true;
			IInputService inputService = ServiceLocator.Instance.GetService<IInputService>();
			inputService.RegisterIdleHandler(Name, delegate
			{
				_isRegistered = false;
				inputService.RemoveIdleHandler(Name);
				StartIdleTimer();
				LogHelper.Instance.Log("system is idle, timer: {0}", Name);
			});
		}
	}

	private void StartIdleTimer()
	{
		if (!_isTimerStarted)
		{
			_isTimerStarted = true;
			(ServiceLocator.Instance.GetService<ITimerService>()?.GetTimer(Name))?.Start();
		}
	}

	private void StopIdleTimer()
	{
		if (_isTimerStarted)
		{
			_isTimerStarted = false;
			(ServiceLocator.Instance.GetService<ITimerService>()?.GetTimer(Name))?.Stop();
		}
	}
}
