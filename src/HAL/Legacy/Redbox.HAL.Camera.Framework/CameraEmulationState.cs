using System.Drawing;
using Redbox.HAL.Core;

namespace Redbox.HAL.Camera.Framework;

public sealed class CameraEmulationState : EmulationStateConfig
{
	public static CameraEmulationState Instance => Singleton<CameraEmulationState>.Instance;

	public int ExpectedVideoFrameCount { get; set; }

	public string ExpectedStartCaptureResult { get; set; }

	public Bitmap ExepctedVideoGetFrameResult { get; set; }

	public string[] ExpectedVideoCompressorsResult { get; set; }

	public string[] ExpectedCameraDeviceNamesResult { get; set; }

	public string ExpectedStopCaptureResult { get; set; }

	public void Reset()
	{
		ExpectedVideoFrameCount = 1;
		ExpectedStartCaptureResult = "C:\\temp\\dummy.avi";
		ExepctedVideoGetFrameResult = new Bitmap(640, 480);
		ExpectedCameraDeviceNamesResult = new string[1] { "Emulated Camera" };
		ExpectedVideoCompressorsResult = new string[1] { "Emulated Video Compressor" };
		ExpectedStopCaptureResult = "CAPTURE ERROR";
	}

	protected override object GetEmulated(string name)
	{
		return null;
	}

	protected override bool ProcessOption(string op, object[] value)
	{
		return false;
	}

	private CameraEmulationState()
	{
		Reset();
	}
}
