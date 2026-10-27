namespace Redbox.HAL.Component.Model;

public interface IPutToObserver : IPutObserver
{
	void OnMoveError(ErrorCodes error);
}
