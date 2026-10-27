using System;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Controller.Framework;

internal sealed class ArcusLimitResponse : IMotionControlLimitResponse
{
	public bool ReadOk { get; private set; }

	public IMotionControlLimit[] Limits { get; private set; }

	public bool IsLimitBlocked(MotionControlLimits limit)
	{
		if (!ReadOk || Limits == null || limit == MotionControlLimits.None)
		{
			throw new InvalidOperationException("Limits not setup.");
		}
		IMotionControlLimit[] limits = Limits;
		foreach (IMotionControlLimit motionControlLimit in limits)
		{
			if (motionControlLimit.Limit == limit)
			{
				return motionControlLimit.Blocked;
			}
		}
		LogHelper.Instance.Log("[ArcusLimitResponse] Unable to find limit {0}", limit.ToString());
		return true;
	}

	internal ArcusLimitResponse(string queryResponse)
	{
		ReadOk = false;
		Limits = null;
		if (queryResponse != null)
		{
			string[] array = queryResponse.Trim().Split(',');
			if (array.Length == 28 && int.TryParse(array[13].TrimEnd('.'), out var result))
			{
				ReadOk = true;
				Limits = new IMotionControlLimit[2];
				Limits[0] = new ArcusControllerLimit(MotionControlLimits.Upper, (result & 0x10) != 0);
				Limits[1] = new ArcusControllerLimit(MotionControlLimits.Lower, (result & 0x20) != 0);
			}
		}
	}
}
