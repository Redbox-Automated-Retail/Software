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
		IInventoryService service = ServiceLocator.Instance.GetService<IInventoryService>();
		int num = base.Context.PopTop<int>();
		int num2 = base.Context.PopTop<int>();
		base.Context.AppLog.WriteFormatted("Vend time is set to {0}s, vend frequency = {1}", num2, num);
		base.Context.AppLog.Write("Random sync test start.");
		RandomLocationSelector randomLocationSelector = new RandomLocationSelector(base.DecksService, service);
		int num3 = 0;
		int num4 = 0;
		bool flag = true;
		while (flag)
		{
			ILocation location = randomLocationSelector.SelectTargetLocation();
			if (location == null)
			{
				flag = false;
				break;
			}
			base.Context.AppLog.WriteFormatted("Sync {0}", location.ToString());
			num4++;
			bool vend = false;
			if (num != 0 && num4 % num == 0)
			{
				vend = true;
			}
			RandomSyncDecorator randomSyncDecorator = new RandomSyncDecorator(base.Context, base.Result, location, num2, vend);
			SyncResult syncResult = SyncHelper.SyncSlot(base.Result, base.Context, location, randomSyncDecorator);
			if (syncResult == SyncResult.HardwareError || syncResult == SyncResult.GetError)
			{
				base.Context.AppLog.WriteFormatted("Sync returned error {0}", syncResult.ToString());
				flag = false;
				AddError("Sync encountered an error.");
			}
			if (randomSyncDecorator.SecureReads > 0)
			{
				num3++;
			}
			if (ContextSwitchRequested())
			{
				flag = false;
			}
		}
		base.Context.AppLog.WriteFormatted("Disk count with secure code finds: {0}", num3);
	}

	internal RandomSync(ExecutionResult result, ExecutionContext context)
		: base(result, context)
	{
	}
}
