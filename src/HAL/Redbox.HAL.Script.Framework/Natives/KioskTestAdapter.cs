using System;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Script.Framework.Natives;

internal class KioskTestAdapter : IDisposable
{
	protected readonly IControlSystem ControlSystem;

	protected readonly IMotionControlService MotionService;

	protected readonly ExecutionContext Context;

	protected readonly ExecutionResult Result;

	private readonly ApplicationLog AppLog;

	public void Dispose()
	{
	}

	protected virtual ErrorCodes OnUnitComplete()
	{
		return ErrorCodes.Success;
	}

	protected virtual bool OnVendDisk(string matrix)
	{
		IControllerService controllerService = ServiceLocator.Instance.GetService<IControllerService>();
		IVendItemResult vendItemResult = controllerService.VendItemInPicker(2);
		if (ErrorCodes.PickerEmpty == vendItemResult.Status)
		{
			LogHelper.Instance.WithContext(LogEntryType.Error, "The item {0} was taken.", matrix);
			return false;
		}
		if (ErrorCodes.PickerFull != vendItemResult.Status)
		{
			return false;
		}
		ErrorCodes errorCodes = controllerService.AcceptDiskAtDoor();
		return ErrorCodes.PickerFull == errorCodes;
	}

	internal void Run()
	{
		bool vend = Context.PopTop<bool>();
		int sourceDeck = Context.PopTop<int>();
		int sourceSlot = Context.PopTop<int>();
		int destDeck = Context.PopTop<int>();
		int destSlot = Context.PopTop<int>();
		if (MotionService.MoveTo(sourceDeck, sourceSlot, MoveMode.Get, Context.AppLog) != ErrorCodes.Success)
		{
			Context.CreateInfoResult("MoveError", "Failed to move to initial slot");
			HandleMachineError();
			return;
		}
		IControllerService controllerService = ServiceLocator.Instance.GetService<IControllerService>();
		if (!controllerService.Get().Success)
		{
			Context.CreateInfoResult("GetError", "Failed to pull disc from slot");
			HandleMachineError();
			return;
		}
		string matrix = Scanner.ReadDiskInPicker(ReadDiskOptions.None, Context);
		if (vend && !VendDisk(matrix))
		{
			HandleMachineError();
			return;
		}
		if (MotionService.MoveTo(destDeck, destSlot, MoveMode.Put, Context.AppLog) != ErrorCodes.Success)
		{
			Context.CreateInfoResult("MoveFailed", "Kiosk was unable to move to put position of destination slot");
			HandleMachineError();
			return;
		}
		ControlSystem.TrackCycle();
		if (!controllerService.Put(matrix).Success)
		{
			Context.CreateInfoResult("PutFailed", "Kiosk failed to put disc into slot");
			HandleMachineError();
			return;
		}
		ErrorCodes errorCodes = OnUnitComplete();
		if (errorCodes != ErrorCodes.Success)
		{
			Context.CreateInfoResult("PostUnitFailed", $"Post unit test failed with status {errorCodes}");
			HandleMachineError();
		}
		else
		{
			Context.CreateInfoResult("Success", "SUCCESS");
		}
	}

	internal KioskTestAdapter(ExecutionResult r, ExecutionContext c)
	{
		Context = c;
		Result = r;
		ControlSystem = ServiceLocator.Instance.GetService<IControlSystem>();
		MotionService = ServiceLocator.Instance.GetService<IMotionControlService>();
		AppLog = ApplicationLog.ConfigureLog(Context, logAppends: true, "KioskTest", flushOnWrite: false, "KioskTest");
	}

	private bool VendDisk(string matrix)
	{
		Context.AppLog.Write("Performing a Vend Test.");
		if (MotionService.MoveVend(MoveMode.None, AppLog) != ErrorCodes.Success)
		{
			Context.CreateInfoResult("MoveVendError", "Failed to move to vend position");
			return false;
		}
		ErrorCodes errorCodes = ControlSystem.TrackCycle();
		if (errorCodes != ErrorCodes.Success)
		{
			Context.CreateInfoResult("TrackCycleFailure", $"Track cycle returned error {errorCodes}.");
			return false;
		}
		return OnVendDisk(matrix);
	}

	private void HandleMachineError()
	{
		LogHelper.Instance.WithContext(LogEntryType.Error, "An unrecoverable error occurred in KioskTest.");
		Context.CreateInfoResult("MachineError", "An unrecoverable error occurred in KioskTest");
		AddError("MACHINE ERROR");
	}

	private void AddError(string errMsg)
	{
		Result.Errors.Add(Error.NewError("E999", "Execution context error.", errMsg));
	}
}
