using System;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Script.Framework;

[NativeJob(ProgramName = "reset-touchscreen-controller")]
internal sealed class ResetTouchscreenControllerJob : NativeJobAdapter
{
	protected override void ExecuteInner()
	{
		ApplicationLog applicationLog = ApplicationLog.ConfigureLog(base.Context, logAppends: true, "Service", flushOnWrite: false, null);
		IUsbDeviceService service = ServiceLocator.Instance.GetService<IUsbDeviceService>();
		applicationLog.WriteFormatted("Request to reset touchscreen controller");
		ITouchscreenDescriptor touchscreenDescriptor = service.FindTouchScreen(matchDriver: false);
		bool flag = false;
		if (touchscreenDescriptor != null)
		{
			try
			{
				flag = touchscreenDescriptor.HardReset();
			}
			catch (Exception e)
			{
				LogHelper.Instance.Log("Reset touchscreen caught an exception", e);
				flag = false;
			}
			applicationLog.WriteFormatted(" Touchscreen found; reset returned {0}", flag);
		}
		else
		{
			applicationLog.WriteFormatted("Unable to locate a touchscreen.");
		}
		ServiceLocator.Instance.GetService<IHardwareCorrectionStatisticService>().Insert(HardwareCorrectionStatistic.Touchscreen, base.Context, flag);
		if (!flag)
		{
			AddError("Touchscreen rest failure.");
		}
	}

	internal ResetTouchscreenControllerJob(ExecutionResult result, ExecutionContext ctx)
		: base(result, ctx)
	{
	}
}
