using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Script.Framework.Natives;

[NativeJob(ProgramName = "change-camera-configuration")]
internal sealed class ChangeCameraConfigurationJob : NativeJobAdapter
{
	protected override void ExecuteInner()
	{
		ScannerServices scannerServices = base.Context.PopTop<ScannerServices>();
		ServiceLocator.Instance.GetService<IConfigurationService>().SetPropertyByName("CAMERA", "ScannerService", new object[1] { scannerServices });
		if (ServiceLocator.Instance.GetService<IScannerDeviceService>().GetConfiguredDevice().Service != scannerServices)
		{
			AddError("Failed to toggle service.");
		}
	}

	internal ChangeCameraConfigurationJob(ExecutionResult result, ExecutionContext ctx)
		: base(result, ctx)
	{
	}
}
