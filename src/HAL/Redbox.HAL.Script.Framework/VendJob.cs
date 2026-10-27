using System;
using System.Threading;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Controller.Framework;

namespace Redbox.HAL.Script.Framework;

[NativeJob(ProgramName = "vend")]
internal sealed class VendJob : NativeJobAdapter, IGetObserver
{
	private VendMethod Method;

	private IGetResult CurrentGetResult;

	private string ReceivedSignal;

	private readonly EventWaitHandle SignalEvent = new EventWaitHandle(initialState: false, EventResetMode.ManualReset);

	public void OnStuck(IGetResult result)
	{
		ServiceLocator.Instance.GetService<IInventoryService>().UpdateEmptyStuck(result.Location);
	}

	public bool OnEmpty(IGetResult result)
	{
		IInventoryService service = ServiceLocator.Instance.GetService<IInventoryService>();
		if (ControllerConfiguration.Instance.PeekDuringVend)
		{
			using (PeekOperation peekOperation = new PeekOperation())
			{
				IPeekResult peekResult = peekOperation.Execute();
				if (!peekResult.TestOk || peekResult.IsFull)
				{
					result.Update(ErrorCodes.ItemStuck);
					service.UpdateEmptyStuck(result.Location);
					return false;
				}
				return true;
			}
		}
		result.Update(ErrorCodes.ItemStuck);
		service.UpdateEmptyStuck(result.Location);
		return false;
	}

	protected override void ExecuteInner()
	{
		try
		{
			ILocation location = null;
			string method = base.Context.PopTop<string>();
			Method = ConvertToMethod(method);
			if (VendMethod.Preposition == Method)
			{
				RegisterForCorrectionEvents();
			}
			string overrideName = ((Method == VendMethod.Location) ? "field-remove" : "vend");
			ApplicationLog log = ApplicationLog.ConfigureLog(base.Context, logAppends: true, "vend", flushOnWrite: false, overrideName);
			base.Context.RegisterHandler(delegate(string signal)
			{
				ReceivedSignal = signal;
				SignalEvent.Set();
			});
			LogHelper.Instance.WithContext("Vend start, Method = {0}", Method);
			base.Context.Send("VendStart");
			int num = base.Context.PopTop<int>();
			int num2 = num;
			if (VendMethod.Location == Method)
			{
				using CheckPickerOperation checkPickerOperation = new CheckPickerOperation(base.Context);
				if (checkPickerOperation.CheckPicker(base.Result, null) != ErrorCodes.PickerEmpty)
				{
					AddError("Failed to put disc away.");
					return;
				}
			}
			IInventoryService service = ServiceLocator.Instance.GetService<IInventoryService>();
			string text = null;
			while (num > 0)
			{
				if (num2 != num)
				{
					base.Context.Send("VendGraphicDisplay");
					if (ContextSwitchRequested())
					{
						break;
					}
				}
				if (VendMethod.Location == Method)
				{
					int deck = base.Context.PopTop<int>();
					int slot = base.Context.PopTop<int>();
					location = service.Get(deck, slot);
					text = location.ID;
				}
				else
				{
					text = base.Context.PopTop<string>();
					using (CheckPickerOperation checkPickerOperation2 = new CheckPickerOperation(base.Context))
					{
						ErrorCodes errorCodes = checkPickerOperation2.CheckPicker(base.Result, text);
						if (ErrorCodes.PickerEmpty != errorCodes)
						{
							LogHelper.Instance.WithContext("Check picker returned an error status {0}", errorCodes.ToString());
							WaitForSignal();
							AddError("Item stuck in the gripper.");
							break;
						}
					}
					if (Method == VendMethod.ID)
					{
						base.Context.Send("FindItemToVend");
					}
					location = service.Lookup(text);
					if (location == null)
					{
						base.Context.Send("ItemNotFound");
						base.Context.CreateLookupErrorResult("VendError", text);
						if (--num <= 0)
						{
							WaitForSignal();
							break;
						}
						continue;
					}
				}
				LogHelper.Instance.WithContext(LogEntryType.Info, "Trying to vend ID: {0} at {1}", text, location);
				TransactionResult transactionResult = MoveAndGet(location, text);
				if (TransactionResult.MachineError == transactionResult)
				{
					WaitForSignal();
					OnMachineError(text);
					break;
				}
				if (TransactionResult.ContinueToNext == transactionResult)
				{
					if (--num <= 0)
					{
						WaitForSignal();
						break;
					}
					continue;
				}
				if (ControllerConfiguration.Instance.PrepositionVendFraudMove && VendMethod.Preposition == Method)
				{
					ServiceLocator.Instance.GetService<IMotionControlService>().MoveTo(6, location.Slot, MoveMode.None, base.AppLog);
					if (!OnPrePosition(location, text))
					{
						break;
					}
				}
				ErrorCodes errorCodes2 = base.MotionService.MoveVend(MoveMode.None, log);
				if (errorCodes2 != ErrorCodes.Success)
				{
					base.Context.Send("MachineError");
					WaitForSignal();
					base.Context.OnMoveVendError(errorCodes2, text);
					break;
				}
				base.ControlSystem.TrackCycle();
				if (Method == VendMethod.Preposition && !OnPrePosition(location, text))
				{
					break;
				}
				base.Context.Send("PickupGraphicDisplay");
				IVendItemResult vendItemResult = ServiceLocator.Instance.GetService<IControllerService>().VendItemInPicker();
				if (ErrorCodes.PickerEmpty == vendItemResult.Status)
				{
					base.Context.Send("ItemVended");
					LogHelper.Instance.WithContext("The item {0} was taken.", text);
					base.Context.CreateResult("ItemVended", "The item was successfully vended.", location.Deck, location.Slot, text, null, null);
					num--;
					continue;
				}
				if (ErrorCodes.PickerFull != vendItemResult.Status)
				{
					OnMachineError(text);
					break;
				}
				if (!vendItemResult.Presented)
				{
					LogHelper.Instance.WithContext(LogEntryType.Error, "The disk was not presented - home the Y axis.");
					base.MotionService.HomeAxis(Axis.Y);
					base.ControlSystem.RollerToPosition(RollerPosition.Position6, 3000);
				}
				OnFraudAttempt(location, text);
				break;
			}
		}
		finally
		{
			if (ControllerConfiguration.Instance.EmptyStuckAlertThreshold > 0)
			{
				IPersistentCounterService service2 = ServiceLocator.Instance.GetService<IPersistentCounterService>();
				IPersistentCounter persistentCounter = service2.Find("EmptyOrStuckCount");
				if (persistentCounter != null && persistentCounter.Value > ControllerConfiguration.Instance.EmptyStuckAlertThreshold)
				{
					LogHelper.Instance.Log(LogEntryType.Error, "The Empty/Stuck counter for alert generation has exceeded its threshold (value = {0})", persistentCounter.Value);
					if (ControllerConfiguration.Instance.AlertForEmptyStuck)
					{
						base.Context.CreateInfoResult("EmptyStuckThresholdExceeded", "There are too many empty/stuck failures transactions on this kiosk, please investigate.");
						service2.Reset(persistentCounter);
					}
				}
			}
			try
			{
				SignalEvent.Close();
			}
			catch
			{
			}
			base.MotionService.MoveVend(MoveMode.Get, base.Context.AppLog);
		}
	}

	internal VendJob(ExecutionResult result, ExecutionContext ctx)
		: base(result, ctx)
	{
	}

	private bool OnPrePosition(ILocation bcLocation, string idToFind)
	{
		if (!WaitForSignal())
		{
			FileDisk(bcLocation, idToFind);
			using (CheckPickerOperation checkPickerOperation = new CheckPickerOperation(base.Context))
			{
				if (checkPickerOperation.CheckPicker(base.Result, idToFind) != ErrorCodes.PickerEmpty)
				{
					AddError("Failed to put disc away.");
					return false;
				}
			}
			AddError("The vend job failed with errored SIGNAL status.");
			return false;
		}
		base.Context.CreateResult("PrepositionedItem", "Preposition picker has item ready at vend door.", bcLocation.Deck, bcLocation.Slot, null, null, null);
		Method = VendMethod.ID;
		return true;
	}

	private ErrorCodes OnFraudAttempt(ILocation source, string id)
	{
		if (ServiceLocator.Instance.GetService<IControllerService>().AcceptDiskAtDoor() != ErrorCodes.PickerFull)
		{
			LogHelper.Instance.WithContext("Unable to move disc back to sensor 3; assume the disc is vended.");
			return FileFraudedDisk(source, id);
		}
		base.ControlSystem.TrackCycle();
		ScanResult scanResult = null;
		using (Scanner scanner = new Scanner(base.ControlSystem))
		{
			scanResult = scanner.Read();
		}
		using (scanResult)
		{
			if (!scanResult.SnapOk)
			{
				base.Context.CreateCameraCaptureErrorResult();
				return FileFraudedDisk(source, id);
			}
			if (!scanResult.ReadCode)
			{
				LogHelper.Instance.WithContext("The barcode in the picker could not be read; scanned ID = {0}; item assumed vended.", scanResult.ScannedMatrix);
			}
			if (scanResult.ScannedMatrix == id)
			{
				LogHelper.Instance.WithContext("Item ID = {0} was not taken; the barcode in the picker matched the item requested to vend.", id);
				base.Context.CreateResult("UnclaimedVend", "The vend request failed because the item was not taken.  It has been returned.", source.Deck, source.Slot, id, null, null);
				FileDisk(source, scanResult.ScannedMatrix);
				IPickerSensorReadResult pickerSensorReadResult = base.ControlSystem.ReadPickerSensors();
				if (!pickerSensorReadResult.Success)
				{
					return ErrorCodes.SensorReadError;
				}
				return pickerSensorReadResult.IsFull ? ErrorCodes.PickerFull : ErrorCodes.PickerEmpty;
			}
			base.Context.AppLog.Write($"Item ID = {id} was not taken; it did not match the barcode of the item in the picker ID = {scanResult.ScannedMatrix}.");
			LogHelper.Instance.WithContext("The barcode in the picker could not be matched to the requested item; returned ID = {0}; item assumed vended.", scanResult.ScannedMatrix);
		}
		base.ControlSystem.LogInputs(ControlBoards.Picker);
		base.ControlSystem.LogInputs(ControlBoards.Aux);
		return FileFraudedDisk(source, id);
	}

	private ErrorCodes FileFraudedDisk(ILocation source, string id)
	{
		_ = $"Item ID = {id} was vended.";
		base.Context.CreateResult("ItemVended", "The item was successfully vended.", source.Deck, source.Slot, id, null, null);
		FileDisk(source, id);
		IPickerSensorReadResult pickerSensorReadResult = base.ControlSystem.ReadPickerSensors();
		if (!pickerSensorReadResult.Success)
		{
			return ErrorCodes.SensorReadError;
		}
		if (!pickerSensorReadResult.IsFull)
		{
			return ErrorCodes.PickerEmpty;
		}
		return ErrorCodes.PickerFull;
	}

	private bool FileDisk(ILocation location, string id)
	{
		if (ServiceLocator.Instance.GetService<IMotionControlService>().MoveTo(location, MoveMode.Put, base.Context.AppLog) != ErrorCodes.Success)
		{
			return false;
		}
		if (!ServiceLocator.Instance.GetService<IControllerService>().Put(id).Success)
		{
			return false;
		}
		if (CurrentGetResult != null)
		{
			IInventoryService service = ServiceLocator.Instance.GetService<IInventoryService>();
			location.Flags = CurrentGetResult.Flags;
			location.ReturnDate = CurrentGetResult.ReturnTime;
			service.Save(location);
		}
		return true;
	}

	private VendMethod ConvertToMethod(string method)
	{
		if ("PREPOSITION-PICKER" == method)
		{
			return VendMethod.Preposition;
		}
		if ("BY-ID" == method)
		{
			return VendMethod.ID;
		}
		if ("BY-LOCATION" == method)
		{
			return VendMethod.Location;
		}
		return VendMethod.None;
	}

	private TransactionResult MoveAndGet(ILocation loc, string barcode)
	{
		CurrentGetResult = null;
		LogHelper.Instance.WithContext("Move {0} to get requested barcode = {1}.", loc, barcode);
		switch (ServiceLocator.Instance.GetService<IMotionControlService>().MoveTo(loc, MoveMode.Get, base.Context.AppLog))
		{
		case ErrorCodes.Timeout:
			base.Context.CreateInfoResult("MoveFailure", "The MOVE instruction returned TIMEOUT.", barcode);
			return TransactionResult.ContinueToNext;
		default:
			base.Context.Send("MachineError");
			return TransactionResult.MachineError;
		case ErrorCodes.Success:
		{
			IControllerService service = ServiceLocator.Instance.GetService<IControllerService>();
			CurrentGetResult = service.Get(this);
			if (CurrentGetResult.Success)
			{
				return TransactionResult.Success;
			}
			if (CurrentGetResult.IsSlotEmpty)
			{
				base.Context.Send("ItemNotFound");
				base.Context.CreateResult("VendError", "Get returned empty slot.", loc.Deck, loc.Slot, CurrentGetResult.Previous, null, barcode);
				return TransactionResult.ContinueToNext;
			}
			if (CurrentGetResult.ItemStuck && ControllerConfiguration.Instance.EmptyStuckAlertThreshold > 0 && Method != VendMethod.Location)
			{
				ServiceLocator.Instance.GetService<IPersistentCounterService>().Increment("EmptyOrStuckCount");
			}
			service.ClearGripper();
			IPickerSensorReadResult pickerSensorReadResult = base.ControlSystem.ReadPickerSensors();
			if (!pickerSensorReadResult.Success)
			{
				base.Context.Send("MachineError");
				return TransactionResult.MachineError;
			}
			if (!pickerSensorReadResult.IsFull)
			{
				if (ErrorCodes.GripperExtendTimeout == CurrentGetResult.HardwareError)
				{
					base.Context.AppLog.WriteFormatted("Gripper extend failed at location {0}; updating empty/stuck counter.", loc.ToString());
					base.InventoryService.UpdateEmptyStuck(CurrentGetResult.Location);
				}
				base.Context.Send("GripperTimedOut");
				base.Context.CreateResult("VendError", "Gripper timed out.", loc.Deck, loc.Slot, CurrentGetResult.Previous, null, barcode);
				return TransactionResult.ContinueToNext;
			}
			base.Context.Send("MachineError");
			return TransactionResult.MachineError;
		}
		}
	}

	private void OnMachineError(string barcode)
	{
		string text = "An unrecoverable error occurred in vend.";
		LogHelper.Instance.WithContext(LogEntryType.Error, text);
		base.Context.CreateInfoResult("MachineError", text, barcode);
		AddError("MACHINE ERROR");
	}

	private bool WaitForSignal()
	{
		if (VendMethod.Preposition != Method)
		{
			return true;
		}
		int num = 300000;
		if (!SignalEvent.WaitOne(num))
		{
			LogHelper.Instance.WithContext("Didn't receive a signal in the specified time ( {0}ms )", num);
			base.Context.PushTop("TIMEOUT");
			return false;
		}
		if (string.IsNullOrEmpty(ReceivedSignal))
		{
			ReceivedSignal = "NONE";
		}
		bool flag = ReceivedSignal.Equals("continue", StringComparison.CurrentCultureIgnoreCase);
		LogHelper.Instance.WithContext("Received a signal '{0}' ( {1} ) within timeout.", ReceivedSignal, flag ? "expected" : "not expected");
		return flag;
	}
}
