using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Redbox.Core;
using Redbox.FraudServices.Proxy.ComponentModel;
using Redbox.HardwareServices.Proxy.ComponentModel;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.ComponentModel.FraudServices;
using Redbox.KioskEngine.ComponentModel.HardwareServices;
using Redbox.KioskEngine.Environment.FraudServices;
using Redbox.Rental.Model.DataService;

namespace Redbox.Rental.Services;

public class HardwareHelper
{
	public static void TurnOnAirExchangerFanIfConfigured(bool forcedAirExchangerConfigured, string currentBundleName)
	{
		IHardwareService service = ServiceLocator.Instance.GetService<IHardwareService>();
		IHardwareState service2 = ServiceLocator.Instance.GetService<IHardwareState>();
		LogHelper.Instance.Log($"Air Exchanger Configured: {service2.ExchangerStatus}");
		if (forcedAirExchangerConfigured)
		{
			LogHelper.Instance.Log("Forcing Air Exchanger to be Configured.");
		}
		if (forcedAirExchangerConfigured || service2.ExchangerStatus == AirExchangerStatus.Ok)
		{
			ErrorList errorList = service?.ExecuteImmediate("AIRXCHGR FANON", null, clearStack: false, currentBundleName);
			if (errorList != null && errorList.Count > 0)
			{
				LogHelper.Instance.Log("ERROR: Unable to connect to HAL service: ");
				errorList.ToLogHelper();
			}
			else
			{
				LogHelper.Instance.Log("Air Exchanger has been turned ON");
			}
		}
	}

	public static void ProcessResults(string jobId, string jobName)
	{
		ReadOnlyCollection<IHardwareResult> results = GetResults(jobId);
		ProcessResults(jobName, results);
	}

	private static void ProcessResults(string jobName, ReadOnlyCollection<IHardwareResult> results)
	{
		if (results == null || !results.Any())
		{
			LogHelper.Instance.Log("No Results to process.");
			return;
		}
		IHardwareState service = ServiceLocator.Instance.GetService<IHardwareState>();
		IBarcodeTrackingService service2 = ServiceLocator.Instance.GetService<IBarcodeTrackingService>();
		BarcodeServices decoderUsed = (Enum.TryParse<BarcodeServices>(service?.BarcodeDecoder.ToString(), out var result) ? result : BarcodeServices.None);
		CameraGeneration cameraType = (Enum.TryParse<CameraGeneration>(service?.CurrentCameraGeneration.ToString(), out var result2) ? result2 : CameraGeneration.Unknown);
		string kioskId = ServiceLocator.Instance.GetService<IDataService>()?.GetStore().StoreId.ToString() ?? null;
		IMacroService service3 = ServiceLocator.Instance.GetService<IMacroService>();
		string kioskClientSoftwareVersion = service3["EngineVersion"] ?? "Not Found";
		string hALSoftwareVersion = service3["HALVersion"] ?? "Not Found";
		foreach (IHardwareResult item in results.Where((IHardwareResult x) => x.Code == "CameraStatsJson"))
		{
			IDictionary<string, object> messageData = item.MessageData;
			string itemID = item.ItemID;
			int scanAttempt = int.Parse((messageData?.FirstOrDefault((KeyValuePair<string, object> x) => x.Key == "ReadAttempts").Value ?? ((object)0)).ToString());
			int barcodesRead = int.Parse((messageData?.FirstOrDefault((KeyValuePair<string, object> x) => x.Key == "BarcodesRead").Value ?? ((object)0)).ToString());
			int secureBarcodesRead = int.Parse((messageData?.FirstOrDefault((KeyValuePair<string, object> x) => x.Key == "SecureCodesRead").Value ?? ((object)0)).ToString());
			DateTime result3 = DateTime.MinValue;
			DateTime.TryParse(item.Timestamp, out result3);
			service2.AddBarcodeTrackingInfoToQueue(new BarcodeTrackingInfo
			{
				KioskId = kioskId,
				Barcode = itemID,
				BarcodesRead = barcodesRead,
				SecureBarcodesRead = secureBarcodesRead,
				ScanDateTime = result3,
				CameraType = cameraType,
				DecoderUsed = decoderUsed,
				KioskClientSoftwareVersion = kioskClientSoftwareVersion,
				HALSoftwareVersion = hALSoftwareVersion,
				MarkedAsFraud = false,
				ScanAttempt = scanAttempt,
				SourceName = (jobName ?? "none")
			});
		}
	}

	private static ReadOnlyCollection<IHardwareResult> GetResults(string jobId)
	{
		if (string.IsNullOrWhiteSpace(jobId))
		{
			LogHelper.Instance.Log("JobId wasn't provided.");
			return null;
		}
		IHardwareService service = ServiceLocator.Instance.GetService<IHardwareService>();
		ReadOnlyCollection<IHardwareResult> hardwareResults = null;
		service?.GetProgramResults(jobId, out hardwareResults);
		return hardwareResults;
	}
}
