namespace Redbox.HAL.Component.Model;

public interface IGetFromObserver : IGetObserver
{
	void OnMoveError(ErrorCodes error);
}
