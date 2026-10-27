using System;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Client.Services;

internal sealed class LimitResponse : IMotionControlLimitResponse
{
	public bool ReadOk { get; private set; }

	public IMotionControlLimit[] Limits { get; private set; }

	public bool IsLimitBlocked(MotionControlLimits limit)
	{
		throw new NotImplementedException();
	}

	internal LimitResponse(bool readOk, IMotionControlLimit[] limits)
	{
		ReadOk = readOk;
		Limits = limits;
	}
}
