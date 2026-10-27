using System;
using System.Threading.Tasks;
using DeviceService.ComponentModel;
using DeviceService.ComponentModel.KDS;
using DeviceService.ComponentModel.Requests;
using DeviceService.ComponentModel.Responses;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace DeviceService.Domain;

public class DeviceStatusService : IDeviceStatusService
{
	private readonly IIUC285Proxy _proxy;

	private readonly IKioskDataServiceClient _client;

	private readonly IApplicationSettings _settings;

	private readonly ILogger<DeviceStatusService> _logger;

	public DeviceStatusService(IIUC285Proxy proxy, IApplicationSettings settings, IKioskDataServiceClient client, ILogger<DeviceStatusService> logger)
	{
		_proxy = proxy;
		_client = client;
		_settings = settings;
		_logger = logger;
	}

	public async Task<StandardResponse> PostDeviceStatus()
	{
		KioskData fileData = DataFileHelper.GetFileData<KioskData>(_settings.DataFilePath, "KioskData.json", LogException);
		StandardResponse standardResponse;
		if (fileData != null)
		{
			DeviceStatus deviceStatus = await _proxy.CheckDeviceStatus(fileData.KioskId);
			standardResponse = await _client.PostDeviceStatus(deviceStatus);
		}
		else
		{
			standardResponse = new StandardResponse(new Exception("KioskData.json not found in Data folder"));
			_logger.LogInformation("PostDeviceStatus Result: " + JsonConvert.SerializeObject(standardResponse));
		}
		return standardResponse;
	}

	public async Task<StandardResponse> PostDeviceStatus(DeviceStatusRequest request)
	{
		KioskData data = new KioskData
		{
			ApiUrl = request.ApiUrl,
			ApiKey = request.ApiKey,
			KioskId = request.KioskId
		};
		DataFileHelper.TryUpdateFileData(_settings.DataFilePath, "KioskData.json", data);
		DeviceStatus deviceStatus = await _proxy.CheckDeviceStatus(request.KioskId);
		return await _client.PostDeviceStatus(deviceStatus);
	}

	public async Task<DeviceStatus> GetDeviceStatus()
	{
		KioskData fileData = DataFileHelper.GetFileData<KioskData>(_settings.DataFilePath, "KioskData.json", LogException);
		if (fileData != null)
		{
			return await _proxy.CheckDeviceStatus(fileData.KioskId);
		}
		return null;
	}

	private void LogException(string message, Exception e)
	{
		_logger?.LogError(e, "Unhandled Exception in " + message);
	}
}
