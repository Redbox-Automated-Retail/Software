using System;
using DeviceService.ComponentModel;
using DeviceService.ComponentModel.Commands;
using DeviceService.ComponentModel.Responses;

namespace DeviceService.Client.Jobs;

public abstract class BaseReadCardJob : IReadCardJob
{
	protected IDeviceServiceClient _deviceServiceClient;

	protected bool _inProcess;

	protected bool _readCompleted;

	protected bool _cancelled;

	protected readonly Guid _readCardCommandRequestId = Guid.NewGuid();

	protected Action<BaseResponseEvent> _jobCompletedCallback;

	protected Action<BaseResponseEvent> _readJobEvents;

	protected int? _timeout;

	public Guid RequestId => _readCardCommandRequestId;

	public BaseReadCardJob(IDeviceServiceClient deviceServiceClient, Action<BaseResponseEvent> jobCompleteCallback, Action<BaseResponseEvent> readJobEvents, int? timeout)
	{
		BaseReadCardJob baseReadCardJob = this;
		_deviceServiceClient = deviceServiceClient;
		_jobCompletedCallback = delegate(BaseResponseEvent baseResponseEvent)
		{
			baseReadCardJob._readCompleted = true;
			jobCompleteCallback(baseResponseEvent);
		};
		_readJobEvents = readJobEvents;
		_timeout = timeout;
	}

	protected abstract void PerformCardRead();

	public bool Execute()
	{
		bool result = false;
		if (!_cancelled && !_inProcess)
		{
			_inProcess = true;
			PerformCardRead();
			result = true;
		}
		return result;
	}

	public bool Cancel(Action<BaseResponseEvent> cancelCompleteCallback)
	{
		bool result = false;
		if (!_readCompleted && !_cancelled)
		{
			_cancelled = true;
			result = true;
			_deviceServiceClient.CancelCommand(_readCardCommandRequestId, cancelCompleteCallback);
		}
		return result;
	}
}
