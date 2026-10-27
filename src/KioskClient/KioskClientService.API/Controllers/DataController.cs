using System.Threading.Tasks;
using KioskClientService.API.Models;
using KioskClientService.API.Services.Data;
using Microsoft.AspNetCore.Mvc;

namespace KioskClientService.API.Controllers;

[Route("api/data")]
[ApiController]
public class DataController : ControllerBase
{
	private readonly IDataService _service;

	public DataController(IDataService service)
	{
		_service = service;
	}

	[Route("kioskversions")]
	[HttpPost]
	[ProducesResponseType(typeof(DataModels.KioskDataResponse), 200)]
	[ProducesResponseType(typeof(DataModels.KioskDataResponse), 500)]
	public async Task<ActionResult<DataModels.KioskDataResponse>> Versions(DataModels.KioskVersionRequest request)
	{
		return await _service.SendVersions(base.ControllerContext.HttpContext.Request, request);
	}

	[Route("kioskstatistics")]
	[HttpPost]
	[ProducesResponseType(typeof(DataModels.KioskDataResponse), 200)]
	[ProducesResponseType(typeof(DataModels.KioskDataResponse), 500)]
	public async Task<ActionResult<DataModels.KioskDataResponse>> Statistics(DataModels.KioskStatisticsRequest request)
	{
		return await _service.SendStatistics(base.ControllerContext.HttpContext.Request, request);
	}

	[Route("kioskproperties")]
	[HttpPost]
	[ProducesResponseType(typeof(DataModels.KioskDataResponse), 200)]
	[ProducesResponseType(typeof(DataModels.KioskDataResponse), 500)]
	public async Task<ActionResult<DataModels.KioskDataResponse>> Properties(DataModels.KioskPropertiesRequest request)
	{
		return await _service.SendProperties(base.ControllerContext.HttpContext.Request, request);
	}

	[Route("kioskalert")]
	[HttpPost]
	[ProducesResponseType(typeof(DataModels.KioskDataResponse), 200)]
	[ProducesResponseType(typeof(DataModels.KioskDataResponse), 500)]
	public async Task<ActionResult<DataModels.KioskDataResponse>> KioskAlert(DataModels.KioskAlertRequest request)
	{
		return await _service.SendKioskAlert(base.ControllerContext.HttpContext.Request, request);
	}

	[Route("auditevent")]
	[HttpPost]
	[ProducesResponseType(typeof(DataModels.KioskDataResponse), 200)]
	[ProducesResponseType(typeof(DataModels.KioskDataResponse), 500)]
	public async Task<ActionResult<DataModels.KioskDataResponse>> AuditEvent(DataModels.AuditEventRequest request)
	{
		return await _service.SendAuditEvent(base.ControllerContext.HttpContext.Request, request);
	}

	[Route("kioskevents")]
	[HttpPost]
	[ProducesResponseType(typeof(DataModels.KioskDataResponse), 200)]
	[ProducesResponseType(typeof(DataModels.KioskDataResponse), 500)]
	public async Task<ActionResult<DataModels.KioskDataResponse>> KioskEvents(DataModels.KioskEventsRequest request)
	{
		return await _service.SendKioskEvents(base.ControllerContext.HttpContext.Request, request);
	}

	[Route("kioskfunctioncheck")]
	[HttpPost]
	[ProducesResponseType(typeof(DataModels.KioskDataResponse), 200)]
	[ProducesResponseType(typeof(DataModels.KioskDataResponse), 500)]
	public async Task<ActionResult<DataModels.KioskDataResponse>> KioskFunctionCheck(DataModels.KioskFunctionCheckRequest request)
	{
		return (await _service.SendKioskFunctionCheck(base.ControllerContext.HttpContext.Request, request)).ToObjectResult();
	}
}
