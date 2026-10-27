using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Controller.Framework;

internal sealed class DumpbinPutObserver : IPutToObserver, IPutObserver
{
	public void OnSuccessfulPut(IPutResult res, IFormattedLog log)
	{
		IDumpbinService service = ServiceLocator.Instance.GetService<IDumpbinService>();
		IInventoryService service2 = ServiceLocator.Instance.GetService<IInventoryService>();
		service.AddBinItem(res.StoredMatrix);
		ServiceLocator.Instance.GetService<IMotionControlService>().MoveTo(service.RotationLocation, MoveMode.None, log);
		service2.Reset(service.PutLocation);
	}

	public void OnFailedPut(IPutResult res, IFormattedLog log)
	{
	}

	public void OnMoveError(ErrorCodes e)
	{
	}
}
