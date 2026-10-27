using Redbox.HAL.Component.Model;
using Redbox.HAL.Controller.Framework;

namespace Redbox.HAL.Script.Framework;

internal abstract class ThinHelper
{
	protected ApplicationLog AppLog => Context.AppLog;

	protected ExecutionResult Result { get; private set; }

	protected ExecutionContext Context { get; private set; }

	internal static bool Debug => ControllerConfiguration.Instance.EnableVMZTrace;

	internal static ThinHelper MakeInstance(ExecutionResult result, ExecutionContext context)
	{
		if (!ControllerConfiguration.Instance.IsVMZMachine)
		{
			return new QlmThinHelper(result, context);
		}
		return new VMZThinHelper(result, context);
	}

	internal MerchandizeResult ProcessSuccessfulThin(ILocation loc, string id, MerchFlags flags)
	{
		if (!ControllerConfiguration.Instance.IsVMZMachine)
		{
			Context.CreateResult("ThinTransferSuccessful", "The item was transferred from storage to the QLM.", loc.Deck, loc.Slot, id, null, null);
			return MerchandizeResult.Success;
		}
		if (ServiceLocator.Instance.GetService<IDumpbinService>().IsBin(loc))
		{
			Context.CreateResult("ThinTransferSuccessful", "The item was transferred from storage to the dump bin.", null, null, id, null, null);
		}
		else
		{
			Context.CreateResult(ControllerConfiguration.Instance.IsVMZMachine ? "DiskTransferSuccessful" : "ThinTransferSuccessful", "The item was transferred from storage to the VMZ.", loc.Deck, loc.Slot, id, null, null);
			IInventoryService service = ServiceLocator.Instance.GetService<IInventoryService>();
			loc.Flags = flags;
			service.Save(loc);
		}
		return MerchandizeResult.Success;
	}

	internal virtual MerchandizeResult ThinDisk(string id, MerchFlags flags)
	{
		Context.AppLog.Write($"Attempt thin of item {id} (flags = {flags.ToString()})");
		IInventoryService service = ServiceLocator.Instance.GetService<IInventoryService>();
		ILocation location = service.Lookup(id);
		if (location == null)
		{
			Context.CreateLookupErrorResult("ThinLookupFailed", id);
			return MerchandizeResult.LookupFailure;
		}
		if (ControllerConfiguration.Instance.IsVMZMachine && MerchSegmentFactory.Get(flags).Contains(location))
		{
			LogHelper.Instance.Log("The disk {0} {1} is already in its segment; do not move.", id, location.ToString());
			location.Flags = flags;
			service.Save(location);
			Context.CreateResult("VMZThinNotMoved", "The thin item is already in the VMZ; do not move.", location.Deck, location.Slot, id, null, null);
			return MerchandizeResult.DiskInVMZ;
		}
		ILocation location2 = FindEmptyTarget(flags, out var result);
		if (Debug)
		{
			LogHelper.Instance.Log("FindEmptyTarget returned {0}; location = {1}", result.ToString(), (location2 == null) ? "None" : location2.ToString());
		}
		if (location2 == null)
		{
			if (!ControllerConfiguration.Instance.IsVMZMachine)
			{
				return result;
			}
			if (result != MerchandizeResult.VMZFull)
			{
				return result;
			}
			if (!MerchSegmentFactory.Get(flags).CanDump)
			{
				return MerchandizeResult.VMZFull;
			}
			IDumpbinService service2 = ServiceLocator.Instance.GetService<IDumpbinService>();
			if (service2.IsFull())
			{
				return result;
			}
			location2 = service2.PutLocation;
		}
		location = service.Lookup(id);
		if (location == null)
		{
			Context.CreateLookupErrorResult("ThinLookupFailed", id);
			return MerchandizeResult.LookupFailure;
		}
		MerchandizeResult merchandizeResult = FetchThin(location, id);
		if (merchandizeResult != MerchandizeResult.Success)
		{
			return merchandizeResult;
		}
		MerchandizeResult merchandizeResult2 = ThinDiskInner(id, location, location2, flags);
		if (merchandizeResult2 == MerchandizeResult.Success)
		{
			return merchandizeResult2;
		}
		ReturnToSource(location, id);
		return merchandizeResult2;
	}

	protected virtual MerchandizeResult ThinDiskInner(string id, ILocation source, ILocation target, MerchFlags flags)
	{
		return ThinDiskToLocation(source, target, id, flags);
	}

	protected abstract ILocation FindEmptyTarget(MerchFlags flags, out MerchandizeResult result);

	protected MerchandizeResult ThinDiskToLocation(ILocation source, ILocation target, string id, MerchFlags flags)
	{
		if (LogHelper.Instance.IsLevelEnabled(LogEntryType.Debug))
		{
			LogHelper.Instance.WithContext("Transfer from {0} --> {1}.", source.ToString(), target.ToString());
		}
		IDumpbinService service = ServiceLocator.Instance.GetService<IDumpbinService>();
		if (target == service.PutLocation && service.IsFull())
		{
			return MerchandizeResult.DumpBinFull;
		}
		ErrorCodes errorCodes = ServiceLocator.Instance.GetService<IMotionControlService>().MoveTo(target, MoveMode.Put, Context.AppLog);
		if (errorCodes != ErrorCodes.Success)
		{
			MerchandizingHelper.Instance.CleanupJob(Result, Context, errorCodes);
			MerchandizingHelper.Instance.ExitOnMoveError(Result, Context);
			return MerchandizeResult.HardwareError;
		}
		ServiceLocator.Instance.GetService<IControlSystem>().TrackCycle();
		IPutResult putResult = ServiceLocator.Instance.GetService<IControllerService>().Put(id);
		if (putResult.Success)
		{
			return ProcessSuccessfulThin(target, id, flags);
		}
		if (putResult.IsSlotInUse)
		{
			Context.CreateResult(putResult.ToString().ToUpper(), "The slot is in use.", target.Deck, target.Slot, id, null, null);
			if (ControllerConfiguration.Instance.IsVMZMachine)
			{
				return MerchandizeResult.TransferFailure;
			}
			return MerchandizeResult.SlotInUse;
		}
		return MerchandizeResult.TransferFailure;
	}

	protected MerchandizeResult ReturnToSource(ILocation source, string id)
	{
		LogHelper.Instance.WithContext("Unable to find a slot for the thin; put it back and bail.");
		if (ServiceLocator.Instance.GetService<IMotionControlService>().MoveTo(source, MoveMode.Put, Context.AppLog) != ErrorCodes.Success)
		{
			MerchandizingHelper.Instance.ExitOnMoveError(Result, Context);
			return MerchandizeResult.HardwareError;
		}
		ServiceLocator.Instance.GetService<IControlSystem>().TrackCycle();
		if (ServiceLocator.Instance.GetService<IControllerService>().Put(id).Success)
		{
			return MerchandizeResult.HardwareError;
		}
		return MerchandizeResult.MachineFull;
	}

	protected ThinHelper(ExecutionResult result, ExecutionContext context)
	{
		Result = result;
		Context = context;
	}

	protected MerchandizeResult FetchThin(ILocation source, string id)
	{
		int deck = source.Deck;
		int slot = source.Slot;
		Context.CreateResult("ThinDiskFromDrum", "Removing disk from drum for thinning.", deck, slot, id, null, null);
		ErrorCodes errorCodes = ServiceLocator.Instance.GetService<IMotionControlService>().MoveTo(source, MoveMode.Get, Context.AppLog);
		if (errorCodes != ErrorCodes.Success)
		{
			MerchandizingHelper.Instance.CleanupJob(Result, Context, errorCodes);
			MerchandizingHelper.Instance.ExitOnMoveError(Result, Context);
			return MerchandizeResult.HardwareError;
		}
		IControllerService service = ServiceLocator.Instance.GetService<IControllerService>();
		IGetResult getResult = (ControllerConfiguration.Instance.IsVMZMachine ? service.Get(new FetchThinGetObserver()) : service.Get());
		if (getResult.IsSlotEmpty)
		{
			return MerchandizeResult.EmptyStuck;
		}
		if (getResult.Success)
		{
			return MerchandizeResult.Success;
		}
		if (service.ClearGripper() != ErrorCodes.Success)
		{
			Context.CreateResult("ThinItemStuck", "Could not retrieve the item.", deck, slot, null, null, null);
			LogHelper.Instance.WithContext("The gripper is obstructed - bailing thin.");
			return MerchandizeResult.HardwareError;
		}
		IPickerSensorReadResult pickerSensorReadResult = ServiceLocator.Instance.GetService<IControlSystem>().ReadPickerSensors();
		if (!pickerSensorReadResult.Success)
		{
			return MerchandizeResult.HardwareError;
		}
		if (!pickerSensorReadResult.IsFull)
		{
			Context.CreateResult("ThinItemStuck", "Could not retrieve the item.", deck, slot, null, null, null);
			return MerchandizeResult.EmptyStuck;
		}
		return MerchandizeResult.HardwareError;
	}
}
