using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Script.Framework;

[NativeJob(ProgramName = "power-cycle-router")]
internal sealed class ResetRouterJob : NativeJobAdapter
{
	protected override void ExecuteInner()
	{
		IPowerCycleDevice powerCycleDevice = ServiceLocator.Instance.GetService<IPowerCycleDeviceService>().Get(PowerCycleDevices.Router);
		if (!powerCycleDevice.Configured)
		{
			AddError("Router relay is not configured.");
			return;
		}
		ErrorCodes errorCodes = powerCycleDevice.CyclePower();
		LogHelper.Instance.Log("[ResetRouterJob] CyclePower returned {0}", errorCodes);
		ServiceLocator.Instance.GetService<IHardwareCorrectionStatisticService>().Insert(HardwareCorrectionStatistic.RouterRecycle, base.Context, errorCodes == ErrorCodes.Success);
		if (errorCodes != ErrorCodes.Success)
		{
			AddError("Power cycle failed.");
			ServiceLocator.Instance.GetService<IControlSystemService>().Restart();
		}
	}

	internal ResetRouterJob(ExecutionResult result, ExecutionContext ctx)
		: base(result, ctx)
	{
	}
}
