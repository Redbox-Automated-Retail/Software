using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using DeviceService.ComponentModel;
using DeviceService.ComponentModel.FileUpdate;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace DeviceService.Domain.FileUpdate;

public class FileUpdateService : IFileUpdateService
{
	public class Constants
	{
		public const string ManifestFileName = "manifest.json";

		public const string UpdateFolderName = "Updates";

		public const string CompletedFolderName = "Completed";

		public const string NotProcessedFolderName = "Not Processed";

		public const string MissingFileUpdaterMessage = "Unable to process file updates.  Missing FileUpdater object.";

		public const string FileSuccessfullyUpdated = "File successfully updated.";

		public const string NoFilesToUpdateMessage = "No files found to update.";

		public const string NoActiveUpdatesMessage = "There are no active updates.";

		public const string UnableToShutDownMessage = "Unable to run file update(s) because CanShutDown returned false. ";

		public const string UnableToCreateCompletedFolderMessage = "Unable to create completed updates folder. ";

		public const string NearExpectedRebootTimeMessage = "Unable to run file update(s) because of pending device reboot.";

		public const string RevisionDowngradeMessage = "Current manifest file revision is greater than or equal to the incoming manifest file. Erroring update.";

		public const string UpdateOfRevisionNumberFailedMessage = "Unable to update revision number on device.";

		public const string MissingManifestRevisionMessage = "Manifest Revision is missing.";
	}

	private string _updateFolderPath;

	private string _completedFolderPath;

	private string _notProcessedFolderPath;

	private readonly ILogger<FileUpdateService> _logger;

	private Timer _timer;

	private static object _lock = new object();

	private IFileUpdater _fileUpdater;

	private IApplicationControl _applicationControl;

	public string UpdateFolderPath
	{
		get
		{
			if (string.IsNullOrEmpty(_updateFolderPath))
			{
				_updateFolderPath = Path.Combine(_baseFolder, "Updates");
			}
			return _updateFolderPath;
		}
		set
		{
			_updateFolderPath = value;
		}
	}

	public string CompletedFolderPath
	{
		get
		{
			if (string.IsNullOrEmpty(_completedFolderPath))
			{
				_completedFolderPath = Path.Combine(_baseFolder, "Updates", "Completed");
			}
			return _completedFolderPath;
		}
		set
		{
			_completedFolderPath = value;
		}
	}

	public string NotProcessedFolderPath
	{
		get
		{
			if (string.IsNullOrEmpty(_notProcessedFolderPath))
			{
				_notProcessedFolderPath = Path.Combine(_baseFolder, "Updates", "Not Processed");
			}
			return _notProcessedFolderPath;
		}
		set
		{
			_notProcessedFolderPath = value;
		}
	}

	public IFileUpdater FileUpdater
	{
		get
		{
			return _fileUpdater;
		}
		set
		{
			_fileUpdater = value;
		}
	}

	private string _baseFolder => Directory.GetParent(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)).FullName;

	public FileUpdateService(ILogger<FileUpdateService> logger, IFileUpdater fileUpdater, IApplicationControl applicationControl)
	{
		_logger = logger;
		_fileUpdater = fileUpdater;
		_applicationControl = applicationControl;
		StartTimer();
	}

	public IFileUpdateResults UpdateFiles()
	{
		lock (_lock)
		{
			IFileUpdateResults fileUpdateResults = new FileUpdateResults
			{
				Success = true
			};
			Log("Starting FileUpdateService.LoadUpdateFiles...");
			FindZipFileUpdates(fileUpdateResults);
			ReadManifestFiles(fileUpdateResults);
			ValidateFiles(fileUpdateResults);
			List<IZipFileUpdateResult> activeZipFileUpdateResults = GetActiveZipFileUpdateResults(fileUpdateResults);
			ValidateThereAreActiveUpdates(fileUpdateResults, activeZipFileUpdateResults);
			ValidateFileUpdater(fileUpdateResults);
			ValidateNotNearExpectedRebootTime(fileUpdateResults);
			ValidateCanShutDown(fileUpdateResults, activeZipFileUpdateResults);
			InitializeOutputFolders(fileUpdateResults);
			ProcessFileUpdates(fileUpdateResults, activeZipFileUpdateResults);
			Log(fileUpdateResults.ToString());
			Log("Ending FileUpdateService.LoadUpdateFiles");
			return fileUpdateResults;
		}
	}

	private void ValidateFileUpdater(IFileUpdateResults fileUpdateResults)
	{
		if (fileUpdateResults.Success && _fileUpdater == null)
		{
			fileUpdateResults.Success = false;
			fileUpdateResults.Status = FileUpdateHighLevelStatus.MissingFileUpdater;
			Log("Unable to process file updates.  Missing FileUpdater object.");
		}
	}

	private void ValidateCanShutDown(IFileUpdateResults fileUpdateResults, List<IZipFileUpdateResult> activeZipFileUpdateResults)
	{
		if (fileUpdateResults.Success)
		{
			Log("Checking CanShutDown before running file update...");
			if (!_applicationControl.CanShutDown(ShutDownReason.SoftwareUpdate))
			{
				fileUpdateResults.Success = false;
				fileUpdateResults.Status = FileUpdateHighLevelStatus.UnableToShutDown;
				Log("Unable to run file update(s) because CanShutDown returned false. ");
			}
		}
	}

	private void ValidateNotNearExpectedRebootTime(IFileUpdateResults fileUpdateResults)
	{
		if (fileUpdateResults.Success)
		{
			DateTime? dateTime = _fileUpdater?.GetExpectedRebootTime();
			if (dateTime.HasValue && DateIsWithinXMinutes(dateTime.Value, 30))
			{
				fileUpdateResults.Success = false;
				fileUpdateResults.Status = FileUpdateHighLevelStatus.NearExpectedRebootTime;
				Log(string.Format("{0} Expected Reboot Time: {1}", "Unable to run file update(s) because of pending device reboot.", dateTime));
			}
			else
			{
				Log($"No immminent device reboot.  Expected Reboot Time: {dateTime}");
			}
		}
	}

	private bool DateIsWithinXMinutes(DateTime dateTime, int minutes)
	{
		if (DateTime.Now <= dateTime)
		{
			return DateTime.Now.AddMinutes(minutes) >= dateTime;
		}
		return false;
	}

	private void ValidateThereAreActiveUpdates(IFileUpdateResults fileUpdateResults, List<IZipFileUpdateResult> activeZipFileUpdateResults)
	{
		if (fileUpdateResults.Success && (activeZipFileUpdateResults == null || activeZipFileUpdateResults.Count() == 0))
		{
			fileUpdateResults.Success = false;
			Log("There are no active updates.");
		}
	}

	private void ValidateFiles(IFileUpdateResults fileUpdateResults)
	{
		if (fileUpdateResults.Success)
		{
			foreach (IZipFileUpdateResult item in fileUpdateResults.ZipFileUpdateResultCollection)
			{
				if (!ZipFileContainsAllUpdateFiles(item))
				{
					item.Status = ZipFileUpdateStatus.Errored;
					item.StatusMessage = "File missing from zip file.";
				}
			}
		}
		UpdateFileUpdateResultStatus(fileUpdateResults);
	}

	private void UpdateFileUpdateResultStatus(IFileUpdateResults fileUpdateResults)
	{
		StringBuilder stringBuilder = new StringBuilder();
		switch (fileUpdateResults.Status)
		{
		case FileUpdateHighLevelStatus.Normal:
			if (fileUpdateResults.ZipFileUpdateResultCollection.Count == 0)
			{
				stringBuilder.Append("No files found to update.");
			}
			break;
		case FileUpdateHighLevelStatus.UnableToShutDown:
			stringBuilder.Append("Unable to run file update(s) because CanShutDown returned false. ");
			break;
		case FileUpdateHighLevelStatus.UnableToCreateCompletedFolder:
			stringBuilder.Append("Unable to create completed updates folder. ");
			break;
		case FileUpdateHighLevelStatus.MissingFileUpdater:
			stringBuilder.Append("Unable to process file updates.  Missing FileUpdater object.");
			break;
		case FileUpdateHighLevelStatus.NearExpectedRebootTime:
			stringBuilder.Append("Unable to run file update(s) because of pending device reboot.");
			break;
		}
		var enumerable = from x in fileUpdateResults.ZipFileUpdateResultCollection
			group x by x.Status into @group
			select new
			{
				Name = @group.Key,
				Count = @group.Count()
			};
		foreach (var item in enumerable)
		{
			stringBuilder.Append($"{item.Count} zip file(s) {item.Name}; ");
		}
		fileUpdateResults.Success = fileUpdateResults.Status == FileUpdateHighLevelStatus.Normal && !enumerable.Any(x => x.Name == ZipFileUpdateStatus.Errored && x.Count > 0);
		fileUpdateResults.StatusMessage = stringBuilder.ToString();
	}

	private List<IZipFileUpdateResult> GetActiveZipFileUpdateResults(IFileUpdateResults fileUpdateResults)
	{
		List<IZipFileUpdateResult> list = new List<IZipFileUpdateResult>();
		foreach (IZipFileUpdateResult item in fileUpdateResults.ZipFileUpdateResultCollection)
		{
			if (item.Status != ZipFileUpdateStatus.NotProcessed)
			{
				continue;
			}
			if (IsZipFileUpdatAfterStartDate(item))
			{
				if (IsZipFileInUpdateTimeRange(item))
				{
					list.Add(item);
					continue;
				}
				item.Status = ZipFileUpdateStatus.Skipped;
				item.UpdateFileManifestProperties.Status = ManifestPropertiesStatus.OutsideOfTimeRange;
				item.StatusMessage = "Skipped because current time is outside update time range";
				Log($"Skipping processing of zip file update {Path.GetFileName(item.FileName)} because current time is outside update time range {item.UpdateFileManifestProperties.UpdateRangeStart} - {item.UpdateFileManifestProperties.UpdateRangeEnd}");
			}
			else
			{
				item.Status = ZipFileUpdateStatus.Skipped;
				item.UpdateFileManifestProperties.Status = ManifestPropertiesStatus.StartDateIsInTheFuture;
				item.StatusMessage = "Skipped because start date is in the future";
				Log($"Skipping processing of zip file update {Path.GetFileName(item.FileName)} because start date {item.UpdateFileManifestProperties.StartDate} is in the future.");
			}
		}
		return list;
	}

	private void ProcessFileUpdates(IFileUpdateResults fileUpdateResults, List<IZipFileUpdateResult> activeZipFileUpdateResults)
	{
		if (fileUpdateResults.Success)
		{
			if (fileUpdateResults?.ZipFileUpdateResultCollection != null)
			{
				bool flag = false;
				Log("Preparing to update files");
				FileUpdater?.PrepareToUpdateFiles();
				try
				{
					foreach (IZipFileUpdateResult activeZipFileUpdateResult in activeZipFileUpdateResults)
					{
						if (!flag)
						{
							Log("Processing zip file updates in " + Path.GetFileName(activeZipFileUpdateResult.FileName));
							using (ZipArchive zipArchive = ZipFile.OpenRead(activeZipFileUpdateResult.FileName))
							{
								int num = activeZipFileUpdateResult.FileUpdateResultCollection.Max((IFileUpdateResult x) => x.UpdateFileModel.Order);
								foreach (IFileUpdateResult item in activeZipFileUpdateResult.FileUpdateResultCollection.OrderBy((IFileUpdateResult x) => x.UpdateFileModel.Order))
								{
									bool flag2 = item.UpdateFileModel.Order == num;
									string text = (flag2 ? "  - Forcing device reboot because this is the last file in the update." : null);
									Log("    Processing file: " + item.UpdateFileModel.Path + text);
									bool flag3 = false;
									ZipArchiveEntry zipArchiveEntry = zipArchive?.GetEntry(item.UpdateFileModel.Path);
									if (zipArchiveEntry != null)
									{
										try
										{
											Stream stream = zipArchiveEntry.Open();
											bool rebootRequired = flag2 || item.UpdateFileModel.RebootRequired;
											Task<bool> task = FileUpdater.WriteStream(stream, item.UpdateFileModel.Path, rebootRequired, waitForReconnect: true);
											task.Wait();
											flag3 = task.Result;
										}
										catch (Exception arg)
										{
											Log($"Exception while opening/reading stream.  {arg}");
										}
										Log($"    file update for {item.UpdateFileModel.Path} success = {flag3}");
										if (!flag3)
										{
											flag = true;
											item.FileUpdateStatus = FileUpdateStatus.Errored;
											activeZipFileUpdateResult.Status = ZipFileUpdateStatus.Errored;
											activeZipFileUpdateResult.StatusMessage = "Update of " + item.UpdateFileModel.Path + " failed.";
											break;
										}
										item.FileUpdateStatus = FileUpdateStatus.Updated;
										continue;
									}
									flag = true;
									item.FileUpdateStatus = FileUpdateStatus.Errored;
									activeZipFileUpdateResult.Status = ZipFileUpdateStatus.Errored;
									activeZipFileUpdateResult.StatusMessage = "    Unable to find zip archive entry " + item.UpdateFileModel.Path;
									break;
								}
							}
							if (!flag)
							{
								Log($"Updating IUC285 file update revision number to {(activeZipFileUpdateResult?.UpdateFileManifestProperties?.Revision).GetValueOrDefault()}");
								if (activeZipFileUpdateResult?.UpdateFileManifestProperties != null && FileUpdater.SetFileUpdateRevisionNumber(activeZipFileUpdateResult.UpdateFileManifestProperties.Revision.ToString()))
								{
									CompleteUpdate(activeZipFileUpdateResult);
									FileUpdater?.PostDeviceStatus();
								}
								else
								{
									activeZipFileUpdateResult.Status = ZipFileUpdateStatus.Errored;
									activeZipFileUpdateResult.StatusMessage = "Unable to update revision number on device.";
								}
							}
							Log("Finished Processing zip file updates in " + Path.GetFileName(activeZipFileUpdateResult.FileName));
						}
						else
						{
							activeZipFileUpdateResult.Status = ZipFileUpdateStatus.Skipped;
							activeZipFileUpdateResult.StatusMessage = "Skipped processing because prior update file failed.";
							Log("Skipping processing of zip file updates in " + Path.GetFileName(activeZipFileUpdateResult.FileName) + " because prior update file failed.");
						}
					}
				}
				finally
				{
					Log("Completing file updates.");
					FileUpdater?.CompleteFileUpdates();
				}
			}
		}
		else
		{
			MoveOldRevisionFiles(fileUpdateResults, activeZipFileUpdateResults);
		}
		UpdateFileUpdateResultStatus(fileUpdateResults);
	}

	private void MoveOldRevisionFiles(IFileUpdateResults fileUpdateResults, List<IZipFileUpdateResult> activeZipFileUpdateResults)
	{
		if (fileUpdateResults?.ZipFileUpdateResultCollection == null)
		{
			return;
		}
		foreach (IZipFileUpdateResult item in fileUpdateResults.ZipFileUpdateResultCollection)
		{
			if (item.UpdateFileManifestProperties.Status == ManifestPropertiesStatus.RevisionDowngrade)
			{
				if (!File.Exists(item.FileName))
				{
					Log("Unable to move file " + item.FileName + " because it no longer exists.");
				}
				else
				{
					MoveFile(item, NotProcessedFolderPath);
				}
			}
		}
	}

	private bool IsZipFileUpdatAfterStartDate(IZipFileUpdateResult zipFileUpdateResult)
	{
		if (zipFileUpdateResult != null && (zipFileUpdateResult.UpdateFileManifestProperties?.StartDate).HasValue)
		{
			DateTime now = DateTime.Now;
			DateTime? obj = zipFileUpdateResult?.UpdateFileManifestProperties?.StartDate;
			return now >= obj;
		}
		return true;
	}

	private bool IsZipFileInUpdateTimeRange(IZipFileUpdateResult zipFileUpdateResult)
	{
		if (zipFileUpdateResult != null && (zipFileUpdateResult.UpdateFileManifestProperties?.UpdateRangeStart).HasValue && zipFileUpdateResult != null && (zipFileUpdateResult.UpdateFileManifestProperties?.UpdateRangeEnd).HasValue)
		{
			TimeSpan timeOfDay = DateTime.Now.TimeOfDay;
			TimeSpan? obj = zipFileUpdateResult?.UpdateFileManifestProperties?.UpdateRangeStart;
			if (timeOfDay >= obj)
			{
				timeOfDay = DateTime.Now.TimeOfDay;
				TimeSpan? obj2 = zipFileUpdateResult?.UpdateFileManifestProperties?.UpdateRangeEnd;
				return timeOfDay <= obj2;
			}
			return false;
		}
		return true;
	}

	private bool ZipFileContainsAllUpdateFiles(IZipFileUpdateResult zipFileUpdateResult)
	{
		bool result = true;
		if (zipFileUpdateResult.FileUpdateResultCollection != null && zipFileUpdateResult.FileUpdateResultCollection.Count > 0)
		{
			using ZipArchive zipArchive = ZipFile.OpenRead(zipFileUpdateResult.FileName);
			foreach (IFileUpdateResult item in zipFileUpdateResult.FileUpdateResultCollection)
			{
				if (zipArchive.GetEntry(item.UpdateFileModel.Path) == null)
				{
					item.FileUpdateStatus = FileUpdateStatus.FileMissing;
					result = false;
				}
			}
		}
		return result;
	}

	private void FindZipFileUpdates(IFileUpdateResults fileUpdateResults)
	{
		Log("Looking for update zip files in: " + UpdateFolderPath);
		if (Directory.Exists(UpdateFolderPath))
		{
			bool flag = false;
			foreach (string item in Directory.EnumerateFiles(UpdateFolderPath, "*.zip"))
			{
				Log("Found update file: " + Path.GetFileName(item));
				flag = true;
				fileUpdateResults.ZipFileUpdateResultCollection.Add(new ZipFileUpdateResult
				{
					FileName = item,
					Status = ZipFileUpdateStatus.NotProcessed,
					StatusMessage = "Not Processed"
				});
			}
			if (!flag)
			{
				fileUpdateResults.StatusMessage = "No files found to update.";
				Log("No files found to update.");
			}
		}
		else
		{
			Log("Update Folder " + UpdateFolderPath + " does not exist. No files found to update.");
		}
	}

	private void ReadManifestFiles(IFileUpdateResults fileUpdateResults)
	{
		foreach (IZipFileUpdateResult item in fileUpdateResults.ZipFileUpdateResultCollection)
		{
			ReadManifestFile(item.FileName, out var fileModels, out var manifestData);
			item.UpdateFileManifestProperties = manifestData;
			if (item.UpdateFileManifestProperties.Status != ManifestPropertiesStatus.Normal)
			{
				item.StatusMessage = $"Error in Manifest file.  Manifest Status: {item.UpdateFileManifestProperties.Status}";
				item.Status = ZipFileUpdateStatus.Errored;
			}
			if (fileModels == null)
			{
				continue;
			}
			foreach (IFileModel item2 in fileModels.OrderBy((IFileModel x) => x.Order))
			{
				if (item2 != null)
				{
					IFileUpdateResult fileUpdateResult = new FileUpdateResult
					{
						FileUpdateStatus = FileUpdateStatus.NotProcessed,
						UpdateFileModel = new FileModel
						{
							Order = item2.Order,
							Path = item2.Path,
							RebootRequired = item2.RebootRequired
						}
					};
					if (Path.GetFileName(fileUpdateResult?.UpdateFileModel?.Path).Length > 15)
					{
						fileUpdateResult.FileUpdateStatus = FileUpdateStatus.FileNameTooLong;
						item.Status = ZipFileUpdateStatus.Errored;
						item.StatusMessage = "Error in Manifest file.  File Name is too long " + Path.GetFileName(fileUpdateResult.UpdateFileModel.Path);
					}
					item.FileUpdateResultCollection.Add(fileUpdateResult);
				}
			}
		}
		if (fileUpdateResults.ZipFileUpdateResultCollection.Count > 0)
		{
			fileUpdateResults.ZipFileUpdateResultCollection.Sort(delegate(IZipFileUpdateResult a, IZipFileUpdateResult b)
			{
				int result = 0;
				if (a?.UpdateFileManifestProperties != null && b?.UpdateFileManifestProperties != null)
				{
					result = a.UpdateFileManifestProperties.Revision.CompareTo(b.UpdateFileManifestProperties.Revision);
				}
				return result;
			});
			string[] value = fileUpdateResults.ZipFileUpdateResultCollection.Select((IZipFileUpdateResult eachZipFileUpdate) => $"{Path.GetFileName(eachZipFileUpdate?.FileName)} revision: {eachZipFileUpdate?.UpdateFileManifestProperties.Revision ?? (-1)}").ToArray();
			Log("Sorted update files by revision: " + string.Join(", ", value));
		}
		UpdateFileUpdateResultStatus(fileUpdateResults);
	}

	private void InitializeOutputFolders(IFileUpdateResults fileUpdateResults)
	{
		if (!Directory.Exists(CompletedFolderPath))
		{
			try
			{
				Log("Creating FileUpdateService CompletedFolderPath: " + CompletedFolderPath);
				Directory.CreateDirectory(CompletedFolderPath);
			}
			catch (Exception arg)
			{
				Log($"Unable to create FileUpdateService CompletedFolderPath: {CompletedFolderPath}.  {arg}");
				fileUpdateResults.Success = false;
				fileUpdateResults.Status = FileUpdateHighLevelStatus.UnableToCreateCompletedFolder;
			}
		}
		if (!Directory.Exists(NotProcessedFolderPath))
		{
			try
			{
				Log("Creating FileUpdateService NotProcessedFolderPath: " + NotProcessedFolderPath);
				Directory.CreateDirectory(NotProcessedFolderPath);
			}
			catch (Exception arg2)
			{
				Log($"Unable to create FileUpdateService NotProcessedFolderPath: {NotProcessedFolderPath}.  {arg2}");
				fileUpdateResults.Success = false;
				fileUpdateResults.Status = FileUpdateHighLevelStatus.UnableToCreateNotProcessedFolder;
			}
		}
	}

	public void ReadManifestFile(string zipFileName, out List<IFileModel> fileModels, out IUpdateFileManifestProperties manifestData)
	{
		manifestData = new UpdateFileManifestProperties
		{
			Status = ManifestPropertiesStatus.Normal
		};
		fileModels = new List<IFileModel>();
		try
		{
			if (!File.Exists(zipFileName))
			{
				Log("GetManifestData No File Found at location: " + zipFileName);
				return;
			}
			using ZipArchive zipArchive = ZipFile.OpenRead(zipFileName);
			ZipArchiveEntry entry = zipArchive.GetEntry("manifest.json");
			if (entry == null)
			{
				manifestData.Status = ManifestPropertiesStatus.MissingManifestFile;
				Log("GetManifestData No Manifest File Found in zip file update: " + zipFileName);
				return;
			}
			UpdateFileManifest updateFileManifest = null;
			using StreamReader reader = new StreamReader(entry.Open());
			JsonSerializer jsonSerializer = new JsonSerializer();
			new JsonSerializerSettings();
			try
			{
				updateFileManifest = (UpdateFileManifest)jsonSerializer.Deserialize(reader, typeof(UpdateFileManifest));
				if (updateFileManifest != null)
				{
					if (updateFileManifest.Revision <= 0)
					{
						manifestData.Status = ManifestPropertiesStatus.MissingRevisionNumber;
						Log("Manifest Revision is missing.");
					}
					else
					{
						int num = FileUpdater?.GetFileUpdateRevisionNumber() ?? (-1);
						if (updateFileManifest != null && num.CompareTo(updateFileManifest.Revision) >= 0)
						{
							manifestData.Status = ManifestPropertiesStatus.RevisionDowngrade;
							Log(string.Format("{0}  Current revision: {1}  Manifest revision: {2}", "Current manifest file revision is greater than or equal to the incoming manifest file. Erroring update.", num, updateFileManifest.Revision));
						}
						else
						{
							Log($"Device current revision: {num}");
						}
					}
				}
			}
			catch (Exception arg)
			{
				manifestData.Status = ManifestPropertiesStatus.CorruptedManifestFile;
				LogError($"Error deserializing manifest file in zip file: {zipFileName}{Environment.NewLine} Exception: {arg}");
			}
			if (updateFileManifest != null)
			{
				ManifestPropertiesStatus status = manifestData.Status;
				manifestData = new UpdateFileManifestProperties
				{
					Name = updateFileManifest.Name,
					Status = status,
					Revision = updateFileManifest.Revision,
					Id = updateFileManifest.Id,
					StartDate = updateFileManifest.StartDate,
					UpdateRangeStart = updateFileManifest.UpdateRangeStart,
					UpdateRangeEnd = updateFileManifest.UpdateRangeEnd
				};
				if (updateFileManifest?.FileModels != null && updateFileManifest.FileModels.Any())
				{
					foreach (FileModel fileModel in updateFileManifest.FileModels)
					{
						fileModels.Add(new FileModel
						{
							Order = fileModel.Order,
							Path = fileModel.Path,
							RebootRequired = fileModel.RebootRequired
						});
					}
					return;
				}
				manifestData.Status = ManifestPropertiesStatus.ManifestHasNoUpdateFiles;
				Log($"GetManifestData No Files to update with manifest {manifestData.Id} - {manifestData.Name} in file: {zipFileName}");
			}
			else if (updateFileManifest == null)
			{
				manifestData.Status = ManifestPropertiesStatus.CorruptedManifestFile;
				Log("Error: Unable to deserialize manifest file from file update " + zipFileName);
			}
		}
		catch (Exception arg2)
		{
			manifestData.Status = ManifestPropertiesStatus.ExceptionWhileReading;
			LogError($"Error Reading Zip Update File: {zipFileName}{Environment.NewLine}Exception: {arg2}");
		}
	}

	private void CompleteUpdate(IZipFileUpdateResult zipFileUpdateResult)
	{
		zipFileUpdateResult.Status = ZipFileUpdateStatus.SuccessfullyProcessed;
		zipFileUpdateResult.StatusMessage = "File successfully updated.";
		if (!File.Exists(zipFileUpdateResult.FileName))
		{
			Log("Unable to move file " + zipFileUpdateResult.FileName + " because it no longer exists.");
		}
		else
		{
			MoveFile(zipFileUpdateResult, CompletedFolderPath);
		}
	}

	private void MoveFile(IZipFileUpdateResult zipFileUpdateResult, string destinationFolderName)
	{
		try
		{
			string text = Path.Combine(destinationFolderName, Path.GetFileName(zipFileUpdateResult.FileName));
			if (File.Exists(text))
			{
				text = GetUniqueFileName(text);
			}
			File.Move(zipFileUpdateResult.FileName, text);
			Log("Moved file " + Path.GetFileName(zipFileUpdateResult.FileName) + " to folder " + text);
		}
		catch (Exception arg)
		{
			LogError($"Failed To Move Update file {zipFileUpdateResult?.FileName}{Environment.NewLine}Exception: {arg}");
		}
	}

	private static string GetUniqueFileName(string destPath)
	{
		string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(destPath);
		string? directoryName = Path.GetDirectoryName(destPath);
		string extension = Path.GetExtension(destPath);
		string format = Path.Combine(directoryName, fileNameWithoutExtension + "{0}" + extension);
		int num = 1;
		string text = string.Format(format, "");
		while (File.Exists(text))
		{
			text = string.Format(format, "(" + num++ + ")");
		}
		destPath = text;
		return destPath;
	}

	private void Log(string message)
	{
		_logger?.LogInformation(message);
	}

	private void LogError(string message)
	{
		_logger?.LogError(message);
	}

	private void StartTimer()
	{
		double totalMilliseconds = TimeSpan.FromMinutes(15.0).TotalMilliseconds;
		_timer = new Timer
		{
			Interval = totalMilliseconds,
			AutoReset = true,
			Enabled = false
		};
		_timer.Elapsed += _timer_Elapsed;
		_timer.Start();
	}

	private void _timer_Elapsed(object sender, ElapsedEventArgs e)
	{
		_timer.Stop();
		try
		{
			UpdateFiles();
		}
		finally
		{
			_timer.Start();
		}
	}
}
