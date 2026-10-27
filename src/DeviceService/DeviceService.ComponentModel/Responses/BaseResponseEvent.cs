using System;
using DeviceService.ComponentModel.Commands;

namespace DeviceService.ComponentModel.Responses;

public abstract class BaseResponseEvent : BaseEvent
{
	public Guid RequestId { get; set; }

	public bool Success { get; set; }

	public BaseResponseEvent(BaseCommandRequest request)
	{
		if (request != null)
		{
			RequestId = request.RequestId;
		}
	}
}
