using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using KioskClientService.API.App;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace KioskClientService.API.Services.Products.MoreLikeThis;

public class MoreLikeThisData : IMoreLikeThisData
{
	private readonly IStoreService _store;

	private readonly ILogger<MoreLikeThisData> _logger;

	private readonly string _dataDirectory;

	private readonly string _moreLikeThisDataFilePath;

	private object _lockObj;

	private DateTime _fileContentExpireTime = DateTime.Today.AddDays(-1.0);

	public DateTime FileContentExpireTime => _fileContentExpireTime;

	public MoreLikeThisData(IStoreService store, ILogger<MoreLikeThisData> logger)
	{
		_store = store;
		_logger = logger;
		_dataDirectory = Path.GetFullPath(Path.Combine(_store.DataPath, "Data"));
		_moreLikeThisDataFilePath = _dataDirectory + "\\MoreLikeThisData.json";
		_lockObj = new object();
	}

	public MoreLikeThisDataFileContent GetFileContent()
	{
		MoreLikeThisDataFileContent moreLikeThisDataFileContent = new MoreLikeThisDataFileContent
		{
			ExpireTime = _fileContentExpireTime,
			Items = new ConcurrentDictionary<long, List<long>>()
		};
		try
		{
			if (!File.Exists(_moreLikeThisDataFilePath))
			{
				Directory.CreateDirectory(_dataDirectory);
			}
			else
			{
				MoreLikeThisDataFileContent moreLikeThisDataFileContent2 = JsonConvert.DeserializeObject<MoreLikeThisDataFileContent>(File.ReadAllText(_moreLikeThisDataFilePath));
				moreLikeThisDataFileContent.ExpireTime = (_fileContentExpireTime = moreLikeThisDataFileContent2.ExpireTime);
				moreLikeThisDataFileContent.Items = moreLikeThisDataFileContent2.Items;
			}
		}
		catch (Exception ex)
		{
			_logger.LogErrorWithSource(ex, "Error occured while getting MoreLikeThisData file content", "GetFileContent");
		}
		return moreLikeThisDataFileContent;
	}

	public void RefreshAll(ConcurrentDictionary<long, List<long>> moreLikeThisTitleMapping, DateTime nextFullRefreshDate)
	{
		MoreLikeThisDataFileContent fileModel = new MoreLikeThisDataFileContent
		{
			ExpireTime = nextFullRefreshDate,
			Items = moreLikeThisTitleMapping
		};
		RefreshFile(fileModel);
	}

	public void RefreshAllWithCurrentExpireTime(ConcurrentDictionary<long, List<long>> moreLikeThisTitleMapping)
	{
		MoreLikeThisDataFileContent fileModel = new MoreLikeThisDataFileContent
		{
			ExpireTime = FileContentExpireTime,
			Items = moreLikeThisTitleMapping
		};
		RefreshFile(fileModel);
	}

	private void RefreshFile(MoreLikeThisDataFileContent fileModel)
	{
		try
		{
			lock (_lockObj)
			{
				if (File.Exists(_moreLikeThisDataFilePath))
				{
					_logger.LogInfoWithSource("Deleting MoreLikeThisData file", "RefreshFile");
					File.Delete(_moreLikeThisDataFilePath);
					_logger.LogInfoWithSource("Deleted MoreLikeThisData file successfully", "RefreshFile");
				}
				_logger.LogInfoWithSource("Writing refreshed data into MoreLikeThisData file", "RefreshFile");
				File.WriteAllText(_moreLikeThisDataFilePath, fileModel.ToJsonIndented());
				_logger.LogInfoWithSource("Successfully wrote refreshed data into MoreLikeThisData file.", "RefreshFile");
				_fileContentExpireTime = fileModel.ExpireTime;
			}
		}
		catch (Exception ex)
		{
			_logger.LogErrorWithSource(ex, "Exception occured while refreshing MoreLikeThisData file", "RefreshFile");
		}
	}
}
