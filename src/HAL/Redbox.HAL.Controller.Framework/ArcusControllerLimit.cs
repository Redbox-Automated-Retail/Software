using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Controller.Framework;

internal sealed class ArcusControllerLimit : IMotionControlLimit
{
	public MotionControlLimits Limit { get; private set; }

	public bool Blocked { get; private set; }

	internal ArcusControllerLimit(MotionControlLimits n, bool b)
	{
		Limit = n;
		Blocked = b;
	}
}
