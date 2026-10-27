using System;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Script.Framework;

internal struct FraudValidator : IDisposable
{
	private readonly ExecutionContext Context;

	public void Dispose()
	{
	}

	internal FraudValidationResult Validate(ScanResult scanResult)
	{
		if (scanResult != null && scanResult.SnapOk && scanResult.ReadCode)
		{
			scanResult.CopyToFraudFolder();
		}
		if (!ServiceLocator.Instance.GetService<IFraudService>().IsConfigured)
		{
			Context.CreateInfoResult("ServiceNotConfigured", "The service is not configured for fraud scan.");
			return FraudValidationResult.NotConfigured;
		}
		string scannedMatrix = scanResult.ScannedMatrix;
		if (!scanResult.SnapOk)
		{
			string text = ErrorCodes.CameraCapture.ToString().ToUpper();
			Context.CreateInfoResult("FraudScanError", $"Scan returned error code {text}");
			Context.ContextLog.WriteFormatted("[DiskValidatorHelper] The scan returned error code {0}", text);
			return FraudValidationResult.DeviceError;
		}
		if (!scanResult.ReadCode)
		{
			Context.CreateNoBarcodeReadResult();
			return FraudValidationResult.None;
		}
		if (scanResult.SecureTokensFound > 0)
		{
			Context.AppLog.WriteFormatted("Security markers detected on barcode {0}.", scannedMatrix);
			Context.CreateResult("FraudMarkersRead", "A security marker was scanned with the barcode.", scannedMatrix);
			return FraudValidationResult.Peeled;
		}
		Context.AppLog.WriteFormatted("The disk {0} had no security markers.", scannedMatrix);
		Context.CreateResult("NoFraudMarkersDetected", "No security markers were detected.", scannedMatrix);
		return FraudValidationResult.Photocopy;
	}

	internal FraudValidator(ExecutionContext context)
	{
		this = default(FraudValidator);
		Context = context;
	}
}
