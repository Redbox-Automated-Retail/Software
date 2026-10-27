using System;
using System.IO;
using System.Threading.Tasks;
using DeviceService.ComponentModel;
using DeviceService.ComponentModel.Bluefin;
using DeviceService.ComponentModel.Requests;
using DeviceService.ComponentModel.Responses;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace DeviceService.Domain;

public class ActivationService : IActivationService
{
	private ILogger<ActivationService> _logger;

	private IBluefinServiceClient _bluefinService;

	private IApplicationSettings _settings;

	private IIUC285Proxy _deviceProxy;

	public ActivationService(IApplicationSettings settings, ILogger<ActivationService> logger, IBluefinServiceClient bluefinService, IIUC285Proxy deviceProxy)
	{
		_logger = logger;
		_bluefinService = bluefinService;
		_settings = settings;
		_deviceProxy = deviceProxy;
	}

	public async Task<bool> CheckAndActivate(IBluefinActivationRequest request)
	{
		UnitDataModel unitData = _deviceProxy.UnitData;
		return await CheckAndActivate(request, unitData.ManufacturingSerialNumber, unitData.UnitSerialNumber);
	}

	public async Task<bool> CheckAndActivate(IBluefinActivationRequest request, string mfgSerialNumber, string injectedSerialNumber)
	{
		bool isSuccess = true;
		try
		{
			_logger.Log(LogLevel.Information, $"ActivationService.CheckAndActivate called with kioskId {request.KioskId}, mfgSerialNumber {mfgSerialNumber}, injectedSerialNumber {injectedSerialNumber}");
			DeviceActivationData activationData = GetActivationData();
			if (activationData == null)
			{
				_logger.Log(LogLevel.Information, "No previous activation info was found.");
			}
			else
			{
				_logger.Log(LogLevel.Information, $"Previous activation info was found with kiosk Id {activationData.KioskId} serial number {activationData.MfgSerialNumber}.");
			}
			if (activationData == null || activationData.KioskId != request.KioskId || activationData.MfgSerialNumber != mfgSerialNumber)
			{
				_logger.Log(LogLevel.Information, "Calling BluefinService.Activate");
				StandardResponse standardResponse = await _bluefinService.Activate(request, mfgSerialNumber, injectedSerialNumber, DateTime.Now);
				if (standardResponse.Success)
				{
					_logger.Log(LogLevel.Information, "Activation Successful.");
					UpdateActivationData(request.KioskId, mfgSerialNumber);
				}
				else
				{
					LogErrors($"Activation Not Successful. HttpErrorCode: {standardResponse.StatusCode}", standardResponse);
					isSuccess = false;
				}
			}
		}
		catch (Exception exception)
		{
			_logger.LogError(exception, "Unable to activate device due to unhandled exception:");
			isSuccess = false;
		}
		if (!isSuccess)
		{
			FailedRetry(request, mfgSerialNumber, injectedSerialNumber);
		}
		return isSuccess;
	}

	private void FailedRetry(IBluefinActivationRequest request, string mfgSerialNumber, string injectedSerialNumber)
	{
		if (!_settings.ActivationFailureRetry)
		{
			return;
		}
		Task.Run(async delegate
		{
			_logger.LogInformation("ActivationService.FailedRetry - waiting 5 minutes");
			await Task.Delay(300000);
			_logger.LogInformation("ActivationService.FailedRetry - starting");
			if (await CheckAndActivate(request, mfgSerialNumber, injectedSerialNumber))
			{
				_logger.LogInformation("ActivationService.FailedRetry - worked");
			}
			else
			{
				_logger.LogInformation("ActivationService.FailedRetry - failed again");
			}
		});
	}

	private DeviceActivationData GetActivationData()
	{
		string path = Path.Combine(_settings.DataFilePath, "BluefinActivationData.json");
		DeviceActivationData result = null;
		if (File.Exists(path))
		{
			using StreamReader reader = File.OpenText(path);
			result = (DeviceActivationData)new JsonSerializer().Deserialize(reader, typeof(DeviceActivationData));
		}
		return result;
	}

	private void UpdateActivationData(long kioskId, string mfgSerialNumber)
	{
		try
		{
			if (!Directory.Exists(_settings.DataFilePath))
			{
				Directory.CreateDirectory(_settings.DataFilePath);
			}
			DeviceActivationData value = new DeviceActivationData
			{
				KioskId = kioskId,
				MfgSerialNumber = mfgSerialNumber
			};
			string path = Path.Combine(_settings.DataFilePath, "BluefinActivationData.json");
			string contents = JsonConvert.SerializeObject(value);
			File.WriteAllText(path, contents);
		}
		catch (Exception exception)
		{
			_logger.LogError(exception, "Unable to write previous activation data:");
		}
	}

	private void LogErrors(string message, StandardResponse response)
	{
		if ((response?.Errors?.Count).GetValueOrDefault() == 0)
		{
			return;
		}
		_logger.Log(LogLevel.Error, message);
		foreach (Error error in response.Errors)
		{
			_logger.Log(LogLevel.Error, "Code: " + error.Code + " Message: " + error.Message);
		}
	}
}
