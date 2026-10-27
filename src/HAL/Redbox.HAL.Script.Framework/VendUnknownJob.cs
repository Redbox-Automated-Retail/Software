using Redbox.HAL.Component.Model;
using Redbox.HAL.Controller.Framework;

namespace Redbox.HAL.Script.Framework;

[NativeJob(ProgramName = "vend-unknown", Operand = "VEND-UNKNOWN")]
internal sealed class VendUnknownJob : NativeJobAdapter, IGetObserver
{
	private const string HW_ERR_MSG = "Unable to remove unknown due to hardware errors. Inspect the slot after completing the process.";

	public void OnStuck(IGetResult result)
	{
	}

	public bool OnEmpty(IGetResult result)
	{
		if (!ControllerConfiguration.Instance.TestSlotDuringRemoval)
		{
			result.Update(ErrorCodes.ItemStuck);
			return false;
		}
		using PeekOperation peekOperation = new PeekOperation();
		IPeekResult peekResult = peekOperation.Execute();
		if (!peekResult.TestOk)
		{
			result.Update(peekResult.Error);
			return false;
		}
		if (!peekResult.IsFull)
		{
			return true;
		}
		result.Update(ErrorCodes.ItemStuck);
		return false;
	}

	protected override void ExecuteInner()
	{
		ApplicationLog.ConfigureLog(base.Context, logAppends: true, "Unknowns", flushOnWrite: false, "VendUnknown").Write("Vend unknown start.");
		base.Context.Send("VendStart");
		ServiceLocator.Instance.GetService<IRuntimeService>().SpinWait(30);
		base.Context.Send("VendGraphicDisplay");
		using (CheckPickerOperation checkPickerOperation = new CheckPickerOperation(base.Context))
		{
			if (ErrorCodes.PickerEmpty != checkPickerOperation.CheckPicker(base.Result, null))
			{
				AddError("Unable to clear the picker.");
				return;
			}
		}
		int num = base.Context.PopTop<int>();
		int num2 = base.Context.PopTop<int>();
		IInventoryService service = ServiceLocator.Instance.GetService<IInventoryService>();
		ILocation location = service.Get(num, num2);
		LogHelper.Instance.Log("Move to {0} to pick requested item.", location.ToString());
		IMotionControlService service2 = ServiceLocator.Instance.GetService<IMotionControlService>();
		if (service2.MoveTo(num, num2, MoveMode.Get, base.Context.AppLog) != ErrorCodes.Success)
		{
			base.Context.Send("MachineError");
			base.Context.CreateResult("MoveError", "Unable to remove unknown due to hardware errors. Inspect the slot after completing the process.", num, num2, null, null, null);
			AddError("MACHINE ERROR");
			return;
		}
		IControllerService service3 = ServiceLocator.Instance.GetService<IControllerService>();
		IGetResult getResult = service3.Get(this);
		if (getResult.IsSlotEmpty || getResult.ItemStuck)
		{
			if (!ControllerConfiguration.Instance.RotateDrumDuringUnknownRemoval2)
			{
				base.Context.CreateResult("SlotEmpty", "The slot is empty.", num, num2, null, null, null);
				VendEnd();
			}
			else if (getResult.IsSlotEmpty)
			{
				base.Context.AppLog.WriteFormatted("Slot is empty, moving to next.");
				base.Context.CreateResult("SlotEmpty", "The slot is empty.", num, num2, null, null, null);
				VendEnd();
			}
			else
			{
				LogHelper.Instance.Log("Disc stuck in slot, moving to next.");
				base.Context.CreateResult("GetAfterPeekShowsFull", "This Location was NOT updated in inventory.", num, num2, getResult.Previous, getResult.ReturnTime, null);
				ShowEmptyStuck(location);
			}
			return;
		}
		if (!getResult.Success)
		{
			base.Context.CreateResult("GetError", "Unable to remove unknown due to hardware errors. Inspect the slot after completing the process.", num, num2, null, null, null);
			AddError("MACHINE ERROR");
			return;
		}
		base.Context.Send("MoveToVend");
		if (base.MotionService.MoveVend(MoveMode.None, base.Context.AppLog) != ErrorCodes.Success)
		{
			base.Context.Send("MachineError");
			base.Context.CreateResult("MoveError", "Unable to remove unknown due to hardware errors. Inspect the slot after completing the process.", num, num2, null, null, null);
			AddError("MACHINE ERROR");
			return;
		}
		base.Context.Send("PickupGraphicDisplay");
		IVendItemResult vendItemResult = service3.VendItemInPicker();
		if (vendItemResult.Status == ErrorCodes.PickerEmpty)
		{
			base.Context.Send("ItemVended");
			base.Context.AppLog.Write("The item was taken.");
			base.Context.CreateResult("ItemVended", "The item was successfully vended.", num, num2, getResult.Previous, getResult.ReturnTime, null);
			VendEnd();
			return;
		}
		if (vendItemResult.Status != ErrorCodes.PickerFull)
		{
			base.Context.Send("MachineError");
			base.Context.CreateInfoResult("MachineError", "Unable to remove unknown due to hardware errors. Inspect the slot after completing the process.");
			AddError("MACHINE ERROR");
			return;
		}
		LogHelper.Instance.Log("The item was not taken; pull it in and file it away.");
		ErrorCodes errorCodes = service3.AcceptDiskAtDoor();
		if (ErrorCodes.PickerFull != errorCodes)
		{
			base.Context.CreateResult("RollInToSensor3TimedOut", "Unable to remove unknown due to hardware errors. Inspect the slot after completing the process.", num, num2, null, null, null);
			AddError("MACHINE ERROR");
			return;
		}
		if (service2.MoveTo(location, MoveMode.Put, base.Context.AppLog) != ErrorCodes.Success)
		{
			base.Context.Send("MachineError");
			base.Context.CreateResult("MoveError", "Unable to remove unknown due to hardware errors. Inspect the slot after completing the process.", num, num2, null, null, null);
			AddError("MACHINE ERROR");
			return;
		}
		base.ControlSystem.TrackCycle();
		if (!service3.Put("UNKNOWN").Success)
		{
			base.Context.CreateResult("PutError", "Unable to remove unknown due to hardware errors. Inspect the slot after completing the process.", num, num2, null, null, null);
			AddError("MACHINE ERROR");
			return;
		}
		location.ReturnDate = getResult.ReturnTime;
		service.Save(location);
		base.Context.CreateResult("UnclaimedVend", "The vend request failed because the item was not taken.  It has been returned.", num, num2, getResult.Previous, null, null);
		VendEnd();
	}

	internal VendUnknownJob(ExecutionResult r, ExecutionContext c)
		: base(r, c)
	{
	}

	private void VendEnd()
	{
		base.ControlSystem.VendDoorClose();
		base.ControlSystem.TrackClose();
		base.MotionService.MoveVend(MoveMode.Get, base.Context.AppLog);
	}
}
