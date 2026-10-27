using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Script.Framework.Natives;

internal class RandomSyncDecorator : SyncDecorator
{
	private int m_secureReads;

	private readonly bool VendDisk;

	private readonly int VendTime;

	internal int SecureReads => m_secureReads;

	protected override void OnReadDisk(ScanResult result)
	{
		if (result.ReadCode)
		{
			m_secureReads += result.SecureTokensFound;
		}
	}

	internal RandomSyncDecorator(ExecutionContext ctx, ExecutionResult r, ILocation loc, int vendtime, bool vend)
		: base(ctx, r, loc)
	{
		VendDisk = vend;
		VendTime = vendtime;
	}

	internal override SyncResult AfterDiskRead(ScanResult sr)
	{
		if (!VendDisk)
		{
			return SyncResult.Success;
		}
		if (ServiceLocator.Instance.GetService<IMotionControlService>().MoveVend(MoveMode.None, Context.AppLog) != ErrorCodes.Success)
		{
			Context.CreateMoveErrorResult(SyncLocation.Deck, SyncLocation.Slot);
			AddError("job error.");
			return SyncResult.HardwareError;
		}
		IControllerService service = ServiceLocator.Instance.GetService<IControllerService>();
		IVendItemResult vendItemResult = service.VendItemInPicker(VendTime);
		if (!vendItemResult.Presented)
		{
			AddError("Job error.");
			return SyncResult.HardwareError;
		}
		if (vendItemResult.Status != ErrorCodes.PickerFull)
		{
			AddError("Job error.");
			return SyncResult.HardwareError;
		}
		ErrorCodes errorCodes = service.AcceptDiskAtDoor();
		if (ErrorCodes.PickerFull != errorCodes)
		{
			AddError("Job error.");
			return SyncResult.HardwareError;
		}
		IControlSystem service2 = ServiceLocator.Instance.GetService<IControlSystem>();
		service2.TrackCycle();
		SyncResult result = SyncResult.Success;
		using (Scanner scanner = new Scanner(service2))
		{
			using ScanResult scanResult = scanner.Read();
			if (!scanResult.SnapOk)
			{
				Context.CreateCameraCaptureErrorResult();
				result = SyncResult.HardwareError;
			}
		}
		return result;
	}
}
