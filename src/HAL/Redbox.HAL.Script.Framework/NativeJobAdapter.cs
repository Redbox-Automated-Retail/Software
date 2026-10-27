using Redbox.HAL.Component.Model;
using Redbox.HAL.Controller.Framework;

namespace Redbox.HAL.Script.Framework;

internal abstract class NativeJobAdapter
{
	protected ApplicationLog AppLog => Context.AppLog;

	protected IInventoryService InventoryService => ServiceLocator.Instance.GetService<IInventoryService>();

	protected IDumpbinService DumpbinService => ServiceLocator.Instance.GetService<IDumpbinService>();

	protected IMotionControlService MotionService => ServiceLocator.Instance.GetService<IMotionControlService>();

	protected IDecksService DecksService => ServiceLocator.Instance.GetService<IDecksService>();

	protected IControlSystem ControlSystem => ServiceLocator.Instance.GetService<IControlSystem>();

	protected IControllerService ControlService => ServiceLocator.Instance.GetService<IControllerService>();

	protected ExecutionResult Result { get; private set; }

	protected ExecutionContext Context { get; private set; }

	internal string Name { get; set; }

	protected abstract void ExecuteInner();

	protected void AddError(string errMsg)
	{
		Result.Errors.Add(Error.NewError("E999", "Execution context error.", errMsg));
	}

	protected void RegisterForCorrectionEvents()
	{
		if (ControllerConfiguration.Instance.HandleHardwareCorrectionEvents)
		{
			Context.StartHardwareCorrectionEvent += delegate(HardwareCorrectionEventArgs s)
			{
				LogHelper.Instance.WithContext(false, LogEntryType.Info, "Hardware correction start event ( Statistic = {0} ).", s.Statistic);
				Context.Send($"HardwareCorrectionStart:{s.Statistic}");
			};
			Context.EndHardwareCorrectionEvent += delegate(HardwareCorrectionEventArgs s)
			{
				LogHelper.Instance.WithContext("Hardware correction end event ( Statistic = {0} status = {1} ).", s.Statistic, s.CorrectionOk);
				Context.Send(s.CorrectionOk ? $"HardwareCorrectionSuccess:{s.Statistic}" : $"HardwareCorrectionFailure:{s.Statistic}");
			};
		}
	}

	protected void ShowEmptyStuck(ILocation location)
	{
		using MoveDecoratorService moveDecoratorService = new MoveDecoratorService(Context);
		if (moveDecoratorService.ShowEmptyStuck(location) != ErrorCodes.Success)
		{
			AddError("There was an error moving to the offset position.");
		}
	}

	protected bool MoveAndGet(int deck, int slot)
	{
		if (MotionService.MoveTo(deck, slot, MoveMode.Get, AppLog) != ErrorCodes.Success)
		{
			return false;
		}
		return ServiceLocator.Instance.GetService<IControllerService>().Get().Success;
	}

	protected bool MoveAndPut(int deck, int slot, string id)
	{
		if (MotionService.MoveTo(deck, slot, MoveMode.Put, AppLog) != ErrorCodes.Success)
		{
			return false;
		}
		ControlSystem.TrackCycle();
		return ServiceLocator.Instance.GetService<IControllerService>().Put(id).Success;
	}

	protected bool ContextSwitchRequested()
	{
		if (Context.DeferredStatusChangeRequested.HasValue)
		{
			LogHelper.Instance.Log($"SwitchRequested: deferred change to {Context.DeferredStatusChangeRequested.Value.ToString()}", LogEntryType.Info);
			Context.PromoteDeferredStatusChange();
			return true;
		}
		Context.FlushMessages();
		if (Context.Messages.Count != 0)
		{
			LogHelper.Instance.Log($"Context {Context.ID} {Context.ProgramName}: msg count = {Context.Messages.Count}", LogEntryType.Info);
		}
		return false;
	}

	protected bool TestInventoryStore()
	{
		if (InventoryService.CheckIntegrity() == ErrorCodes.Success)
		{
			return true;
		}
		Context.AppLog.WriteFormatted("The inventory store is corrupt.");
		Context.CreateInfoResult("InventoryStoreError", "Inventory database integrity test failed.");
		AddError("Datastore corrupt");
		return false;
	}

	protected NativeJobAdapter(ExecutionResult result, ExecutionContext context)
	{
		Result = result;
		Context = context;
	}

	internal void Execute()
	{
		ExecuteInner();
	}
}
