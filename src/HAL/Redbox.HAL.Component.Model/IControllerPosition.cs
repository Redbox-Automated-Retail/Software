namespace Redbox.HAL.Component.Model;

public interface IControllerPosition
{
	int? XCoordinate { get; }

	int? YCoordinate { get; }

	bool ReadOk { get; }
}
