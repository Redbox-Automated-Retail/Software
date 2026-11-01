using System.Threading;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Controller.Framework;

namespace Redbox.HAL.Script.Framework.Natives;

[NativeJob(ProgramName = "one-disk-random-sync", HideFromList = true)]
internal sealed class OneDiskRandomSyncJob : NativeJobAdapter
{
	protected override void ExecuteInner()
	{
		ApplicationLog applicationLog = ApplicationLog.ConfigureLog(base.Context, logAppends: false, "RandomSync", flushOnWrite: true, null);
		int num = base.Context.PopTop<int>();
		int num2 = base.Context.PopTop<int>();
		IControlSystem service = ServiceLocator.Instance.GetService<IControlSystem>();
		IMotionControlService motionControlService = ServiceLocator.Instance.GetService<IMotionControlService>();
		IControllerService controllerService = ServiceLocator.Instance.GetService<IControllerService>();
		base.Context.AppLog.WriteFormatted("Vend time is set to {0}s, vend frequency = {1}", num2, num);
		base.Context.AppLog.Write("One disk random sync test start.");
		RandomLocationSelector randomLocationSelector = new RandomLocationSelector(base.DecksService, base.InventoryService);
		bool isVMZMachine = ControllerConfiguration.Instance.IsVMZMachine;
		int num3 = 0;
		bool flag = false;
		try
		{
			if (motionControlService.MoveTo(1, 1, MoveMode.Get, base.Context.AppLog) != ErrorCodes.Success)
			{
				flag = true;
				return;
			}
			if (!controllerService.Get().Success)
			{
				flag = true;
				return;
			}
			while (!flag)
			{
				ILocation location = randomLocationSelector.SelectTargetLocation();
				if (location == null)
				{
					break;
				}
				if (location.Excluded)
				{
					base.Context.AppLog.WriteFormatted("Not syncing excluded location {0}", location);
					continue;
				}
				if (!isVMZMachine && location.Deck == 8)
				{
					if (base.ControlSystem.EngageQlm(base.Context.AppLog) != ErrorCodes.Success)
					{
						flag = true;
						continue;
					}
					Thread.Sleep(1500);
					if (base.ControlSystem.DisengageQlm(base.Context.AppLog) != ErrorCodes.Success)
					{
						flag = true;
					}
					continue;
				}
				base.Context.AppLog.WriteFormatted("Sync {0}", location.ToString());
				base.Context.Send($"Iteration {++num3}");
				if (num != 0)
				{
					_ = num3 % num == 0;
				}
				else
					_ = 0;
				string id = "UNKNOWN";
				using (Scanner scanner = new Scanner(CenterDiskMethod.VendDoorAndBack, service))
				{
					using ScanResult scanResult = scanner.Read();
					id = scanResult.ScannedMatrix;
					if (!scanResult.SnapOk)
					{
						base.Context.CreateCameraCaptureErrorResult();
						flag = true;
					}
				}
				if (motionControlService.MoveTo(location, MoveMode.Put, base.Context.AppLog) != ErrorCodes.Success)
				{
					flag = true;
					break;
				}
				if (!controllerService.Put(id).Success)
				{
					flag = true;
					break;
				}
				if (motionControlService.MoveTo(location, MoveMode.Get, base.Context.AppLog) != ErrorCodes.Success)
				{
					flag = true;
					break;
				}
				if (!controllerService.Get().Success || OnVendReceive(num2) != ErrorCodes.Success)
				{
					flag = true;
				}
			}
			if (!flag)
			{
				if (motionControlService.MoveTo(1, 1, MoveMode.Put, base.Context.AppLog) != ErrorCodes.Success)
				{
					flag = true;
				}
				else if (!controllerService.Put("UNKNOWN").Success)
				{
					flag = true;
				}
			}
		}
		finally
		{
			applicationLog.WriteFormatted("Job visited {0} locations", num3);
			if (flag)
			{
				AddError("Job error");
			}
		}
	}

	internal OneDiskRandomSyncJob(ExecutionResult result, ExecutionContext context)
		: base(result, context)
	{
	}

	private ErrorCodes OnVendReceive(int vendTime)
	{
		ErrorCodes errorCodes = ServiceLocator.Instance.GetService<IMotionControlService>().MoveVend(MoveMode.None, base.Context.AppLog);
		if (errorCodes != ErrorCodes.Success)
		{
			return errorCodes;
		}
		IControllerService service = ServiceLocator.Instance.GetService<IControllerService>();
		IVendItemResult vendItemResult = service.VendItemInPicker(vendTime);
		if (!vendItemResult.Presented)
		{
			return ErrorCodes.PickerObstructed;
		}
		if (vendItemResult.Status != ErrorCodes.PickerFull)
		{
			return vendItemResult.Status;
		}
		ErrorCodes errorCodes2 = service.AcceptDiskAtDoor();
		if (ErrorCodes.PickerFull != errorCodes2)
		{
			return errorCodes2;
		}
		return ErrorCodes.Success;
	}
}
