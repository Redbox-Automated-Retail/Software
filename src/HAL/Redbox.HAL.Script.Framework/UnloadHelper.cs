using Redbox.HAL.Component.Model;
using Redbox.HAL.Controller.Framework;

namespace Redbox.HAL.Script.Framework;

internal struct UnloadHelper : IFileDiskHandler
{
	private readonly ExecutionContext Context;

	private readonly ExecutionResult Result;

	public void MachineFull(ExecutionResult result, string idInPicker)
	{
		Context.CreateInfoResult("QlmUnloadMachineFull", "The FINDEMPTY request failed because the machine is full.", idInPicker);
	}

	public FileDiscIterationResult MoveError(ExecutionResult result, string idInPicker, ErrorCodes moveResult, int deck, int slot)
	{
		if (moveResult == ErrorCodes.Success)
		{
			return FileDiscIterationResult.Continue;
		}
		if (ErrorCodes.Timeout == moveResult)
		{
			return FileDiscIterationResult.NextLocation;
		}
		Context.CreateResult("MachineError", "There was an error executing the MOVE instruction.", deck, slot, idInPicker, null, null);
		return FileDiscIterationResult.Halt;
	}

	public void DiskFiled(ExecutionResult result, IPutResult putResult)
	{
		Context.CreateResult("QlmUnloadPutIntoDrum", "An item in the picker was put away.", putResult.PutLocation.Deck, putResult.PutLocation.Slot, putResult.StoredMatrix, null, null);
	}

	public FileDiscIterationResult OnFailedPut(ExecutionResult result, IPutResult putResult)
	{
		if (putResult.IsSlotInUse)
		{
			Context.CreateResult(putResult.ToString().ToUpper(), "The slot is in use.", putResult.PutLocation.Deck, putResult.PutLocation.Slot, putResult.StoredMatrix, null, null);
			return FileDiscIterationResult.Continue;
		}
		Context.CreateInfoResult("MachineError", "The PUT request failed; try to put the product away into an empty slot.", putResult.StoredMatrix);
		return FileDiscIterationResult.Continue;
	}

	public void FailedToFileDisk(ExecutionResult result, string idInPicker)
	{
		Context.CreateInfoResult("MachineError", "The PUT request failed; unable to put the disk away.", idInPicker);
	}

	internal void CheckAndClearPicker(ExecutionResult result)
	{
		IControlSystem service = ServiceLocator.Instance.GetService<IControlSystem>();
		IPickerSensorReadResult pickerSensorReadResult = service.ReadPickerSensors();
		if (!pickerSensorReadResult.Success)
		{
			result.Errors.Add(Error.NewError("E001", "Item stuck", "There is an item stuck in the gripper; can't continue."));
		}
		else
		{
			if (!pickerSensorReadResult.IsFull)
			{
				return;
			}
			string id = "UNKNOWN";
			if (ServiceLocator.Instance.GetService<IControllerService>().ClearGripper() != ErrorCodes.Success)
			{
				Context.CreateItemStuckResult(id);
				result.Errors.Add(Error.NewError("E001", "Item stuck", "There is an item stuck in the gripper; can't continue."));
				return;
			}
			pickerSensorReadResult = service.ReadPickerSensors();
			if (!pickerSensorReadResult.Success)
			{
				result.Errors.Add(Error.NewError("E001", "Item stuck", "There is an item stuck in the gripper; can't continue."));
				return;
			}
			if (pickerSensorReadResult.IsFull)
			{
				id = Scanner.ReadDiskInPicker(ReadDiskOptions.None, Context);
				using FileDiskOperation fileDiskOperation = new FileDiskOperation(Context);
				if (fileDiskOperation.PutDiscAway(result, id) != ErrorCodes.Success)
				{
					Context.CreateItemStuckResult(id);
					result.Errors.Add(Error.NewError("E001", "Item stuck", "There is an item stuck in the gripper; can't continue."));
				}
				return;
			}
			LogHelper.Instance.WithContext("Item was cleared from picker.");
		}
	}

	internal MerchandizeResult UnloadDisk(int srcDeck, int srcSlot)
	{
		bool isVMZMachine = ControllerConfiguration.Instance.IsVMZMachine;
		ServiceLocator.Instance.GetService<IPersistentCounterService>();
		IMotionControlService service = ServiceLocator.Instance.GetService<IMotionControlService>();
		IControllerService service2 = ServiceLocator.Instance.GetService<IControllerService>();
		ErrorCodes errorCodes = ErrorCodes.Success;
		errorCodes = service.MoveTo(srcDeck, srcSlot, MoveMode.Get, Context.AppLog);
		if (errorCodes != ErrorCodes.Success)
		{
			MerchandizingHelper.Instance.CleanupJob(Result, Context, errorCodes);
			Context.CreateResult("MachineError", "There was an error executing the MOVE instruction.", srcDeck, srcSlot, null, null, null);
			MerchandizingHelper.Instance.AddError(Result, "The MOVE instruction failed.");
			return MerchandizeResult.HardwareError;
		}
		IGetResult getResult = service2.Get();
		if (getResult.IsSlotEmpty)
		{
			Context.CreateResult("QlmUnloadQlmSlotEmpty", "There was no item found at this location.", srcDeck, srcSlot, null, null, null);
			return MerchandizeResult.EmptyStuck;
		}
		if (!getResult.Success)
		{
			if (isVMZMachine && getResult.ItemStuck)
			{
				IInventoryService service3 = ServiceLocator.Instance.GetService<IInventoryService>();
				ILocation location = service3.Get(srcDeck, srcSlot);
				location.Flags = MerchFlags.None;
				service3.Save(location);
				Context.CreateResult("UnloadEmptyStuck", "The disk at this location is stuck.", srcDeck, srcSlot, null, null, null);
				return MerchandizeResult.EmptyStuck;
			}
			Context.CreateResult("MachineError", string.Format("A hardware error occurred in the get from {0}.", isVMZMachine ? "VMZ" : "QLM"), srcDeck, srcSlot, null, null, null);
			MerchandizingHelper.Instance.AddError(Result, string.Format("An error occurred getting an item from the {0}.", isVMZMachine ? "VMZ" : "QLM"));
			if (service2.ClearGripper() == ErrorCodes.Success)
			{
				MerchandizingHelper.Instance.CleanupJob(Result, Context, ErrorCodes.Success);
			}
			return MerchandizeResult.HardwareError;
		}
		string text = "UNKNOWN";
		using (ScanResult scanResult = Scanner.SmartReadDisk())
		{
			text = scanResult.ScannedMatrix;
			if (!scanResult.SnapOk)
			{
				if (ControllerConfiguration.Instance.IsVMZMachine)
				{
					Result.Errors.Add(Error.NewError("E997", "CAMERA CAPTURE error.", "The camera isn't working."));
				}
			}
			else if (!scanResult.ReadCode)
			{
				Context.CreateResult("QlmUnloadBadRead", "The READ request failed because no barcode could be read.", srcDeck, srcSlot, null, null, null);
			}
		}
		Context.CreateResult("QlmUnloadBarcodeFromQlm", string.Format("The item pulled from the {0} has the following barcode.", isVMZMachine ? "VMZ" : "QLM"), srcDeck, srcSlot, text, null, null);
		if (ControllerConfiguration.Instance.IsVMZMachine)
		{
			errorCodes = service.MoveTo(srcDeck, srcSlot, MoveMode.Put, Context.AppLog);
			if (errorCodes != ErrorCodes.Success)
			{
				MerchandizingHelper.Instance.CleanupJob(Result, Context, errorCodes);
				MerchandizingHelper.Instance.ExitOnMoveError(Result, Context);
				return MerchandizeResult.HardwareError;
			}
			IPutResult putResult = service2.Put(text);
			if (putResult.Success)
			{
				DiskFiled(Result, putResult);
				return MerchandizeResult.Success;
			}
		}
		return FileUnload(text, srcDeck, srcSlot);
	}

	internal UnloadHelper(ExecutionContext context, ExecutionResult result)
	{
		Context = context;
		Result = result;
	}

	private MerchandizeResult FileUnload(string readMatrix, int srcDeck, int srcSlot)
	{
		using FileDiskOperation fileDiskOperation = new FileDiskOperation(this, Context);
		ErrorCodes errorCodes = fileDiskOperation.PutDiscAway(Result, readMatrix);
		if (errorCodes == ErrorCodes.Success)
		{
			return MerchandizeResult.Success;
		}
		ErrorCodes errorCodes2 = ServiceLocator.Instance.GetService<IMotionControlService>().MoveTo(srcDeck, srcSlot, MoveMode.Put, Context.AppLog);
		if (errorCodes2 != ErrorCodes.Success)
		{
			MerchandizingHelper.Instance.CleanupJob(Result, Context, errorCodes2);
			MerchandizingHelper.Instance.ExitOnMoveError(Result, Context);
			return MerchandizeResult.HardwareError;
		}
		ServiceLocator.Instance.GetService<IControlSystem>().TrackCycle();
		if (!ServiceLocator.Instance.GetService<IControllerService>().Put(readMatrix).Success)
		{
			Context.CreateResult("MachineError", "There was an error executing the PUT instruction.", srcDeck, srcSlot, readMatrix, null, null);
		}
		MerchandizingHelper.Instance.CleanupJob(Result, Context, ErrorCodes.Success);
		MerchandizingHelper.Instance.AddError(Result, "Unable to put disk away.");
		return (errorCodes == ErrorCodes.MachineFull) ? MerchandizeResult.MachineFull : MerchandizeResult.HardwareError;
	}
}
