using System;

namespace Redbox.KioskEngine.ComponentModel.FraudServices;

public interface IBarcodeTrackingInfo
{
	string KioskId { get; set; }

	string Barcode { get; set; }

	int BarcodesRead { get; set; }

	int SecureBarcodesRead { get; set; }

	DateTime ScanDateTime { get; set; }

	CameraGeneration CameraType { get; set; }

	BarcodeServices DecoderUsed { get; set; }

	string KioskClientSoftwareVersion { get; set; }

	string HALSoftwareVersion { get; set; }

	bool MarkedAsFraud { get; set; }

	int ScanAttempt { get; set; }

	string SourceName { get; set; }
}
