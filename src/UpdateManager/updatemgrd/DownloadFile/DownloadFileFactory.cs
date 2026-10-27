using System;
using System.Collections.Generic;
using Redbox.Core;
using Redbox.UpdateManager.ComponentModel;
using Redbox.UpdateManager.Environment;
using Redbox.UpdateService.Model;

namespace Redbox.UpdateManager.DownloadFile;

internal class DownloadFileFactory
{
	private IDataStoreService _dataStoreService;

	private IStatusMessageService _statusMessageService;

	private object _lock = new object();

	public const string DownloadFileExtension = ".downloaddat";

	public const string DownloadFileLabel = ".download";

	public static DownloadFileFactory Instance => Singleton<DownloadFileFactory>.Instance;

	private IDataStoreService DataStoreService
	{
		get
		{
			if (_dataStoreService == null)
			{
				_dataStoreService = ServiceLocator.Instance.GetService<IDataStoreService>();
			}
			return _dataStoreService;
		}
	}

	private IStatusMessageService StatusMessageService
	{
		get
		{
			if (_statusMessageService == null)
			{
				_statusMessageService = ServiceLocator.Instance.GetService<IStatusMessageService>();
			}
			return _statusMessageService;
		}
	}

	internal List<IDownloadFile> GetDownloadFiles()
	{
		lock (_lock)
		{
			List<IDownloadFile> list = new List<IDownloadFile>();
			IDataStoreService service = ServiceLocator.Instance.GetService<IDataStoreService>();
			foreach (Guid downLoad in GetDownLoadList())
			{
				try
				{
					DownloadFileData downloadFileData = service.Get<DownloadFileData>(string.Concat(downLoad, ".downloaddat"));
					if (downloadFileData == null)
					{
						DeleteDownloadFile(downLoad);
						continue;
					}
					IDownloadFile downloadFileFromData = GetDownloadFileFromData(downloadFileData);
					if (downloadFileData != null)
					{
						list.Add(downloadFileFromData);
					}
				}
				catch (Exception e)
				{
					LogHelper.Instance.Log($"An unhandled exception occurred in DownloadFileFactory.GetDownloadFiles for Guid: {downLoad}", e);
				}
			}
			return list;
		}
	}

	internal IDownloadFileData ChangeSetToDownloadFile(DownloadFileChangeSetData changeSet)
	{
		DownloadFileDataType downloadFileDataType = changeSet.DownloadFileType switch
		{
			DownloadFileType.Bits => DownloadFileDataType.Bits, 
			DownloadFileType.WUA => DownloadFileDataType.WUA, 
			DownloadFileType.Script => DownloadFileDataType.Script, 
			_ => DownloadFileDataType.Script, 
		};
		return new DownloadFileData
		{
			FileGuid = Guid.NewGuid(),
			ActivateScript = changeSet.ActivateScript,
			DestinationPath = changeSet.DestinationPath,
			DownloadFileDataState = DownloadFileDataState.None,
			DownloadFileDataType = downloadFileDataType,
			EndTime = changeSet.EndTime,
			FileKey = changeSet.FileKey,
			FileName = changeSet.FileName,
			Id = changeSet.Id,
			Name = changeSet.Name,
			StartTime = changeSet.StartTime,
			StatusKey = changeSet.StatusKey,
			Url = changeSet.Url,
			RetryCount = 0
		};
	}

	internal IDownloadFile GetDownloadFileFromChangeSet(DownloadFileChangeSetData changeSet)
	{
		return GetDownloadFileFromData(ChangeSetToDownloadFile(changeSet));
	}

	internal IDownloadFile GetDownloadFileFromData(IDownloadFileData downloadFileData)
	{
		return downloadFileData.DownloadFileDataType switch
		{
			DownloadFileDataType.Bits => new BitsDownloadFile(downloadFileData), 
			DownloadFileDataType.Script => new ScriptDownloadFile(downloadFileData), 
			_ => null, 
		};
	}

	internal void SaveDownloadFile(IDownloadFileData downloadFileData)
	{
		lock (_lock)
		{
			DataStoreService.Set(string.Concat(downloadFileData.FileGuid, ".downloaddat"), downloadFileData);
			List<Guid> downLoadList = GetDownLoadList();
			if (!downLoadList.Contains(downloadFileData.FileGuid))
			{
				downLoadList.Add(downloadFileData.FileGuid);
				DataStoreService.Set(".download", downLoadList);
			}
		}
	}

	internal void DeleteDownloadFile(IDownloadFileData downloadFileData)
	{
		lock (_lock)
		{
			DataStoreService.Delete(string.Concat(downloadFileData.FileGuid, ".downloaddat"));
			List<Guid> downLoadList = GetDownLoadList();
			if (downLoadList.Contains(downloadFileData.FileGuid))
			{
				downLoadList.Remove(downloadFileData.FileGuid);
				if (downLoadList.Count > 0)
				{
					DataStoreService.Set(".download", downLoadList);
					return;
				}
				DataStoreService.CleanUp(".downloaddat");
				DataStoreService.Delete(".download");
			}
		}
	}

	internal void DeleteDownloadFile(Guid guid)
	{
		lock (_lock)
		{
			DataStoreService.Delete(string.Concat(guid, ".downloaddat"));
			List<Guid> downLoadList = GetDownLoadList();
			if (downLoadList.Contains(guid))
			{
				downLoadList.Remove(guid);
				if (downLoadList.Count > 0)
				{
					DataStoreService.Set(".download", downLoadList);
					return;
				}
				DataStoreService.CleanUp(".downloaddat");
				DataStoreService.Delete(".download");
			}
		}
	}

	internal List<Guid> GetDownLoadList()
	{
		List<Guid> list = ServiceLocator.Instance.GetService<IDataStoreService>().Get<List<Guid>>(".download");
		if (list == null)
		{
			list = new List<Guid>();
		}
		return list;
	}

	internal ErrorList SendStatus(StatusMessage.StatusMessageType statusMessageType, string statusKey, string description, object data)
	{
		ErrorList errorList = new ErrorList();
		try
		{
			errorList.AddRange(StatusMessageService.EnqueueMessage(statusMessageType, statusKey, description, data.ToJson()));
		}
		catch (Exception e)
		{
			errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("DFS999", "An unhandled exception occurred in DownloadFileService.SendStatus.", e));
		}
		errorList.ToLogHelper();
		return errorList;
	}

	private DownloadFileFactory()
	{
	}
}
