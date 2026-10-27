using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Script.Framework.Natives;

[NativeJob(ProgramName = "test-and-reset-ccr")]
internal sealed class TestAndResetCCRJob : NativeJobAdapter
{
	private const string CCRCode = "CCRStatus";

	protected override void ExecuteInner()
	{
		ApplicationLog applicationLog = ApplicationLog.ConfigureLog(base.Context, logAppends: true, "Service", flushOnWrite: true, null);
		IUsbDeviceService service = ServiceLocator.Instance.GetService<IUsbDeviceService>();
		IQueryUsbDeviceResult queryUsbDeviceResult = service.FindCCR();
		if (!queryUsbDeviceResult.Found)
		{
			applicationLog.WriteFormatted("Unable to find CCR device.");
			base.Context.CreateInfoResult("CCRStatus", "Unable to locate CCR device");
			AddError("No CCR");
			return;
		}
		if (queryUsbDeviceResult.Running)
		{
			base.Context.CreateInfoResult("CCRStatus", "The CCR appears to be working.");
			return;
		}
		applicationLog.WriteFormatted("Found device {0}", queryUsbDeviceResult);
		if (queryUsbDeviceResult.IsNotStarted)
		{
			applicationLog.WriteFormatted(" Device not started; RESET returned {0}", queryUsbDeviceResult.Descriptor.ResetDriver());
		}
		else if (queryUsbDeviceResult.IsDisabled)
		{
			applicationLog.WriteFormatted(" Device not enabled; change to enable returned {0}", service.ChangeByHWID(queryUsbDeviceResult.Descriptor, DeviceState.Enable));
		}
		DeviceStatus deviceStatus = service.FindDeviceStatus(queryUsbDeviceResult.Descriptor);
		applicationLog.WriteFormatted(" Device status AFTER fix = {0}", deviceStatus);
		HardwareCorrectionEventArgs e = new HardwareCorrectionEventArgs(HardwareCorrectionStatistic.CreditCardReaderReset);
		if ((deviceStatus & DeviceStatus.Found) != DeviceStatus.None && (deviceStatus & DeviceStatus.Enabled) != DeviceStatus.None)
		{
			applicationLog.WriteFormatted("  ** Successfully reset the device **");
			base.Context.CreateInfoResult("CCRStatus", "Successfully reset the CCR");
			e.CorrectionOk = true;
		}
		else
		{
			applicationLog.WriteFormatted("  !!Failed to reset the device!!");
			base.Context.CreateInfoResult("CCRStatus", "Failed to reset the CCR.");
			AddError("Failed to restart CCR");
			e.CorrectionOk = false;
		}
		ServiceLocator.Instance.GetService<IHardwareCorrectionStatisticService>().Insert(e, base.Context);
	}

	internal TestAndResetCCRJob(ExecutionResult result, ExecutionContext ctx)
		: base(result, ctx)
	{
	}
}
