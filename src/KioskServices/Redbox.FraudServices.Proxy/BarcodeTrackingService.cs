using Redbox.Core;
using Redbox.FraudServices.Proxy.ComponentModel;
using Redbox.KioskEngine.ComponentModel.FraudServices;

namespace Redbox.FraudServices.Proxy;

public class BarcodeTrackingService : IBarcodeTrackingService
{
	public static BarcodeTrackingService Instance => Singleton<BarcodeTrackingService>.Instance;

	public void AddBarcodeTrackingInfoToQueue(IBarcodeTrackingInfo barcodeTrackingInfo)
	{
		LogHelper.Instance.Log("logging barcode tracking for: " + barcodeTrackingInfo.ToJson());
	}

	private BarcodeTrackingService()
	{
	}
}
