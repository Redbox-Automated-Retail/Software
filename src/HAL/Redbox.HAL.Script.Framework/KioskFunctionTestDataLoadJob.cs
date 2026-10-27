using System.Collections.Generic;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Script.Framework;

[NativeJob(ProgramName = "load-kiosk-function-check-data", Operand = "LOAD-KIOSK-FUNCTION-CHECK-DATA")]
internal sealed class KioskFunctionTestDataLoadJob : NativeJobAdapter
{
	protected override void ExecuteInner()
	{
		IList<IKioskFunctionCheckData> list = ServiceLocator.Instance.GetService<IKioskFunctionCheckService>().Load();
		foreach (IKioskFunctionCheckData item in list)
		{
			base.Context.PushTop(item.UserIdentifier);
			base.Context.PushTop(item.Timestamp.ToString());
			base.Context.PushTop(item.TouchscreenDriverTestResult);
			base.Context.PushTop(item.CameraDriverTestResult);
			base.Context.PushTop(item.SnapDecodeTestResult);
			base.Context.PushTop(item.TrackTestResult);
			base.Context.PushTop(item.VendDoorTestResult);
			base.Context.PushTop(item.InitTestResult);
			base.Context.PushTop(item.VerticalSlotTestResult);
		}
		base.Context.PushTop(list.Count);
	}

	internal KioskFunctionTestDataLoadJob(ExecutionResult result, ExecutionContext ctx)
		: base(result, ctx)
	{
	}
}
