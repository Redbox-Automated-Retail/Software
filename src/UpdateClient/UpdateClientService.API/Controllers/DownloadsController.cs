using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using UpdateClientService.API.Services.DownloadService;
using UpdateClientService.API.Services.DownloadService.Responses;
using UpdateClientService.API.Services.IoT.DownloadFiles;

namespace UpdateClientService.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class DownloadsController : ControllerBase
{
	private readonly IDownloadService _downloadService;

	private readonly IDownloadFilesService _downloadFilesService;

	private readonly ILogger<DownloadsController> _logger;

	private readonly IMemoryCache _cache;

	public DownloadsController(IDownloadService downloadService, IDownloadFilesService downloadFilesService, ILogger<DownloadsController> logger, IMemoryCache cache)
	{
		_downloadService = downloadService;
		_downloadFilesService = downloadFilesService;
		_logger = logger;
		_cache = cache;
	}

	[HttpHead("s3/proxy/{key}/{type}")]
	[ProducesResponseType(typeof(RedirectResult), 301)]
	public async Task<ActionResult> ProxyForS3Head(string key, DownloadPathType type)
	{
		key = Uri.UnescapeDataString(key);
		if (!_cache.TryGetValue<string>($"{key}/{type}/head", out var value))
		{
			value = await _downloadService.GetProxiedS3Url(key, type, isHead: true);
			if (!string.IsNullOrWhiteSpace(value) && Uri.TryCreate(value, UriKind.Absolute, out Uri _))
			{
				_cache.Set($"{key}/{type}/head", value, TimeSpan.FromMinutes(30.0));
			}
		}
		return new RedirectResult(value, permanent: true);
	}

	[HttpGet("s3/proxy/{key}/{type}")]
	[ProducesResponseType(typeof(RedirectResult), 301)]
	public async Task<RedirectResult> ProxyForS3Get(string key, DownloadPathType type)
	{
		key = Uri.UnescapeDataString(key);
		if (!_cache.TryGetValue<string>($"{key}/{type}/get", out var value))
		{
			value = await _downloadService.GetProxiedS3Url(key, type, isHead: true);
			if (!string.IsNullOrWhiteSpace(value) && Uri.TryCreate(value, UriKind.Absolute, out Uri _))
			{
				_cache.Set($"{key}/{type}/get", value, TimeSpan.FromMinutes(30.0));
			}
		}
		return new RedirectResult(value, permanent: true);
	}

	[HttpPost]
	[ProducesResponseType(typeof(GetFileResponse), 202)]
	[ProducesResponseType(typeof(GetFileResponse), 500)]
	public async Task<ActionResult<GetFileResponse>> StartDownload(Uri url, string hash = null)
	{
		return (await _downloadService.AddDownload(hash, url.ToString(), DownloadPriority.Normal)).ToObjectResult();
	}

	[HttpGet]
	[ProducesResponseType(typeof(GetDownloadStatusesResponse), 200)]
	public async Task<ActionResult> GetDownloadStatuses()
	{
		return (await _downloadService.GetDownloadsResponse()).ToObjectResult();
	}

	[HttpGet("{key}")]
	[ProducesResponseType(typeof(GetDownloadStatusResponse), 200)]
	[ProducesResponseType(typeof(GetDownloadStatusResponse), 404)]
	public async Task<ActionResult> GetDownloadStatus(string key)
	{
		return (await _downloadService.GetDownloadResponse(key)).ToObjectResult();
	}

	[HttpDelete("{key}")]
	[ProducesResponseType(typeof(DeleteDownloadResponse), 200)]
	[ProducesResponseType(typeof(DeleteDownloadResponse), 500)]
	public async Task<ActionResult> CancelDownload(string key)
	{
		return (await _downloadService.CancelDownload(key)).ToObjectResult();
	}

	[HttpPost("{key}")]
	[ProducesResponseType(typeof(CompleteDownloadResponse), 200)]
	[ProducesResponseType(typeof(CompleteDownloadResponse), 500)]
	public async Task<ActionResult<CompleteDownloadResponse>> CompleteDownload(string key)
	{
		return (await _downloadService.CompleteDownload(key)).ToObjectResult();
	}
}
