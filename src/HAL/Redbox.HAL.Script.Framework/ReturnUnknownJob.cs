using System;
using System.Threading;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Controller.Framework;

namespace Redbox.HAL.Script.Framework;

[NativeJob(ProgramName = "return-unknown")]
internal class ReturnUnknownJob : NativeJobAdapter, IFileDiskHandler
{
	private string ReceivedSignal = string.Empty;

	private string Previous = "UNKNOWN";

	private bool IsFieldInsert;

	private readonly int SignalWait = 30000;

	private readonly EventWaitHandle WaitEvent = new EventWaitHandle(initialState: false, EventResetMode.ManualReset);

	private static readonly CenterDiskMethod[] m_methods = new CenterDiskMethod[2]
	{
		CenterDiskMethod.None,
		CenterDiskMethod.DrumAndBack
	};

	public FileDiscIterationResult MoveError(ExecutionResult result, string idInPicker, ErrorCodes moveResult, int deck, int slot)
	{
		base.Context.Send("MachineError");
		base.Context.CreateMoveErrorResult(deck, slot);
		return FileDiscIterationResult.Halt;
	}

	public void MachineFull(ExecutionResult result, string id)
	{
		base.Context.Send("MachineFull");
	}

	public FileDiscIterationResult OnFailedPut(ExecutionResult result, IPutResult putResult)
	{
		base.Context.CreateInfoResult("PutFailure", $"There was an error while executing the PUT instruction: {putResult.ToString().ToUpper()}");
		if (putResult.IsSlotInUse)
		{
			base.Context.CreateResult(putResult.ToString().ToUpper(), "The slot is in use.", putResult.PutLocation.Deck, putResult.PutLocation.Slot, putResult.StoredMatrix, null, null);
			return FileDiscIterationResult.Continue;
		}
		return FileDiscIterationResult.Halt;
	}

	public void FailedToFileDisk(ExecutionResult result, string idInPicker)
	{
		base.Context.CreateInfoResult("PutFailureMaxAttempts", "Failed to file disk.", idInPicker);
		base.Context.Send("MaxPutAwayAttemptsExceeded");
		if (base.MotionService.MoveVend(MoveMode.Put, base.Context.AppLog) != ErrorCodes.Success)
		{
			base.Context.Send("MachineError");
			base.Context.CreateInfoResult("MachineError", "The MOVEVEND instruction failed.");
			AddError("Machine error.");
		}
		else
		{
			ServiceLocator.Instance.GetService<IControllerService>().VendItemInPicker();
			AddError("Unable to put away disk.");
		}
	}

	public void DiskFiled(ExecutionResult result, IPutResult putResult)
	{
		base.Context.Send($"ItemPlacementDone:{putResult.StoredMatrix}");
		base.Context.CreateResult("ItemReturned", "The item was successfully returned.", putResult.PutLocation.Deck, putResult.PutLocation.Slot, putResult.StoredMatrix, null, null);
		if (!IsFieldInsert && InventoryConstants.CodeIsUnknown(putResult.StoredMatrix) && "NONE" != Previous)
		{
			if (!DateTime.TryParse(Previous, out var result2))
			{
				base.AppLog.WriteFormatted("Unable to parse date/time information {0}.", Previous);
				AddError("Failed to parse date/time");
			}
			else
			{
				IInventoryService service = ServiceLocator.Instance.GetService<IInventoryService>();
				putResult.PutLocation.ReturnDate = result2;
				service.Save(putResult.PutLocation);
			}
		}
	}

	protected override void ExecuteInner()
	{
		try
		{
			Previous = base.Context.PopTop<string>();
			IsFieldInsert = "FMA-INSERT" == Previous;
			base.Context.RegisterHandler(delegate(string signal)
			{
				try
				{
					ReceivedSignal = signal;
					LogHelper.Instance.WithContext(false, LogEntryType.Info, "Received signal '{0}'", ReceivedSignal);
					WaitEvent.Set();
				}
				catch (Exception e)
				{
					LogHelper.Instance.Log("[return-unknown] Handler received an exception", e);
				}
			});
			ApplicationLog applicationLog = ApplicationLog.ConfigureLog(base.Context, logAppends: true, IsFieldInsert ? "Return" : "Unknowns", flushOnWrite: true, IsFieldInsert ? "FieldInsert" : "ReturnUnknown");
			applicationLog.Write("Return-unknown start.");
			if (base.InventoryService.MachineIsFull())
			{
				base.Context.CreateMachineFullResult();
				base.Context.Send("MachineFull");
				AddError("The machine is full.");
				return;
			}
			IPickerSensorReadResult pickerSensorReadResult = base.ControlSystem.ReadPickerSensors();
			if (!pickerSensorReadResult.Success)
			{
				base.Context.CreateInfoResult("ItemStuckInGripper", "There is a disc stuck in the picker.");
				AddError("Gripper is obstructed.");
			}
			else if (pickerSensorReadResult.IsFull)
			{
				applicationLog.Write("The gripper is obstructed - exiting.");
				base.Context.CreateInfoResult("ItemStuckInGripper", "There is a disc stuck in the picker.");
				AddError("Gripper is obstructed.");
			}
			else
			{
				if (ContextSwitchRequested())
				{
					return;
				}
				ErrorCodes errorCodes = base.MotionService.MoveVend(MoveMode.Get, base.Context.AppLog);
				if (errorCodes != ErrorCodes.Success)
				{
					base.Context.CreateInfoResult("MachineError", $"There was an error while executing the MOVEVEND instruction: {errorCodes.ToString()}");
					base.Context.Send("MachineError");
					AddError("Unable to move to the vend door.");
					return;
				}
				base.Context.Send("BlockingOnSensor");
				switch (base.ControlService.AcceptDiskAtDoor())
				{
				case ErrorCodes.PickerEmpty:
					base.Context.CreateInfoResult("NoReceivedItem", "No disc was received by picker.");
					break;
				case ErrorCodes.PickerObstructed:
					base.ControlService.PushOut();
					base.Context.CreateInfoResult("ItemStuckInGripper", "More than 3 sensors are lit.");
					AddError("Item Stuck in gripper.");
					break;
				case ErrorCodes.VendDoorRentTimeout:
					base.Context.CreateInfoResult("MachineError", "VENDDOOR RENT timed out.");
					AddError("Unable to open the vend door.");
					break;
				case ErrorCodes.VendDoorCloseTimeout:
					base.Context.CreateInfoResult("MachineError", "Venddoor close timeout.");
					AddError("Unable to close the vend door.");
					break;
				default:
					base.Context.Send("MachineError");
					base.Context.CreateInfoResult("MachineError", "Unable to accept disc in picker.");
					AddError("Machine error during accept disc.");
					break;
				case ErrorCodes.PickerFull:
				{
					applicationLog.Write("Item received into picker.");
					base.ControlSystem.TrackCycle();
					using ScanResult scanResult = Scanner.SmartReadDisk(m_methods);
					if (!scanResult.SnapOk)
					{
						base.Context.CreateCameraCaptureErrorResult();
					}
					else if (!scanResult.ReadCode)
					{
						base.Context.CreateNoBarcodeReadResult();
						if (IsFieldInsert)
						{
							switch (base.ControlService.VendItemInPicker().Status)
							{
							case ErrorCodes.PickerEmpty:
								base.Context.CreateInfoResult("BackwardDiscTaken", "The backward disc was taken.");
								return;
							default:
								base.Context.Send("MachineError");
								base.Context.CreateInfoResult("MachineError", "Machine error ejecting the disc.");
								AddError("Hardware error.");
								return;
							case ErrorCodes.PickerFull:
								break;
							}
							ErrorCodes errorCodes2 = base.ControlService.AcceptDiskAtDoor();
							if (ErrorCodes.PickerEmpty == errorCodes2)
							{
								applicationLog.Write("The disk was taken by the user.");
								break;
							}
							if (ErrorCodes.PickerFull != errorCodes2)
							{
								base.Context.CreateInfoResult("ItemStuckInGripper", $"Accept disk at door returned error {errorCodes2.ToString().ToUpper()}");
								AddError("Item stuck in the vend door.");
								break;
							}
						}
					}
					else
					{
						applicationLog.Write($"The ID in the picker: {scanResult.ScannedMatrix}");
						base.Context.CreateInfoResult("BarcodeRead", "The returned item's barcode.", scanResult.ScannedMatrix);
						base.Context.Send($"ReturnEnd:{scanResult.ScannedMatrix}");
						if (!IsFieldInsert)
						{
							ErrorCodes errorCodes3 = ScanAndWait(scanResult);
							if (ErrorCodes.PickerEmpty == errorCodes3 || ErrorCodes.PickerFull != errorCodes3)
							{
								break;
							}
						}
					}
					using FileDiskOperation fileDiskOperation = new FileDiskOperation(this, base.Context);
					if (fileDiskOperation.PutDiscAway(base.Result, scanResult.ScannedMatrix) == ErrorCodes.Success)
					{
						base.MotionService.MoveVend(MoveMode.Get, base.Context.AppLog);
					}
					break;
				}
				}
			}
		}
		finally
		{
			WaitEvent.Close();
		}
	}

	internal ReturnUnknownJob(ExecutionResult result, ExecutionContext ctx)
		: base(result, ctx)
	{
		SignalWait = ControllerConfiguration.Instance.SignalWaitTimeout;
	}

	private ErrorCodes ScanAndWait(ScanResult scanResult)
	{
		using FraudValidator fraudValidator = new FraudValidator(base.Context);
		FraudValidationResult fraudValidationResult = fraudValidator.Validate(scanResult);
		if (FraudValidationResult.Photocopy != fraudValidationResult)
		{
			return ErrorCodes.PickerFull;
		}
		base.Context.Send($"PhotocopyBarcode:{scanResult.ScannedMatrix}");
		if (!WaitEvent.WaitOne(SignalWait) || ReceivedSignal.Equals("continue", StringComparison.CurrentCultureIgnoreCase))
		{
			return ErrorCodes.PickerFull;
		}
		IVendItemResult vendItemResult = base.ControlService.VendItemInPicker();
		switch (vendItemResult.Status)
		{
		case ErrorCodes.PickerEmpty:
			base.Context.CreateInfoResult("BackwardDiscTaken", "The backward disc was taken.");
			return vendItemResult.Status;
		default:
			base.Context.Send("MachineError");
			base.Context.CreateInfoResult("MachineError", "Machine error ejecting the disc.");
			AddError("Hardware error.");
			return vendItemResult.Status;
		case ErrorCodes.PickerFull:
		{
			ErrorCodes errorCodes = base.ControlService.AcceptDiskAtDoor();
			if (ErrorCodes.PickerEmpty == errorCodes)
			{
				base.Context.AppLog.Write("The disk was taken by the user.");
			}
			else if (ErrorCodes.PickerFull != errorCodes)
			{
				base.Context.CreateInfoResult("ItemStuckInGripper", $"Accept disk at door returned error {errorCodes.ToString().ToUpper()}");
				AddError("Item stuck in the vend door.");
			}
			return errorCodes;
		}
		}
	}
}
