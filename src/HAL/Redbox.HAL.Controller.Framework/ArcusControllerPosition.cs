using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Controller.Framework;

internal class ArcusControllerPosition : IControllerPosition
{
	public int? XCoordinate { get; internal set; }

	public int? YCoordinate { get; internal set; }

	public bool ReadOk
	{
		get
		{
			if (XCoordinate.HasValue)
			{
				return YCoordinate.HasValue;
			}
			return false;
		}
	}

	internal ArcusControllerPosition()
	{
		XCoordinate = (YCoordinate = null);
	}
}
