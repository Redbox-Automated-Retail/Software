using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Controller.Framework;

internal sealed class DefaultPutObserver : IPutToObserver, IPutObserver
{
	public void OnSuccessfulPut(IPutResult result, IFormattedLog log)
	{
	}

	public void OnFailedPut(IPutResult result, IFormattedLog log)
	{
	}

	public void OnMoveError(ErrorCodes error)
	{
	}
}
