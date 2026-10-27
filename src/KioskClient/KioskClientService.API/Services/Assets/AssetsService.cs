using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using KioskClientService.API.App;
using KioskClientService.API.Services.Campaigns;
using KioskClientService.API.Services.Configuration;
using KioskClientService.API.Services.ProxyApi;
using Microsoft.Extensions.Logging;

namespace KioskClientService.API.Services.Assets;

internal class AssetsService : IAssetsService
{
	private const string AssetsFolder = "Assets";

	private readonly string _assetsFolderPath;

	private readonly ILogger<AssetsService> _logger;

	private readonly IStoreService _storeService;

	private readonly IAssetsConfiguration _configuration;

	private readonly IHttpService _httpService;

	public AssetsService(ILogger<AssetsService> logger, IStoreService storeService, IOptionsSnapshotKioskConfiguration kioskConfiguration, IHttpService httpService)
	{
		_logger = logger;
		_storeService = storeService;
		_configuration = kioskConfiguration.Assets;
		_httpService = httpService;
		_assetsFolderPath = Path.Combine(_storeService.DataPath, "Assets");
		if (!Directory.Exists(_assetsFolderPath))
		{
			_logger.LogInfoWithSource("Creating assets directory: " + _assetsFolderPath, ".ctor");
			Directory.CreateDirectory(_assetsFolderPath);
		}
	}

	public async Task<ApiBaseResponse> Refresh(IEnumerable<StartScreenDetails> startScreens)
	{
		_logger?.LogInfoWithSource("Started", "Refresh");
		try
		{
			if (_storeService.KioskId == 0L)
			{
				_logger?.LogErrorWithSource("KioskId is 0", "Refresh");
				return new ApiBaseResponse
				{
					StatusCode = HttpStatusCode.InternalServerError,
					Error = "KioskId is 0"
				};
			}
			await PerformAssetCleanup();
			await DownloadAssets(startScreens);
			return new ApiBaseResponse
			{
				StatusCode = HttpStatusCode.OK
			};
		}
		catch (Exception ex)
		{
			_logger.LogErrorWithSource(ex, "Exception refreshing assets", "Refresh");
			return new ApiBaseResponse
			{
				StatusCode = HttpStatusCode.InternalServerError,
				Error = "Exception refreshing assets"
			};
		}
	}

	private async Task PerformAssetCleanup()
	{
		DirectoryInfo directoryInfo = new DirectoryInfo(_assetsFolderPath);
		FileInfo[] files = directoryInfo.GetFiles();
		int num = files.Count();
		_logger.LogInfoWithSource($"Found {num} assets to check for cleanup", "PerformAssetCleanup");
		if (num > 0)
		{
			DateTime threshold = DateTime.Now.AddDays(-1 * _configuration.LifespanDays);
			List<Task<bool>> list = new List<Task<bool>>();
			FileInfo[] array = files;
			foreach (FileInfo fileInfo in array)
			{
				list.Add(CheckAndDeleteAsset(fileInfo.FullName, threshold));
			}
			list.ForEach(delegate(Task<bool> t)
			{
				t.Start();
			});
			bool[] source = await Task.WhenAll(list);
			_logger.LogInfoWithSource($"Cleaned up {source.Count((bool r) => r)} assets", "PerformAssetCleanup");
		}
	}

	private Task<bool> CheckAndDeleteAsset(string file, DateTime threshold)
	{
		return new Task<bool>(delegate
		{
			try
			{
				FileInfo fileInfo = new FileInfo(file);
				if (fileInfo.LastAccessTime < threshold)
				{
					_logger.LogInfoWithSource("Deleting expired asset: " + file, "CheckAndDeleteAsset");
					fileInfo.Delete();
					return true;
				}
				if (IsNotAValidAlreadyDownloadedAsset(file))
				{
					_logger.LogInfoWithSource("Deleting invalid asset: " + file, "CheckAndDeleteAsset");
					fileInfo.Delete();
					return true;
				}
			}
			catch (Exception ex)
			{
				_logger.LogErrorWithSource(ex, "Exception when checking/deleting asset " + file, "CheckAndDeleteAsset");
			}
			return false;
		});
	}

	private async Task DownloadAssets(IEnumerable<StartScreenDetails> startScreens)
	{
		IEnumerable<Asset> assets = startScreens.GetAssets();
		if (!assets.Any())
		{
			return;
		}
		_logger.LogInfoWithSource($"found {assets.Count()} possible assets for download", "DownloadAssets");
		List<Task<bool>> list = new List<Task<bool>>();
		foreach (Asset item in assets)
		{
			list.Add(CheckAndDownloadAsset(item.Url, item.FullPath));
		}
		list.ForEach(delegate(Task<bool> t)
		{
			t.Start();
		});
		bool[] source = await Task.WhenAll(list);
		_logger.LogInfoWithSource($"downloaded {source.Count((bool r) => r)} assets from cms", "DownloadAssets");
	}

	public bool IsNotAValidAlreadyDownloadedAsset(string file)
	{
		try
		{
			if (File.Exists(file))
			{
				using (Image.FromFile(file))
				{
					return false;
				}
			}
		}
		catch
		{
		}
		return true;
	}

	private Task<bool> CheckAndDownloadAsset(string url, string local)
	{
		return new Task<bool>(() => IsNotAValidAlreadyDownloadedAsset(local) && DownloadAsset(url, local).Result);
	}

	private async Task<bool> DownloadAsset(string url, string local)
	{
		_ = 1;
		bool result;
		try
		{
			if (string.IsNullOrEmpty(url) || string.IsNullOrEmpty(local))
			{
				_logger.LogErrorWithSource("Asset is invalid, skipping download", "DownloadAsset");
				result = false;
			}
			else
			{
				_logger.LogInfoWithSource("Downloading asset: " + local, "DownloadAsset");
				APIResponse<byte[]> response = await _httpService.DownloadFile(url);
				if (response.IsSuccessStatusCode)
				{
					await File.WriteAllBytesAsync(local, response.Response);
					_logger.LogInfoWithSource($"saved asset file: {local}, size: {response.Response.Length}", "DownloadAsset");
					result = true;
				}
				else
				{
					_logger.LogErrorWithSource($"Received status {response.StatusCode} downloading asset file: {local}", "DownloadAsset");
					result = false;
				}
			}
		}
		catch (Exception ex)
		{
			_logger.LogErrorWithSource(ex, "Error downloading asset: " + local, "DownloadAsset");
			result = false;
		}
		return result;
	}

	public void BuildLocalPaths(IEnumerable<Asset> assets)
	{
		foreach (Asset asset in assets)
		{
			if (Uri.TryCreate(asset.Url, UriKind.Absolute, out Uri result))
			{
				asset.FullPath = Path.Combine(_assetsFolderPath, Path.GetFileName(result.LocalPath));
			}
			else
			{
				_logger.LogErrorWithSource($"Invalid asset url: '{asset.Url}' for asset id: {asset.Id}!", "BuildLocalPaths");
			}
		}
	}
}
