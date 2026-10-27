using Redbox.HAL.Component.Model;
using Redbox.HAL.Controller.Framework;

namespace Redbox.HAL.Script.Framework;

[NativeJob(ProgramName = "clear-merch-status", Operand = "CLEAR-MERCH-STATUS")]
internal sealed class ClearMerchStatusJob : NativeJobAdapter
{
	protected override void ExecuteInner()
	{
		int num = 0;
		IInventoryService service = ServiceLocator.Instance.GetService<IInventoryService>();
		try
		{
			if (!ControllerConfiguration.Instance.IsVMZMachine)
			{
				return;
			}
			int num2 = base.Context.PopTop<int>();
			for (int i = 0; i < num2; i++)
			{
				string id = base.Context.PopTop<string>();
				ILocation location = service.Lookup(id);
				if (location != null && SegmentManager.Instance.InCompressedZone(location) && !SegmentManager.Instance.IsNonThinType(location.Flags))
				{
					LogHelper.Instance.WithContext("The flags at {0} are changed from {1} -> None", location.ToString(), location.Flags.ToString());
					location.Flags = MerchFlags.None;
					service.Save(location);
					num++;
				}
			}
		}
		finally
		{
			base.Context.CreateInfoResult("MerchFlagsResetCount", num.ToString());
		}
	}

	internal ClearMerchStatusJob(ExecutionResult result, ExecutionContext ctx)
		: base(result, ctx)
	{
	}
}
