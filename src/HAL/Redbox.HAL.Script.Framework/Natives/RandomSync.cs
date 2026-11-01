using Redbox.HAL.Component.Model;
using Redbox.HAL.Controller.Framework;

namespace Redbox.HAL.Script.Framework.Natives;

[NativeJob(ProgramName = "random-sync", Operand = "RANDOMSYNC", HideFromList = true)]
internal sealed class RandomSync : NativeJobAdapter
{
	protected override void ExecuteInner()
	{
		ApplicationLog applicationLog = ApplicationLog.ConfigureLog(base.Context, logAppends: false, "RandomSync", flushOnWrite: true, null);
		if (!ControllerConfiguration.Instance.IsVMZMachine)
		{
			applicationLog.Write("The random sync job is only supported on VMZ kiosks. Nice try.");
			AddError("Unsupported configuration.");
			return;
		}
		IInventoryService inventoryService = ServiceLocator.Instance.GetService<IInventoryService>();
		int vendFrequency = base.Context.PopTop<int>();
		int vendTime = base.Context.PopTop<int>();
		base.Context.AppLog.WriteFormatted("Vend time is set to {0}s, vend frequency = {1}", vendTime, vendFrequency);
		base.Context.AppLog.Write("Random sync test start.");
		RandomLocationSelector randomLocationSelector = new RandomLocationSelector(base.DecksService, inventoryService);
		int secureFinds = 0;
		int visited = 0;
		bool running = true;
		while (running)
		{
			ILocation location = randomLocationSelector.SelectTargetLocation();
			if (location == null)
			{
				running = false;
				break;
			}
			base.Context.AppLog.WriteFormatted("Sync {0}", location.ToString());
			visited++;
			bool vend = false;
			if (vendFrequency != 0 && visited % vendFrequency == 0)
			{
				vend = true;
			}
			RandomSyncDecorator randomSyncDecorator = new RandomSyncDecorator(base.Context, base.Result, location, vendTime, vend);
			SyncResult syncResult = SyncHelper.SyncSlot(base.Result, base.Context, location, randomSyncDecorator);
			if (syncResult == SyncResult.HardwareError || syncResult == SyncResult.GetError)
			{
				base.Context.AppLog.WriteFormatted("Sync returned error {0}", syncResult.ToString());
				running = false;
				AddError("Sync encountered an error.");
			}
			if (randomSyncDecorator.SecureReads > 0)
			{
				secureFinds++;
			}
			if (ContextSwitchRequested())
			{
				running = false;
			}
		}
		base.Context.AppLog.WriteFormatted("Disk count with secure code finds: {0}", secureFinds);
	}

	internal RandomSync(ExecutionResult result, ExecutionContext context)
		: base(result, context)
	{
	}
}
