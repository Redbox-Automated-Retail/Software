using System;
using Redbox.HAL.Component.Model;
using Redbox.KioskEngine.ComponentModel;

namespace Redbox.HardwareServices.Proxy;

public class KioskFunctionCheckDataConverter : Redbox.KioskEngine.ComponentModel.IKioskFunctionCheckData
{
	private Redbox.HAL.Component.Model.IKioskFunctionCheckData _kfcData;

	public string CameraDriverTestResult => _kfcData.CameraDriverTestResult;

	public string InitTestResult => _kfcData.InitTestResult;

	public string SnapDecodeTestResult => _kfcData.SnapDecodeTestResult;

	public DateTime Timestamp => _kfcData.Timestamp;

	public string TouchscreenDriverTestResult => _kfcData.TouchscreenDriverTestResult;

	public string TrackTestResult => _kfcData.TrackTestResult;

	public string UserIdentifier => _kfcData.UserIdentifier;

	public string VendDoorTestResult => _kfcData.VendDoorTestResult;

	public string VerticalSlotTestResult => _kfcData.VerticalSlotTestResult;

	public KioskFunctionCheckDataConverter(Redbox.HAL.Component.Model.IKioskFunctionCheckData kfcData)
	{
		_kfcData = kfcData;
	}
}
