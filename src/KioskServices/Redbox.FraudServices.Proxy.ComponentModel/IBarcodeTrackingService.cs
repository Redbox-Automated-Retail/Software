using Redbox.KioskEngine.ComponentModel.FraudServices;

namespace Redbox.FraudServices.Proxy.ComponentModel;

public interface IBarcodeTrackingService
{
	void AddBarcodeTrackingInfoToQueue(IBarcodeTrackingInfo barcodeTrackingInfo);
}
