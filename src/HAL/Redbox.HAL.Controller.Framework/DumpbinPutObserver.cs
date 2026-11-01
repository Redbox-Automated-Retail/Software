using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Controller.Framework;

internal sealed class DumpbinPutObserver : IPutToObserver, IPutObserver
{
	public void OnSuccessfulPut(IPutResult res, IFormattedLog log)
	{
		IDumpbinService dumpbinService = ServiceLocator.Instance.GetService<IDumpbinService>();
		IInventoryService inventoryService = ServiceLocator.Instance.GetService<IInventoryService>();
		dumpbinService.AddBinItem(res.StoredMatrix);
		ServiceLocator.Instance.GetService<IMotionControlService>().MoveTo(dumpbinService.RotationLocation, MoveMode.None, log);
		inventoryService.Reset(dumpbinService.PutLocation);
	}

	public void OnFailedPut(IPutResult res, IFormattedLog log)
	{
	}

	public void OnMoveError(ErrorCodes e)
	{
	}
}
