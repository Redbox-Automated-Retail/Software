using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KioskClientService.API.App;
using KioskClientService.API.Services.HttpQueue;
using KioskClientService.API.Services.Inventory;
using KioskClientService.API.Services.Inventory.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace KioskClientService.API.Controllers;

[Route("api/inventory")]
[ApiController]
public class InventoryController : ControllerBase
{
	private readonly ILogger<InventoryController> _logger;

	private readonly IInventoryService _inventoryService;

	private readonly IHttpQueueService _httpQueueService;

	public InventoryController(ILogger<InventoryController> logger, IInventoryService inventoryService, IHttpQueueService httpQueueService)
	{
		_logger = logger;
		_inventoryService = inventoryService;
		_httpQueueService = httpQueueService;
	}

	[Route("MerchandizingOrders")]
	[HttpGet]
	public async Task<ActionResult<GetMerchandizingOrdersResponse>> GetCurrentMerchandizingOrders()
	{
		return new OkObjectResult(await _inventoryService.GetAllCurrentMerchandizingOrders());
	}

	[Route("UpdateMerchandizingStatus")]
	[HttpPost]
	public async Task<ActionResult<UpdateMerchandizingStatusResponse>> UpdateMerchandizingStatus([FromBody] List<MerchandStatusUpdate> updates)
	{
		_logger?.LogInfoWithSource($"UpdateMerchandizingStatus called for {updates?.Count ?? 0} barcodes.", "UpdateMerchandizingStatus");
		UpdateMerchandizingStatusResponse updateMerchandizingStatusResponse = await _inventoryService.UpdateMerchandizingStatus(updates);
		_logger.LogInfoWithSource($"response: {updateMerchandizingStatusResponse}", "UpdateMerchandizingStatus");
		if (updates.Any())
		{
			updateMerchandizingStatusResponse.CheckAndReplay(_logger, updates.First().MessageId.Value, base.ControllerContext.HttpContext.Request, _httpQueueService);
		}
		return updateMerchandizingStatusResponse.ToObjectResult();
	}

	[Route("sync")]
	[HttpPost]
	[ProducesResponseType(typeof(InventoryResponse), 200)]
	[ProducesResponseType(typeof(InventoryResponse), 401)]
	[ProducesResponseType(typeof(InventoryResponse), 500)]
	public async Task<ActionResult<InventoryResponse>> Sync([FromBody] InventorySnapshotRequest request)
	{
		_logger?.LogInfoWithSource($"Sync called for Kiosk: {request.KioskId}, number of discs: {request.Discs.Count}, message id: {request.MessageId}", "Sync");
		InventoryResponse inventoryResponse = await _inventoryService.Sync(request);
		inventoryResponse.MessageId = request.MessageId;
		return inventoryResponse.ToObjectResult();
	}

	[Route("discaction")]
	[HttpPost]
	[ProducesResponseType(typeof(InventoryResponse), 200)]
	[ProducesResponseType(typeof(InventoryResponse), 400)]
	[ProducesResponseType(typeof(InventoryResponse), 401)]
	[ProducesResponseType(typeof(InventoryResponse), 500)]
	public async Task<ActionResult<InventoryResponse>> DiscAction([FromBody] DiscActionRequest request)
	{
		_logger?.LogInfoWithSource($"DiscAction called for kiosk: {request.KioskId}, user: {request.UserName}, jobname: {request.JobName}, action date: {request.ActionDate}, message id: {request.MessageId}", "DiscAction");
		InventoryResponse inventoryResponse = await _inventoryService.DiscAction(request);
		inventoryResponse.MessageId = request.MessageId;
		inventoryResponse.CheckAndReplay(_logger, request.MessageId, base.ControllerContext.HttpContext.Request, _httpQueueService);
		return inventoryResponse.ToObjectResult();
	}

	[Route("discfraud")]
	[HttpPost]
	[ProducesResponseType(typeof(InventoryResponse), 200)]
	[ProducesResponseType(typeof(InventoryResponse), 400)]
	[ProducesResponseType(typeof(InventoryResponse), 401)]
	[ProducesResponseType(typeof(InventoryResponse), 500)]
	public async Task<ActionResult<InventoryResponse>> DiscFraud([FromBody] DiscFraudRequest request)
	{
		_logger?.LogInfoWithSource($"DiscFraud called for message id: {request.MessageId}, kiosk: {request.KioskId}, barcode: {request.Barcode}, transaction date: {request.TransactionDate}, properties: {request.Properties.ToJson()}", "DiscFraud");
		InventoryResponse inventoryResponse = await _inventoryService.DiscFraud(request);
		inventoryResponse.MessageId = request.MessageId;
		inventoryResponse.CheckAndReplay(_logger, request.MessageId, base.ControllerContext.HttpContext.Request, _httpQueueService);
		return inventoryResponse.ToObjectResult();
	}

	[Route("kiosktitlescount")]
	[HttpPost]
	[ProducesResponseType(typeof(KioskTitlesCountResponse), 200)]
	[ProducesResponseType(400)]
	[ProducesResponseType(401)]
	[ProducesResponseType(500)]
	public async Task<ActionResult<KioskTitlesCountResponse>> KioskTitlesCount([FromBody] KiosksAndTitlesPayload payload)
	{
		return (await _inventoryService.KioskTitlesCount(payload)).ToObjectResult();
	}
}
