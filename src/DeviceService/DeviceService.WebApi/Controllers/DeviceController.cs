using System;
using System.Threading;
using System.Threading.Tasks;
using DeviceService.ComponentModel;
using DeviceService.ComponentModel.Requests;
using DeviceService.ComponentModel.Responses;
using DeviceService.Domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace DeviceService.WebApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class DeviceController : ControllerBase
{
	private readonly IIUC285Proxy _proxy;

	private readonly IActivationService _activationService;

	private readonly IDeviceStatusService _deviceStatusService;

	private readonly IApplicationControl _applicationControl;

	private readonly ILogger<DeviceController> _logger;

	public DeviceController(IIUC285Proxy proxy, IActivationService activationService, IDeviceStatusService deviceStatusService, IApplicationControl applicationControl, ILogger<DeviceController> logger)
	{
		_proxy = proxy;
		_activationService = activationService;
		_deviceStatusService = deviceStatusService;
		_applicationControl = applicationControl;
		_logger = logger;
	}

	[HttpGet("IsConnected")]
	public ActionResult<bool> IsConnected()
	{
		return _proxy.IsConnected;
	}

	[HttpGet("Reconnect")]
	public ActionResult<bool> Reconnect()
	{
		return _proxy.Reconnect();
	}

	[HttpGet("PingDevice")]
	public ActionResult<bool> PingDevice()
	{
		return _proxy.PingDevice();
	}

	[HttpPost("ReadCard")]
	public async Task<Base87CardReadModel> ReadCard(CardReadRequest request, CancellationToken token)
	{
		Base87CardReadModel result = null;
		await _proxy.ReadCard(request, token, delegate(Base87CardReadModel r)
		{
			result = r;
		}, delegate(string evt, string cardSource)
		{
			_logger.LogInformation(evt + " - " + cardSource);
		});
		return result;
	}

	[HttpGet("UnitHealth")]
	public ActionResult<UnitHealthModel> UnitHealth()
	{
		return _proxy.GetUnitHealth();
	}

	[HttpGet("UnitData")]
	public ActionResult<UnitDataModel> UnitData()
	{
		return _proxy.UnitData;
	}

	[HttpPost("WriteFile")]
	public async Task<bool> WriteFile(string fullPath, bool rebootAfterWrite = false)
	{
		return await _proxy.WriteFile(fullPath, rebootAfterWrite);
	}

	[HttpGet("ReadConfig/{group}/{index}")]
	public ActionResult<string> ReadConfiguration(string group, string index)
	{
		string Data;
		return _proxy.ReadConfig(group, index, out Data) ? Data : null;
	}

	[HttpGet("WriteConfig")]
	public ActionResult<bool> WriteConfiguration(string group, string index, string data)
	{
		return _proxy.WriteConfig(group, index, data);
	}

	[HttpGet("Reboot")]
	public ActionResult<bool> Reboot()
	{
		return _proxy.Reboot();
	}

	[HttpGet("ValidateVersion")]
	public ActionResult<bool> ValidateVersion(int majorVersion, int minorVersion)
	{
		return DeviceService.Domain.DeviceService.IsClientVersionCompatible(new Version(majorVersion, minorVersion));
	}

	[HttpGet("Variable/{value}")]
	public ActionResult<string> GetVariable(int value)
	{
		return _proxy.GetVariable_29(value.ToString());
	}

	[HttpGet("CardInserted")]
	public ActionResult<InsertedStatus> CardInserted()
	{
		return _proxy.CheckIfCardInserted();
	}

	[HttpPost("CheckActivation")]
	public async Task<ActionResult<bool>> CheckActivation(BluefinActivationRequest request)
	{
		return await _activationService.CheckAndActivate(request);
	}

	[HttpGet("DeviceStatus")]
	[ProducesResponseType(typeof(DeviceStatus), 200)]
	[ProducesResponseType(typeof(NoContentResult), 204)]
	public async Task<ActionResult<DeviceStatus>> GetDeviceStatus()
	{
		return await _deviceStatusService.GetDeviceStatus();
	}

	[HttpPost("DeviceStatus")]
	public async Task<StandardResponse> PostDeviceStatus(DeviceStatusRequest request)
	{
		return await _deviceStatusService.PostDeviceStatus(request);
	}

	[HttpPost("Shutdown")]
	public ActionResult<bool> Shutdown(bool forceShutdown, ShutDownReason shutDownReason)
	{
		return _applicationControl?.ShutDown(forceShutdown, shutDownReason) ?? false;
	}

	[HttpPost("CanShutdown")]
	public ActionResult<bool> CanShutdown(ShutDownReason shutDownReason)
	{
		return _applicationControl?.CanShutDown(shutDownReason) ?? false;
	}

	[HttpGet("CardReaderState")]
	[ProducesResponseType(typeof(CardReaderState), 200)]
	public ActionResult<CardReaderState> GetCardReaderState()
	{
		return _proxy.GetCardReaderState();
	}
}
