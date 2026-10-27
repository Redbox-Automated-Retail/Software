using System;
using DeviceService.ComponentModel.Responses;

namespace DeviceService.ComponentModel.Commands;

public interface IReadCardJob
{
	Guid RequestId { get; }

	bool Execute();

	bool Cancel(Action<BaseResponseEvent> cancelCompleteCallback);
}
