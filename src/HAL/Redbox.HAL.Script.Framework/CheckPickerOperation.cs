using System;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Script.Framework;

internal struct CheckPickerOperation : IFileDiskHandler, IDisposable
{
	private readonly ExecutionContext Context;

	public void Dispose()
	{
	}

	public void MachineFull(ExecutionResult result, string idInPicker)
	{
		Context.Send("MachineFull");
		Context.CreateMachineFullResult(idInPicker);
	}

	public FileDiscIterationResult MoveError(ExecutionResult result, string idInPicker, ErrorCodes moveResult, int deck, int slot)
	{
		if (ErrorCodes.Timeout == moveResult)
		{
			return FileDiscIterationResult.NextLocation;
		}
		Context.CreateResult("MoveFailure", $"The MOVE instruction failed: {moveResult.ToString().ToUpper()}", deck, slot, idInPicker, null, null);
		return FileDiscIterationResult.Halt;
	}

	public void DiskFiled(ExecutionResult result, IPutResult putResult)
	{
		Context.CreateResult("ItemReturned", "check-picker cleared an item from the picker.", putResult.PutLocation.Deck, putResult.PutLocation.Slot, putResult.StoredMatrix, null, null);
		string value = string.Format("[check-picker]Item in picker filed to {0}: Barcode: {1} Is duplicate: {2}", putResult.PutLocation.ToString(), putResult.StoredMatrix, putResult.IsDuplicate ? "YES" : "NO");
		Context.SetSymbolValue("MSTESTER-SYMBOL-CP-FILED-DISC", value);
	}

	public FileDiscIterationResult OnFailedPut(ExecutionResult result, IPutResult putResult)
	{
		string message = $"PUT {putResult.PutLocation.ToString()} ID={putResult.StoredMatrix} returned status {putResult.ToString().ToUpper()}";
		Context.CreateResult("PutRequiresRetry", message, putResult.PutLocation.Deck, putResult.PutLocation.Slot, putResult.StoredMatrix, null, null);
		if (putResult.IsSlotInUse)
		{
			Context.CreateResult(putResult.ToString().ToUpper(), "The slot is in use.", putResult.PutLocation.Deck, putResult.PutLocation.Slot, putResult.StoredMatrix, null, null);
		}
		return FileDiscIterationResult.Continue;
	}

	public void FailedToFileDisk(ExecutionResult result, string idInPicker)
	{
		Context.Send("MaxPutAwayAttemptsExceeded");
		Context.CreateInfoResult("PutFailureMaxAttempts", "PutDiscAway was unable to file a disc in the picker away.", idInPicker);
	}

	internal ErrorCodes CheckPicker(ExecutionResult result, string vendId)
	{
		Context.Send("CheckPickerStart");
		LogHelper.Instance.WithContext("Check picker to see if we need to put something away.");
		IControlSystem service = ServiceLocator.Instance.GetService<IControlSystem>();
		IPickerSensorReadResult pickerSensorReadResult = service.ReadPickerSensors();
		if (!pickerSensorReadResult.Success)
		{
			Context.CreateItemStuckResult(vendId);
			Context.SetSymbolValue("MSTESTER-SYMBOL-PICKER-FULL", "Sensor read failed.");
			return ErrorCodes.PickerObstructed;
		}
		if (!pickerSensorReadResult.IsFull)
		{
			OnComplete();
			return ErrorCodes.PickerEmpty;
		}
		if (ServiceLocator.Instance.GetService<IControllerService>().ClearGripper() != ErrorCodes.Success)
		{
			Context.CreateItemStuckResult(vendId);
			Context.SetSymbolValue("MSTESTER-SYMBOL-PICKER-FULL", "The gripper is obstructed.");
			return ErrorCodes.PickerObstructed;
		}
		pickerSensorReadResult = service.ReadPickerSensors();
		if (!pickerSensorReadResult.Success)
		{
			Context.CreateItemStuckResult(vendId);
			Context.SetSymbolValue("MSTESTER-SYMBOL-PICKER-FULL", "Sensor read failed.");
			return ErrorCodes.PickerObstructed;
		}
		if (!pickerSensorReadResult.IsFull)
		{
			OnComplete();
			return ErrorCodes.PickerEmpty;
		}
		Context.Send("ItemInPicker");
		LogHelper.Instance.WithContext("Picker is full, trying to find a slot for product.");
		if (service.VendDoorState != VendDoorState.Closed)
		{
			service.VendDoorClose();
		}
		string text = Scanner.ReadDiskInPicker(ReadDiskOptions.LeaveCaptureResult | ReadDiskOptions.LeaveNoReadResult, Context);
		LogHelper.Instance.WithContext("The ID in the picker: {0}", text);
		using FileDiskOperation fileDiskOperation = new FileDiskOperation(this, Context);
		ErrorCodes errorCodes = fileDiskOperation.PutDiscAway(result, text);
		if (errorCodes == ErrorCodes.Success)
		{
			OnComplete();
		}
		return (errorCodes == ErrorCodes.Success) ? ErrorCodes.PickerEmpty : ErrorCodes.PickerFull;
	}

	internal CheckPickerOperation(ExecutionContext context)
	{
		this = default(CheckPickerOperation);
		Context = context;
	}

	private void OnComplete()
	{
		Context.Send("CheckPickerStop");
		LogHelper.Instance.WithContext("check-picker complete.");
	}
}
