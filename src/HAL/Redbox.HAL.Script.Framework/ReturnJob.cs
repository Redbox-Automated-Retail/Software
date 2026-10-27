using System;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Controller.Framework;

namespace Redbox.HAL.Script.Framework;

internal sealed class ReturnJob : NativeJobAdapter, IFileDiskHandler
{
	private bool CameraErrored;

	private static int m_cameraErrors = 0;

	private static readonly CenterDiskMethod[] CenterMethods = new CenterDiskMethod[2]
	{
		CenterDiskMethod.None,
		CenterDiskMethod.DrumAndBack
	};

	internal static int CameraErrorCount
	{
		get
		{
			if (!ControllerConfiguration.Instance.FixCameraAfterReturn)
			{
				return 0;
			}
			return m_cameraErrors;
		}
	}

	public void MachineFull(ExecutionResult result, string id)
	{
		base.Context.Send("MachineFull");
	}

	public FileDiscIterationResult MoveError(ExecutionResult result, string idInPicker, ErrorCodes moveResult, int deck, int slot)
	{
		if (ErrorCodes.Timeout == moveResult)
		{
			return FileDiscIterationResult.NextLocation;
		}
		base.Context.Send("MachineError");
		base.Context.CreateResult("MachineError", "The MOVE instruction failed.", deck, slot, idInPicker, null, null);
		return FileDiscIterationResult.Halt;
	}

	public void DiskFiled(ExecutionResult result, IPutResult putResult)
	{
		base.Context.Send($"ItemPlacementDone:{putResult.StoredMatrix}");
		base.Context.CreateResult("ItemReturned", "The item was successfully returned.", putResult.PutLocation.Deck, putResult.PutLocation.Slot, putResult.StoredMatrix, null, null);
		if (InventoryConstants.CodeIsUnknown(putResult.StoredMatrix))
		{
			IInventoryService service = ServiceLocator.Instance.GetService<IInventoryService>();
			putResult.PutLocation.ReturnDate = DateTime.Now;
			service.Save(putResult.PutLocation);
		}
	}

	public FileDiscIterationResult OnFailedPut(ExecutionResult result, IPutResult putResult)
	{
		if (putResult.IsSlotInUse)
		{
			base.Context.CreateResult(putResult.ToString().ToUpper(), "The slot is in use.", putResult.PutLocation.Deck, putResult.PutLocation.Slot, putResult.StoredMatrix, null, null);
			return FileDiscIterationResult.Continue;
		}
		base.Context.Send("MachineError");
		base.Context.CreateResult(putResult.ToString().ToUpper(), "The PUT instruction failed.", putResult.PutLocation.Deck, putResult.PutLocation.Slot, putResult.StoredMatrix, null, null);
		return FileDiscIterationResult.Halt;
	}

	public void FailedToFileDisk(ExecutionResult result, string idInPicker)
	{
		base.Context.Send("MaxPutAwayAttemptsExceeded");
	}

	internal static void ResetCameraErrorCounter()
	{
		m_cameraErrors = 0;
	}

	internal ReturnJob(ExecutionResult result, ExecutionContext context)
		: base(result, context)
	{
	}

	protected override void ExecuteInner()
	{
		ServiceLocator.Instance.GetService<IRuntimeService>();
		string scannedId = "UNKNOWN";
		ApplicationLog applicationLog = ApplicationLog.ConfigureLog(base.Context, logAppends: true, "Return", flushOnWrite: false, null);
		try
		{
			LogHelper.Instance.WithContext("Return start.");
			IPickerSensorReadResult pickerSensorReadResult = base.ControlSystem.ReadPickerSensors();
			if (!pickerSensorReadResult.Success)
			{
				base.Context.CreateInfoResult("MachineError", "Unable to read picker sensors.");
				AddError("Unable to read picker sensors.");
				return;
			}
			if (pickerSensorReadResult.IsFull)
			{
				using CheckPickerOperation checkPickerOperation = new CheckPickerOperation(base.Context);
				if (checkPickerOperation.CheckPicker(base.Result, null) != ErrorCodes.PickerEmpty)
				{
					AddError("Failed to put disk away.");
					return;
				}
			}
			if (base.InventoryService.MachineIsFull())
			{
				base.Context.Send("MachineFull");
			}
			else if (base.MotionService.MoveVend(MoveMode.Get, applicationLog) != ErrorCodes.Success)
			{
				OnMoveVendError();
			}
			else
			{
				if (ContextSwitchRequested())
				{
					return;
				}
				base.Context.Send("DisableCancel");
				base.Context.Send("BlockingOnSensor");
				ErrorCodes errorCodes = ServiceLocator.Instance.GetService<IControllerService>().AcceptDiskAtDoor();
				if (ErrorCodes.SensorReadError == errorCodes)
				{
					base.Context.Send("MachineError");
					AddError("Machine error during accept disc.");
					return;
				}
				if (ErrorCodes.PickerEmpty == errorCodes)
				{
					applicationLog.Write("An item was not put in the picker.");
					return;
				}
				if (ErrorCodes.VendDoorRentTimeout == errorCodes)
				{
					base.Context.Send("MachineError");
					base.Context.CreateInfoResult("MachineError", "Unable to set the vend door to RENT.");
					return;
				}
				if (ErrorCodes.PickerObstructed == errorCodes || ErrorCodes.VendDoorCloseTimeout == errorCodes)
				{
					ErrorCodes errorCodes2 = RejectDisk();
					if (ErrorCodes.PickerEmpty == errorCodes2 || ErrorCodes.PickerFull != errorCodes2)
					{
						return;
					}
				}
				LogHelper.Instance.WithContext("Item received into picker.");
				base.ControlSystem.TrackCycle();
				if (!ReadDisk(out scannedId))
				{
					base.Context.Send("ItemInsertError");
					ReturnToUserOrFile(FileDiskInKiosk, delegate
					{
					}, scannedId);
				}
				else if (ServiceLocator.Instance.GetService<IBarcodeValidatorService>().IsValid(scannedId))
				{
					if (ControllerConfiguration.Instance.MarkDuplicatesUnknown && ControllerConfiguration.Instance.LeaveDuplicateResultInReturn && base.InventoryService.IsBarcodeDuplicate(scannedId, out var _))
					{
						base.Context.CreateInfoResult("CameraCaptureFailure", "Return has detected a duplicate barcode.");
					}
					FileDiskInKiosk(scannedId);
				}
				else
				{
					base.Context.Send("UnrecognizedBarcodeReturned");
					base.Context.AppLog.WriteFormatted($"The barcode {scannedId} is not a recognized Redbox barcode.");
					ReturnToUserOrFile(FileUnrecognizedDisk, delegate(string barcode)
					{
						base.Context.CreateInfoResult("NonRedboxDiskReturnedToUser", "The disk was returned to the user.", barcode);
					}, scannedId);
				}
				IPickerSensorReadResult pickerSensorReadResult2 = base.ControlSystem.ReadPickerSensors();
				if (pickerSensorReadResult2.Success && !pickerSensorReadResult2.IsFull)
				{
					if (base.MotionService.MoveVend(MoveMode.Get, base.Context.AppLog) != ErrorCodes.Success)
					{
						AddError("Unable to move to the vend position.");
					}
					else
					{
						base.Context.Send("Bail");
					}
				}
			}
		}
		finally
		{
			if (ControllerConfiguration.Instance.IsVMZMachine)
			{
				IInventoryService service = ServiceLocator.Instance.GetService<IInventoryService>();
				base.Context.CreateInfoResult("EmptySlotCount", service.GetMachineEmptyCount().ToString());
				IDumpbinService service2 = ServiceLocator.Instance.GetService<IDumpbinService>();
				base.Context.CreateInfoResult("DumpBinRemainingCapacity", service2.RemainingSpace().ToString());
				base.Context.CreateInfoResult("ReturnSlotBufferValue", ControllerConfiguration.Instance.ReturnSlotBuffer.ToString());
			}
			if (CameraErrored && ControllerConfiguration.Instance.FixCameraAfterReturn)
			{
				ServiceLocator.Instance.GetService<IScannerDeviceService>().GetConfiguredDevice().Restart();
			}
		}
	}

	private void ReturnToUserOrFile(Action<string> onFileDisk, Action<string> onDiskReturned, string barcode)
	{
		if (base.MotionService.MoveVend(MoveMode.None, base.Context.AppLog) != ErrorCodes.Success)
		{
			OnMoveVendError();
			return;
		}
		LogHelper.Instance.WithContext("Start pushing item out of picker.");
		IControllerService service = ServiceLocator.Instance.GetService<IControllerService>();
		IVendItemResult vendItemResult = service.VendItemInPicker();
		if (ErrorCodes.PickerEmpty == vendItemResult.Status)
		{
			onDiskReturned(barcode);
			base.Context.AppLog.WriteFormatted("The disk was returned to the user.");
			return;
		}
		if (ErrorCodes.PickerFull != vendItemResult.Status)
		{
			base.Context.Send("MachineError");
			AddError("Hardware error.");
			return;
		}
		LogHelper.Instance.WithContext("Disk not taken; accepting disk into kiosk.");
		ErrorCodes errorCodes = service.AcceptDiskAtDoor();
		if (ErrorCodes.PickerEmpty == errorCodes)
		{
			onDiskReturned(barcode);
			base.Context.AppLog.WriteFormatted("The disk was returned to the user.");
		}
		else if (ErrorCodes.PickerFull == errorCodes)
		{
			base.Context.AppLog.Write("Customer did not take disc; accept into kiosk.");
			base.ControlSystem.TrackCycle();
			onFileDisk(barcode);
		}
		else if (ErrorCodes.PickerObstructed == errorCodes || ErrorCodes.VendDoorCloseTimeout == errorCodes)
		{
			ErrorCodes errorCodes2 = RejectDisk();
			if (ErrorCodes.PickerFull == errorCodes2)
			{
				base.ControlSystem.TrackCycle();
				onFileDisk(barcode);
			}
			else
			{
				_ = 28;
			}
		}
	}

	private void FileUnrecognizedDisk(string matrix)
	{
		ApplicationLog appLog = base.Context.AppLog;
		if (ControllerConfiguration.Instance.IsVMZMachine)
		{
			IDumpbinService service = ServiceLocator.Instance.GetService<IDumpbinService>();
			if (service.IsFull())
			{
				FileDiskInKiosk("UNKNOWN");
				return;
			}
			ErrorCodes errorCodes = ServiceLocator.Instance.GetService<IMotionControlService>().MoveTo(service.PutLocation, MoveMode.Put, appLog);
			if (errorCodes != ErrorCodes.Success)
			{
				base.Context.CreateInfoResult("MachineError", $"MOVE failed with error {errorCodes.ToString()}");
				AddError("The MOVE instruction failed.");
				return;
			}
			IPutResult putResult = ServiceLocator.Instance.GetService<IControllerService>().Put(matrix);
			if (!putResult.Success)
			{
				base.Context.CreateInfoResult("PutError", $"PUT failed with error {putResult.ToString()}");
				AddError("The PUT instruction failed.");
			}
			else
			{
				base.Context.CreateInfoResult("PutToBinOK", "The disk was successfully filed", matrix);
				appLog.WriteFormatted("The barcode {0} was put into the dump bin.", matrix);
			}
		}
		else
		{
			FileDiskInKiosk("UNKNOWN");
		}
	}

	private void FileDiskInKiosk(string scannedId)
	{
		ServiceLocator.Instance.GetService<IPersistentCounterService>();
		LogHelper.Instance.WithContext("The ID in the picker: {0}", scannedId);
		using FileDiskOperation fileDiskOperation = new FileDiskOperation(this, base.Context);
		if (fileDiskOperation.PutDiscAway(base.Result, scannedId) != ErrorCodes.Success)
		{
			AddError("Failed to file disk.");
		}
	}

	private ErrorCodes RejectDisk()
	{
		ErrorCodes errorCodes = ServiceLocator.Instance.GetService<IControllerService>().RejectDiskInPicker();
		switch (errorCodes)
		{
		case ErrorCodes.VendDoorRentTimeout:
		case ErrorCodes.VendDoorCloseTimeout:
		case ErrorCodes.SensorReadError:
			base.Context.Send("MachineError");
			AddError("MACHINE ERROR");
			break;
		case ErrorCodes.PickerObstructed:
			base.Context.CreateItemStuckResult(null);
			AddError("There is an item stuck in the gripper.");
			break;
		}
		return errorCodes;
	}

	private void OnMoveVendError()
	{
		base.Context.Send("MachineError");
		base.Context.CreateInfoResult("MachineError", "Unable to move to the vend position.");
		AddError("Unable to move to the vend position.");
	}

	private bool ReadDisk(out string scannedId)
	{
		if (!ServiceLocator.Instance.GetService<IScannerDeviceService>().GetConfiguredDevice().SupportsSecureReads)
		{
			return OnReadWithLegacy(out scannedId);
		}
		return OnReadWithIR(out scannedId);
	}

	private bool OnReadWithIR(out string scannedId)
	{
		scannedId = "UNKNOWN";
		CenterDiskMethod[] methods = new CenterDiskMethod[CenterMethods.Length];
		Array.Copy(CenterMethods, methods, CenterMethods.Length);
		base.Context.ContextLog.WriteFormatted("Number Of Center Methods before resize {0}", methods.Length);
		Scanner.CheckAndResizeForAdditionalFraudAttempts(ref methods);
		base.Context.AppLog.WriteFormatted("Number Of Center Methods: {0}, RetryEnabled: {1}, AdditionalAttempts: {2}", methods.Length, ControllerConfiguration.Instance.RetryReadOnNoMarkersFound, ControllerConfiguration.Instance.AdditionalFraudReadAttempts);
		base.Context.ContextLog.WriteFormatted("Number Of Center Methods {0}", methods.Length);
		for (int i = 0; i < methods.Length; i++)
		{
			base.ControlSystem.Center(methods[i]);
			using Scanner scanner = new Scanner(base.ControlSystem);
			using ScanResult scanResult = scanner.Read();
			scanResult.ReadAttempts = i + 1;
			if (!scanResult.SnapOk)
			{
				Scanner.CreateJsonResult(scanResult);
				return OnCameraFailure();
			}
			m_cameraErrors = 0;
			if (i + 1 < methods.Length && ControllerConfiguration.Instance.RetryReadOnNoMarkersFound && scanResult.SecureTokensFound <= 0)
			{
				base.Context.AppLog.WriteFormatted("{0} - ReturnJob Retry Read Image - No Secure Tokens Found.", i);
				continue;
			}
			if (scanResult.ReadCode)
			{
				using (FraudValidator fraudValidator = new FraudValidator(base.Context))
				{
					fraudValidator.Validate(scanResult);
					scannedId = scanResult.ScannedMatrix;
				}
				Scanner.CreateJsonResult(scanResult);
				return OnBarcodeRead(scannedId);
			}
			if (i + 1 > methods.Length)
			{
				Scanner.CreateJsonResult(scanResult);
			}
		}
		scannedId = "UNKNOWN";
		return OnNoRead(scannedId);
	}

	private bool OnReadWithLegacy(out string scannedId)
	{
		scannedId = "UNKNOWN";
		using (Scanner scanner = new Scanner(base.ControlSystem))
		{
			for (int i = 0; i < CenterMethods.Length; i++)
			{
				base.ControlSystem.Center(CenterMethods[i]);
				using ScanResult scanResult = scanner.Read();
				scanResult.ReadAttempts = i + 1;
				if (!scanResult.SnapOk)
				{
					Scanner.CreateJsonResult(scanResult);
					return OnCameraFailure();
				}
				m_cameraErrors = 0;
				if (scanResult.ReadCode)
				{
					scannedId = scanResult.ScannedMatrix;
					Scanner.CreateJsonResult(scanResult);
					scanResult.CopyToFraudFolder();
					return OnBarcodeRead(scannedId);
				}
				if (i + 1 > CenterMethods.Length)
				{
					Scanner.CreateJsonResult(scanResult);
				}
			}
		}
		scannedId = "UNKNOWN";
		return OnNoRead(scannedId);
	}

	private bool OnNoRead(string scannedId)
	{
		base.Context.CreateInfoResult("BadIdRead", "The READ request failed.", scannedId);
		return false;
	}

	private bool OnCameraFailure()
	{
		CameraErrored = true;
		if (!ControllerConfiguration.Instance.FixCameraAfterReturn || ++m_cameraErrors >= ControllerConfiguration.Instance.ReturnCameraAlertThreshold)
		{
			base.Context.CreateInfoResult("CameraCaptureFailure", "The CAMERA CAPTURE instruction failed.");
		}
		return false;
	}

	private bool OnBarcodeRead(string matrix)
	{
		base.Context.Send($"ReturnEnd:{matrix}");
		return true;
	}
}
