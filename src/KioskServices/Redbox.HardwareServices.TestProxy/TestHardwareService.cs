// RECONSTRUCTED STUB - not decompiled.
// Redbox.HardwareServices.TestProxy.dll (1.88.0.80) is referenced by Redbox.HardwareServices.KernelExtension
// but is not present in the kiosk image. This project only recreates the API surface that KernelExtension
// uses (taken from its IL member references) so the solution compiles. It has no behaviour.
using System.ComponentModel;
using Redbox.HardwareServices.Proxy.ComponentModel;

namespace Redbox.HardwareServices.TestProxy;

public class TestHardwareService
{
	private static readonly TestHardwareService _instance = new TestHardwareService();

	public static TestHardwareService Instance => _instance;

	public int CameraReadStatsBarcodesRead { get; set; }

	public int CameraReadStatsReadAttemps { get; set; }

	public int CameraReadStatsSecureBarcodesRead { get; set; }

	public int CurrentCameraGeneration { get; set; }

	public int BarcodeDecoder { get; set; }

	public BindingList<IHardwareCorrectionStatistic> HardwareCorrectionStatistics { get; } = new BindingList<IHardwareCorrectionStatistic>();

	public bool IsRouterResetEnabled { get; set; }

	public bool RouterResetSuccessful { get; set; }

	public bool TouchScreenResetSuccessful { get; set; }

	public bool ArcusResetOnReturn { get; set; }

	public bool ArcusResetOnVend { get; set; }

	public bool ArcusResetSuccessful { get; set; }

	public Common.ReturnType ReturnType { get; set; }

	public Common.PreposResultType VendResult { get; set; }
}
