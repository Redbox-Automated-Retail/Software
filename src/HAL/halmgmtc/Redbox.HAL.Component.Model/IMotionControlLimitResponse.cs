namespace Redbox.HAL.Component.Model;

public interface IMotionControlLimitResponse
{
	bool ReadOk { get; }

	IMotionControlLimit[] Limits { get; }

	bool IsLimitBlocked(MotionControlLimits limit);
}
