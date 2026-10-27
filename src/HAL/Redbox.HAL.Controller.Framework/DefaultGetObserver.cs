using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Controller.Framework;

public sealed class DefaultGetObserver : IGetFromObserver, IGetObserver
{
	public void OnStuck(IGetResult result)
	{
	}

	public bool OnEmpty(IGetResult result)
	{
		return true;
	}

	public void OnMoveError(ErrorCodes error)
	{
	}
}
