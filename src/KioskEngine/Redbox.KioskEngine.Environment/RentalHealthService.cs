using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Redbox.Core;
using Redbox.HardwareServices.Proxy.ComponentModel;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Rental.Model.Health;

namespace Redbox.KioskEngine.Environment;

public class RentalHealthService : IRentalHealthService
{
	private const string _kfcJson = "KFC.json";

	private string FullKFCJsonPath => Path.Combine(ServiceLocator.Instance.GetService<IEngineApplication>().DataPath, "KFC.json");

	public void KioskFunctionCheck()
	{
		ITimerService timerService = ServiceLocator.Instance.GetService<ITimerService>();
		ServiceLocator.Instance.GetService<ILogger>();
		IMacroService service = ServiceLocator.Instance.GetService<IMacroService>();
		string storeNumber = service["StoreNumber"];
		string timerName = "KioskFunctionCheckTimer";
		timerService.CreateTimer(timerName, 1200000, null, delegate
		{
			timerService.RemoveTimer(timerName);
			Task.Run(delegate
			{
				try
				{
					IList<IKioskFunctionCheckData> hardwareKFC = GetHardwareKFC();
					List<RentalKioskFunctionCheckData> source = ReadKFCJson();
					DateTime lastUpdate = (source.Any() ? source.Max((RentalKioskFunctionCheckData x) => x.Timestamp) : DateTime.MinValue);
					IEnumerable<IKioskFunctionCheckData> enumerable = hardwareKFC?.Where((IKioskFunctionCheckData x) => x.Timestamp > lastUpdate);
					LogHelper.Instance.Log($"Found {enumerable?.Count() ?? 0} new Kiosk Function Check Entries.");
					enumerable?.ForEach(delegate(IKioskFunctionCheckData x)
					{
						SendKioskFunctionCheckData(storeNumber, x);
					});
					UpdateKFCJson(hardwareKFC);
				}
				catch (Exception e)
				{
					LogHelper.Instance.LogException("An error has occurred in RentalHealthService.KioskFunctionCheck", e);
				}
			});
		}).Start();
	}

	private IList<IKioskFunctionCheckData> GetHardwareKFC()
	{
		IHardwareService service = ServiceLocator.Instance.GetService<IHardwareService>();
		if (service == null)
		{
			LogHelper.Instance.LogError("S001", "Unable to locate Hardware Service.");
			return null;
		}
		IList<IKioskFunctionCheckData> data = null;
		LogHelper.Instance.Log("Getting Kiosk Function Check Data From HAL.", LogEntryType.Info);
		ErrorList kioskFunctionCheckData = service.GetKioskFunctionCheckData(out data);
		if (kioskFunctionCheckData != null && kioskFunctionCheckData.Any())
		{
			kioskFunctionCheckData.ForEach(delegate(Redbox.KioskEngine.ComponentModel.Error x)
			{
				LogHelper.Instance.LogError(x.Code, x.Description ?? (string.Empty + System.Environment.NewLine + x.Details));
			});
		}
		else if (data != null)
		{
			LogHelper.Instance.Log(data.ToJson());
		}
		else
		{
			LogHelper.Instance.Log("No data retrieved from HAL.");
		}
		LogHelper.Instance.Log("GetHardwareKFC Complete.");
		return data;
	}

	private List<RentalKioskFunctionCheckData> ReadKFCJson()
	{
		try
		{
			if (!File.Exists(FullKFCJsonPath))
			{
				LogHelper.Instance.Log(string.Format("Unable to read KFC json, File {0} doesn't exist.", "KFC.json"), LogEntryType.Info);
				return new List<RentalKioskFunctionCheckData>();
			}
			List<RentalKioskFunctionCheckData> list = File.ReadAllText(FullKFCJsonPath, Encoding.ASCII).ToObject<List<RentalKioskFunctionCheckData>>();
			LogHelper.Instance.Log($"Found {list?.Count() ?? 0} records in KFC.json file.", LogEntryType.Info);
			return list;
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log(string.Format("Unable to read KFC json, File {0}", "KFC.json"), e);
			try
			{
				string text = Path.ChangeExtension(FullKFCJsonPath, ".bad");
				if (File.Exists(text))
				{
					File.Delete(text);
				}
				File.Move(FullKFCJsonPath, text);
			}
			catch (Exception e2)
			{
				LogHelper.Instance.Log("Renaming bad kfc file failed", e2);
			}
			return new List<RentalKioskFunctionCheckData>();
		}
	}

	private void UpdateKFCJson(IList<IKioskFunctionCheckData> data)
	{
		if (data == null)
		{
			LogHelper.Instance.Log("No data available to update KFC json file.", LogEntryType.Info);
			return;
		}
		LogHelper.Instance.Log("Updating Kiosk Function Check Json File.", LogEntryType.Info);
		File.WriteAllBytes(FullKFCJsonPath, Encoding.ASCII.GetBytes(data.ToJson()));
	}

	public void SendKioskFunctionCheckData(string storeNumber, IKioskFunctionCheckData data)
	{
		IHealthServices service = ServiceLocator.Instance.GetService<IHealthServices>();
		LogHelper.Instance.Log("Sending Kiosk Function Check to Kiosk Service.", LogEntryType.Info);
		service.SendKioskFunctionCheck(storeNumber, data.UserIdentifier, data.Timestamp, data.VerticalSlotTestResult, data.InitTestResult, data.VendDoorTestResult, data.TrackTestResult, data.SnapDecodeTestResult, data.TouchscreenDriverTestResult, data.CameraDriverTestResult, null, delegate
		{
			LogHelper.Instance.Log("Send Kiosk Function Check Complete.", LogEntryType.Info);
		});
	}
}
