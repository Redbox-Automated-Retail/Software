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
		IHardwareState hardwareState = ServiceLocator.Instance.GetService<IHardwareState>();
		LogHelper.Instance.Log($"Air Exchanger Configured: {hardwareState.ExchangerStatus}");
		if (forcedAirExchangerConfigured)
		{
			LogHelper.Instance.Log("Forcing Air Exchanger to be Configured.");
		}
		if (forcedAirExchangerConfigured || hardwareState.ExchangerStatus == AirExchangerStatus.Ok)
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
		IHardwareState hardwareState = ServiceLocator.Instance.GetService<IHardwareState>();
		IBarcodeTrackingService barcodeTrackingService = ServiceLocator.Instance.GetService<IBarcodeTrackingService>();
		BarcodeServices decoderUsed = (Enum.TryParse<BarcodeServices>(hardwareState?.BarcodeDecoder.ToString(), out var result) ? result : BarcodeServices.None);
		CameraGeneration cameraType = (Enum.TryParse<CameraGeneration>(hardwareState?.CurrentCameraGeneration.ToString(), out var parsedCameraGeneration) ? parsedCameraGeneration : CameraGeneration.Unknown);
		string kioskId = ServiceLocator.Instance.GetService<IDataService>()?.GetStore().StoreId.ToString() ?? null;
		IMacroService macroService = ServiceLocator.Instance.GetService<IMacroService>();
		string kioskClientSoftwareVersion = macroService["EngineVersion"] ?? "Not Found";
		string hALSoftwareVersion = macroService["HALVersion"] ?? "Not Found";
		foreach (IHardwareResult item in results.Where((IHardwareResult x) => x.Code == "CameraStatsJson"))
		{
			IDictionary<string, object> messageData = item.MessageData;
			string itemID = item.ItemID;
			int scanAttempt = int.Parse((messageData?.FirstOrDefault((KeyValuePair<string, object> x) => x.Key == "ReadAttempts").Value ?? ((object)0)).ToString());
			int barcodesRead = int.Parse((messageData?.FirstOrDefault((KeyValuePair<string, object> x) => x.Key == "BarcodesRead").Value ?? ((object)0)).ToString());
			int secureBarcodesRead = int.Parse((messageData?.FirstOrDefault((KeyValuePair<string, object> x) => x.Key == "SecureCodesRead").Value ?? ((object)0)).ToString());
			DateTime scanDateTime = DateTime.MinValue;
			DateTime.TryParse(item.Timestamp, out scanDateTime);
			barcodeTrackingService.AddBarcodeTrackingInfoToQueue(new BarcodeTrackingInfo
			{
				KioskId = kioskId,
				Barcode = itemID,
				BarcodesRead = barcodesRead,
				SecureBarcodesRead = secureBarcodesRead,
				ScanDateTime = scanDateTime,
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
		IHardwareService hardwareService = ServiceLocator.Instance.GetService<IHardwareService>();
		ReadOnlyCollection<IHardwareResult> hardwareResults = null;
		hardwareService?.GetProgramResults(jobId, out hardwareResults);
		return hardwareResults;
	}
}
