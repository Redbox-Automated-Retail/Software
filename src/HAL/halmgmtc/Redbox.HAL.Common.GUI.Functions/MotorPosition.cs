namespace Redbox.HAL.Common.GUI.Functions;

public struct MotorPosition
{
	public readonly MoveAxis Axis;

	public int Position { get; internal set; }

	public bool ReadOk { get; internal set; }

	public MotorPosition(MoveAxis axis)
	{
		this = default(MotorPosition);
		Axis = axis;
	}
}
