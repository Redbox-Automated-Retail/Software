using System;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.DataMatrix.Framework;

internal sealed class NilScanner : IScannerDevice, IDisposable
{
	public bool RequiresExternalLighting => false;

	public bool IsConnected => false;

	public bool InError => true;

	public bool IsLegacy => true;

	public bool SupportsSecureReads => false;

	public ScannerServices Service => ScannerServices.Emulated;

	public CameraGeneration CurrentCameraGeneration => CameraGeneration.Unknown;

	public void Dispose()
	{
	}

	public bool TestConfiguration(IMessageSink sink)
	{
		return false;
	}

	public bool Start()
	{
		return false;
	}

	public bool Start(bool unused)
	{
		return false;
	}

	public bool Stop()
	{
		return false;
	}

	public bool Restart()
	{
		return false;
	}

	public IScanResult Scan()
	{
		return new NilScanResult("UNCONFIGURED DEVICE");
	}

	public string FindInstalledPort()
	{
		return "NONE";
	}

	public ISnapResult Snap()
	{
		return new SnapResult(null);
	}

	public ISnapResult Snap(string fileName)
	{
		return new SnapResult(fileName);
	}

	public bool ValidateSettings()
	{
		return ValidateSettings(null);
	}

	public bool ValidateSettings(IMessageSink sink)
	{
		return false;
	}

	internal NilScanner()
	{
	}
}
