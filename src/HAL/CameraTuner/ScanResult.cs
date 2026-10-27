using System;
using System.IO;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.CameraTuner;

internal sealed class ScanResult
{
	private static string SecretToken;

	internal int ReadCount { get; private set; }

	internal string ScannedMatrix { get; private set; }

	internal TimeSpan ExecutionTime { get; private set; }

	internal bool SnapOk { get; private set; }

	internal int SecureCount { get; private set; }

	internal static ScanResult ErrorResult()
	{
		return new ScanResult();
	}

	internal static ScanResult Scan(string file)
	{
		if (string.IsNullOrEmpty(file) || !File.Exists(file))
		{
			return ErrorResult();
		}
		IScanResult scanResult = ServiceLocator.Instance.GetService<IBarcodeReaderFactory>().GetConfiguredReader().Scan(file);
		ScanResult scanResult2 = new ScanResult
		{
			SnapOk = true
		};
		if (scanResult.DecodeResults.Count > 2 || scanResult.DecodeResults.Count == 0)
		{
			LogHelper.Instance.Log("[ScanResult] There are {0} matrix entries.", scanResult.DecodeResults.Count);
			return scanResult2;
		}
		IDecodeResult decodeResult = null;
		foreach (IDecodeResult decodeResult2 in scanResult.DecodeResults)
		{
			if (!decodeResult2.Matrix.Equals(SecretToken, StringComparison.CurrentCultureIgnoreCase))
			{
				decodeResult = decodeResult2;
			}
			else
			{
				scanResult2.SecureCount = decodeResult2.Count;
			}
		}
		if (decodeResult != null)
		{
			scanResult2.ScannedMatrix = decodeResult.Matrix;
			scanResult2.ReadCount = ((decodeResult.Count > 4) ? 4 : decodeResult.Count);
			scanResult2.ExecutionTime = scanResult.ExecutionTime;
		}
		return scanResult2;
	}

	private ScanResult()
	{
		ReadCount = 0;
		ScannedMatrix = "UNKNOWN";
		ExecutionTime = default(TimeSpan);
		SecureCount = 0;
	}

	static ScanResult()
	{
		SecretToken = ServiceLocator.Instance.GetService<IEncryptionService>().DecryptFromBase64("2PiCFxFlrO8=");
	}
}
