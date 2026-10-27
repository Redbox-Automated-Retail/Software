using System.Threading;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Controller.Framework;

namespace Redbox.HAL.Script.Framework;

[NativeJob(ProgramName = "hardware-healer", Operand = "HARDWARE-HEALER")]
internal class HardwareHealerJob : NativeJobAdapter
{
	protected override void ExecuteInner()
	{
		ApplicationLog.ConfigureLog(base.Context, logAppends: true, "Service", flushOnWrite: false, null);
		if (ControllerConfiguration.Instance.EnableLeiweHealing && !base.ControlSystem.GetRevision().Success)
		{
			base.ControlSystem.Shutdown();
			Thread.Sleep(500);
			base.ControlSystem.Initialize();
			if (!base.ControlSystem.GetRevision().Success)
			{
				AddError("Failed to reset boards.");
			}
		}
		if (ControllerConfiguration.Instance.EnableArcusHealing)
		{
			if (!base.MotionService.TestAndReset(quick: false))
			{
				AddError("Can't reset controller.");
			}
			else
			{
				base.MotionService.MoveVend(MoveMode.Get, base.Context.AppLog);
			}
		}
		if (!ControllerConfiguration.Instance.EnableCameraHealing)
		{
			return;
		}
		IScannerDevice configuredDevice = ServiceLocator.Instance.GetService<IScannerDeviceService>().GetConfiguredDevice();
		using ISnapResult snapResult = configuredDevice.Snap();
		if (!snapResult.SnapOk)
		{
			bool flag = configuredDevice.Restart();
			LogHelper.Instance.WithContext("Camera snap returned failure; restart device returned {0}", flag.ToString());
		}
	}

	internal HardwareHealerJob(ExecutionResult result, ExecutionContext ctx)
		: base(result, ctx)
	{
	}
}
