using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using Redbox.Core;
using Redbox.HardwareServices.Proxy.ComponentModel;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Rental.Model;
using Redbox.Rental.Model.Health;

namespace Redbox.HardwareServices.Proxy;

public class HardwareAlertManagerService : IHardwareAlertManagerService
{
	private class JobResultSummary
	{
		public int UndefinedCount { get; set; }

		public int UnknownCount { get; set; }

		public int EmptyCount { get; set; }

		public int SuccessCount { get; set; }

		public int ExcludedCount { get; set; }

		public int TotalCount { get; set; }

		public override string ToString()
		{
			return $"JobResultSummary:  TotalCount:{TotalCount}, SuccessCount:{SuccessCount}, ExcludedCount:{ExcludedCount}, EmptyCount:{EmptyCount}, UnknownCount:{UnknownCount}, UndefinedCount:{UndefinedCount}";
		}
	}

	private static Func<object, bool> IsNotNull = (object value) => value != null;

	private static Func<object, bool> IsValidString = (object value) => value != null && value is string;

	private static Func<object, bool> IsNullOrValidString = (object value) => value == null || value is string;

	public static HardwareAlertManagerService Instance => Singleton<HardwareAlertManagerService>.Instance;

	public ErrorList Initialize()
	{
		LogHelper.Instance.Log("Initialize HardwareAlertManagerService");
		ErrorList result = new ErrorList();
		if (ServiceLocator.Instance.GetService<IHardwareAlertManagerService>() == null)
		{
			ServiceLocator.Instance.AddService(typeof(IHardwareAlertManagerService), Instance);
		}
		return result;
	}

	public string FormatJobResult(IHardwareResult jobResult)
	{
		Validate(jobResult, "Jobresult", IsNotNull);
		Validate(jobResult?.Code, "Jobresult code", IsNotNull);
		return $"<p>Code: {jobResult.Code}, Deck: {jobResult.Deck}, Slot: {jobResult.Slot}, Item ID: {jobResult.ItemID}, Message: {jobResult.Message}</p>";
	}

	public string FormatErrors(string jobId)
	{
		StringBuilder stringBuilder = new StringBuilder();
		Validate(jobId, "Jobid", IsValidString);
		IHardwareService service = ServiceLocator.Instance.GetService<IHardwareService>();
		if (service != null)
		{
			ErrorList errors = new ErrorList();
			if (service.GetJobErrors(jobId, out errors).Count > 0)
			{
				stringBuilder.Append("<p>The job ended in an Errored state; errors follow:</p>");
				foreach (Redbox.KioskEngine.ComponentModel.Error item in errors)
				{
					stringBuilder.Append(FormatError(item));
				}
			}
		}
		return stringBuilder.ToString();
	}

	public void SendJobAlert(string alertType, string source, string description, string jobId, string subType)
	{
		string jobResults = null;
		string jobErrors = null;
		if (jobId != null)
		{
			description = "HAL Job " + jobId + " <br>" + description;
			jobResults = FormatJobResults(jobId);
			jobErrors = FormatErrors(jobId);
		}
		SendAlert(alertType, source, description, jobResults, jobErrors, subType);
	}

	public void SendAlert(string alertType, string source, string description, string jobResults, string jobErrors, string subType)
	{
		Validate(alertType, "Class", IsValidString);
		Validate(source, "Source", IsValidString);
		Validate(description, "Description", IsValidString);
		Validate(jobResults, "Jobresults", IsNullOrValidString);
		Validate(jobErrors, "Errors", IsNullOrValidString);
		Validate(subType, "Sub-Type", IsNullOrValidString);
		string text = "<h2>" + source + "</h2><p>" + description + "</p>" + jobResults + jobErrors;
		LogHelper.Instance.Log("AlertManager.send_alert: " + text);
		string storeNumber = (ServiceLocator.Instance.GetService<IStoreManager>()?.KioskId ?? 0).ToString();
		ServiceLocator.Instance.GetService<IHealthServices>()?.SendAlert(storeNumber, alertType, subType, text, DateTime.Now, null);
	}

	public void SendAlert2(string typeOfSync, string jobId)
	{
		LogHelper.Instance.Log("HardwareAlertManagerService.set_alert2:Start");
		Validate(typeOfSync, "Type of Sync", IsValidString);
		Validate(jobId, "Job Id", IsValidString);
		IHardwareService service = ServiceLocator.Instance.GetService<IHardwareService>();
		if (service == null)
		{
			return;
		}
		service.GetProgramResults(jobId, out var hardwareResults);
		bool flag = hardwareResults?.All((IHardwareResult x) => x.Code == "InventoryState" || x.Code == "ItemReturned" || x.Code == "ExcludedSlotSkipped" || x.Code == "BadIdRead" || x.Code == "ServiceNotConfigured" || x.Code == "NoFraudMarkersDetected" || x.Code == "FraudMarkersRead" || x.Code == "FraudScanError" || x.Code == "Read-ID" || x.Code == "PeeledImageFile" || x.Code == "ReadTime" || x.Code == "DeviceStatus" || x.Code == "NumBarcodes" || x.Code == "ReadAttempts" || x.Code == "CameraStatsJson") ?? false;
		LogHelper.Instance.Log(FormatJobResults(jobId));
		ErrorList errors = new ErrorList();
		ErrorList jobErrors = service.GetJobErrors(jobId, out errors);
		LogHelper.Instance.Log(FormatErrors(jobId));
		StringBuilder stringBuilder = new StringBuilder("<h2>" + typeOfSync);
		if ((jobErrors != null && jobErrors.Count != 0) || !flag)
		{
			LogHelper.Instance.Log("Errored");
			stringBuilder.Append(" Errored</h2>");
		}
		else
		{
			stringBuilder.Append(" Successful</h2>");
		}
		stringBuilder.Append("<br> HAL Job: " + jobId + "<br>");
		stringBuilder.Append("<br> Results:<br>");
		LogHelper.Instance.Log($"Result Count: {hardwareResults?.Count ?? 0}");
		LogHelper.Instance.Log($"Error Count: {jobErrors?.Count ?? 0}");
		if (hardwareResults != null && hardwareResults.Count != 0)
		{
			JobResultSummary jobResultSummary = GetJobResultSummary(hardwareResults);
			stringBuilder.Append("<table border='0'>");
			stringBuilder.Append($"<tr><td>Undefined slots AFTER sync</td><td>:</td><td>{jobResultSummary.UndefinedCount}</td></tr>");
			stringBuilder.Append($"<tr><td>Unknown slots AFTER sync</td><td>:</td><td>{jobResultSummary.UnknownCount}</td></tr>");
			stringBuilder.Append($"<tr><td>Excluded slots</td><td>:</td><td>{jobResultSummary.ExcludedCount}</td></tr>");
			stringBuilder.Append($"<tr><td>Empty slots AFTER sync</td><td>:</td><td>{jobResultSummary.EmptyCount}</td></tr>");
			stringBuilder.Append($"<tr><td>Slots Successfully Synced</td><td>:</td><td>{jobResultSummary.SuccessCount}</td></tr>");
			stringBuilder.Append($"<tr><td><b>Total slots covered in Sync</b></td><td><b>:</b></td><td><b>{jobResultSummary.TotalCount}</b></td></tr>");
			stringBuilder.Append("</table>");
			stringBuilder.Append("<br>Extended Results of " + typeOfSync + "<br>Starting Inventory Sync");
			int onlyOneServiceNotConfigured = 0;
			foreach (IHardwareResult item in hardwareResults)
			{
				stringBuilder.Append(FormatSyncJobResult(item, ref onlyOneServiceNotConfigured));
			}
			stringBuilder.Append("<br>The " + typeOfSync + " Completed.");
		}
		if (jobErrors != null)
		{
			stringBuilder.Append(FormatErrors(jobId));
		}
		string storeNumber = (ServiceLocator.Instance.GetService<IStoreManager>()?.KioskId ?? 0).ToString();
		ServiceLocator.Instance.GetService<IHealthServices>()?.SendAlert(storeNumber, "TitleEmpty", typeOfSync, stringBuilder.ToString(), DateTime.Now, null);
		LogHelper.Instance.Log($"AlertManager.send_alert2: {stringBuilder}");
		LogHelper.Instance.Log("AlertManager.send_alert2:Finish");
	}

	private string FormatError(Redbox.KioskEngine.ComponentModel.Error error)
	{
		return "<p>" + error?.Code + " " + error?.Description + "<br>Details: " + error?.Details + "</p>";
	}

	private string FormatJobResults(string jobId)
	{
		StringBuilder stringBuilder = new StringBuilder();
		IHardwareService service = ServiceLocator.Instance.GetService<IHardwareService>();
		if (service != null)
		{
			service.GetProgramResults(jobId, out var hardwareResults);
			foreach (IHardwareResult item in hardwareResults)
			{
				stringBuilder.Append(FormatJobResult(item));
			}
		}
		return stringBuilder.ToString();
	}

	private string FormatSyncJobResult(IHardwareResult jobResult, ref int onlyOneServiceNotConfigured)
	{
		StringBuilder stringBuilder = new StringBuilder();
		Validate(jobResult, "Jobresult", IsNotNull);
		Validate(jobResult?.Code, "Jobresult code", IsNotNull);
		stringBuilder.Append($"<p>D: {jobResult?.Deck} S: {jobResult?.Slot}");
		string jobResultItem = GetJobResultItem(jobResult);
		if (!string.IsNullOrEmpty(jobResultItem))
		{
			switch (jobResultItem)
			{
			case "UNDEFINED":
				break;
			case "UNKNOWN":
				goto IL_00fe;
			case "EMPTY":
				goto IL_01d2;
			default:
				goto IL_01e0;
			}
		}
		stringBuilder.Append(" UNDEFINED");
		stringBuilder.Append("<b> | Error - " + jobResult?.Code + " - " + jobResult.Message + "</b>");
		goto IL_0252;
		IL_01d2:
		stringBuilder.Append(" EMPTY");
		goto IL_0252;
		IL_0252:
		stringBuilder.Append("</p>");
		return stringBuilder.ToString();
		IL_01e0:
		stringBuilder.Append(" Barcode:" + jobResultItem);
		if (jobResult?.Code == "ItemReturned")
		{
			stringBuilder.Append("<b> | " + jobResult?.Code + " - " + jobResult?.Message + "</b>");
		}
		goto IL_0252;
		IL_00fe:
		stringBuilder.Append(" UNKNOWN");
		if ((jobResult?.Code != "InventoryState" && jobResult.Code != "ItemReturned") || (jobResult?.Code == "ServiceNotConfigured" && onlyOneServiceNotConfigured < 1))
		{
			stringBuilder.Append("<b> | Error - " + jobResult?.Code + " - " + jobResult?.Message + "</b>");
			if (jobResult?.Code == "ServiceNotConfigured")
			{
				onlyOneServiceNotConfigured++;
			}
		}
		goto IL_0252;
	}

	private JobResultSummary GetJobResultSummary(ReadOnlyCollection<IHardwareResult> hardwareResults)
	{
		JobResultSummary jobResultSummary = new JobResultSummary();
		if (hardwareResults != null)
		{
			foreach (IHardwareResult hardwareResult in hardwareResults)
			{
				string jobResultItem = GetJobResultItem(hardwareResult);
				if (jobResultItem != null)
				{
					LogHelper.Instance.Log("Found Item: " + jobResultItem + " with code: " + hardwareResult?.Code);
				}
				if (hardwareResult.Code == "ExcludedSlotSkipped")
				{
					jobResultSummary.ExcludedCount++;
				}
				else
				{
					if (!(hardwareResult.Code == "InventoryState") && !(hardwareResult.Code == "ItemReturned"))
					{
						continue;
					}
					switch (jobResultItem)
					{
					case null:
					case "":
					case "UNDEFINED":
						jobResultSummary.UndefinedCount++;
						break;
					default:
						if (!(jobResultItem?.ToUpper() == "REDBOX"))
						{
							if (jobResultItem == "EMPTY")
							{
								jobResultSummary.EmptyCount++;
							}
							else
							{
								jobResultSummary.SuccessCount++;
							}
							break;
						}
						goto case "UNKNOWN";
					case "UNKNOWN":
						jobResultSummary.UnknownCount++;
						break;
					}
					jobResultSummary.TotalCount++;
				}
			}
		}
		LogHelper.Instance.Log(jobResultSummary.ToString());
		return jobResultSummary;
	}

	private string GetJobResultItem(IHardwareResult jobResult)
	{
		string text = jobResult?.ItemID;
		if (text == null)
		{
			int num = jobResult?.MetaData.IndexOf(',') ?? (-1);
			if (num == 0)
			{
				text = ((jobResult == null || jobResult.MetaData.Length != 1) ? jobResult?.MetaData.Substring(1) : "UNKNOWN");
			}
			else if (num < 0)
			{
				text = jobResult?.MetaData;
			}
			else if (num > 0)
			{
				text = jobResult?.MetaData.Substring(0, num);
			}
		}
		return text;
	}

	private static void Validate(object value, string name, Func<object, bool> validator)
	{
		if (validator != null && !validator(value))
		{
			LogHelper.Instance.Log($"AlertManager: invalid parameter value: {name} = {value} ");
		}
	}

	private HardwareAlertManagerService()
	{
	}
}
