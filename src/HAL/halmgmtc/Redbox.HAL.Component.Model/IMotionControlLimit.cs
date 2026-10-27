namespace Redbox.HAL.Component.Model;

public interface IMotionControlLimit
{
	MotionControlLimits Limit { get; }

	bool Blocked { get; }
}
