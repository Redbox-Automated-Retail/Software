using System.Threading;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Script.Framework;

[NativeJob(ProgramName = "reset-controlsystem-job")]
internal class ResetControlSystemJob : NativeJobAdapter
{
	protected override void ExecuteInner()
	{
		IControlSystem service = ServiceLocator.Instance.GetService<IControlSystem>();
		service.Shutdown();
		Thread.Sleep(500);
		IControlResponse controlResponse = service.Initialize();
		string message = $"RESET returned {controlResponse.ToString()}";
		base.Context.CreateInfoResult("ResetStatus", message);
		if (!controlResponse.Success)
		{
			AddError("RESET failure.");
		}
	}

	internal ResetControlSystemJob(ExecutionResult result, ExecutionContext ctx)
		: base(result, ctx)
	{
	}
}
