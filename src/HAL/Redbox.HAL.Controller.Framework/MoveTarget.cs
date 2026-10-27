using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Controller.Framework;

internal struct MoveTarget
{
	internal Axis Axis;

	internal int? XCoordinate;

	internal int? YCoordinate;
}
