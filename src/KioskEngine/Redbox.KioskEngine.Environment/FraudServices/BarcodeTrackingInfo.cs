using System;
using Redbox.KioskEngine.ComponentModel.FraudServices;

namespace Redbox.KioskEngine.Environment.FraudServices;

public class BarcodeTrackingInfo : IBarcodeTrackingInfo
{
	public string KioskId { get; set; }

	public string Barcode { get; set; }

	public int BarcodesRead { get; set; }

	public int SecureBarcodesRead { get; set; }

	public DateTime ScanDateTime { get; set; }

	public CameraGeneration CameraType { get; set; }

	public BarcodeServices DecoderUsed { get; set; }

	public string KioskClientSoftwareVersion { get; set; }

	public string HALSoftwareVersion { get; set; }

	public bool MarkedAsFraud { get; set; }

	public int ScanAttempt { get; set; }

	public string SourceName { get; set; }
}
