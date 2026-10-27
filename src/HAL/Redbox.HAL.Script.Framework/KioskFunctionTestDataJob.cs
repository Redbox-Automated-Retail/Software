using System;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Script.Framework;

[NativeJob(ProgramName = "kiosk-function-test-data", Operand = "KIOSK-FUNCTION-TEST-DATA")]
internal sealed class KioskFunctionTestDataJob : NativeJobAdapter
{
	protected override void ExecuteInner()
	{
		string verticalSlotTestResult = base.Context.PopTop<string>();
		string initResult = base.Context.PopTop<string>();
		string vendDoorTestResult = base.Context.PopTop<string>();
		string trackTestResult = base.Context.PopTop<string>();
		string snapDecodeTestResult = base.Context.PopTop<string>();
		string cameraDriverTestResult = base.Context.PopTop<string>();
		string touchscreenDriverTestResult = base.Context.PopTop<string>();
		DateTime timestamp = DateTime.Parse(base.Context.PopTop<string>());
		string userIdentifier = base.Context.PopTop<string>();
		IKioskFunctionCheckData data = ServiceLocator.Instance.GetService<ITableTypeFactory>().NewCheckData(verticalSlotTestResult, initResult, vendDoorTestResult, trackTestResult, snapDecodeTestResult, touchscreenDriverTestResult, cameraDriverTestResult, timestamp, userIdentifier);
		ServiceLocator.Instance.GetService<IKioskFunctionCheckService>().Add(data);
	}

	internal KioskFunctionTestDataJob(ExecutionResult result, ExecutionContext ctx)
		: base(result, ctx)
	{
	}
}
