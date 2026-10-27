using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Redbox.NetCore.Logging.Extensions;
using UpdateClientService.API.Services.Files;
using UpdateClientService.API.Services.IoT.IoTCommand;

namespace UpdateClientService.API.Services.IoT.Commands.KioskFiles;

public class KioskFilesService : IKioskFilesService
{
	private readonly ILogger<KioskFilesService> _logger;

	private readonly IFileService _fileService;

	private readonly IIoTProcessStatusService _ioTProcessStatusService;

	public KioskFilesService(ILogger<KioskFilesService> logger, IFileService fileService, IIoTProcessStatusService ioTProcessStatusService, IIoTCommandClient iotCommandClient)
	{
		_logger = logger;
		_fileService = fileService;
		_ioTProcessStatusService = ioTProcessStatusService;
	}

	public async Task<MqttResponse<Dictionary<string, string>>> PeekRequestedFilesAsync(KioskFilePeekRequest logRequestModel)
	{
		MqttResponse<Dictionary<string, string>> mqttResponse = new MqttResponse<Dictionary<string, string>>
		{
			Data = new Dictionary<string, string>()
		};
		new List<FileInfo>();
		try
		{
			List<FileInfo> files;
			if (string.IsNullOrWhiteSpace(logRequestModel.FileQuery?.Path))
			{
				(files, mqttResponse.Error) = GetFilesByFileType(logRequestModel);
			}
			else
			{
				(files, mqttResponse.Error) = await GetFilesByQuery(logRequestModel);
			}
			if (files != null && files.Count > 0)
			{
				FilterFilesByDate(ref files, logRequestModel.Date, logRequestModel.EndDate);
			}
			files?.ForEach(delegate(FileInfo file)
			{
				string value = file.LastWriteTime.ToString();
				if (!string.IsNullOrWhiteSpace(logRequestModel.FileQuery?.Path))
				{
					FileQuery fileQuery = logRequestModel.FileQuery;
					if (fileQuery != null && fileQuery.SearchOptions?.RecurseSubdirectories == true)
					{
						mqttResponse.Data.Add(Path.GetRelativePath(logRequestModel.FileQuery.Path, file.FullName), value);
						return;
					}
				}
				mqttResponse.Data.Add(file.Name, value);
			});
			if (!mqttResponse.Data.Any())
			{
				mqttResponse.Error = "There were no files found for given request. Request -> " + logRequestModel.ToJson() + ", Additional Errors -> " + mqttResponse.Error;
			}
		}
		catch (Exception ex)
		{
			_logger.LogErrorWithSource(ex, "Exception while gettings FileInfos", "PeekRequestedFilesAsync", "/sln/src/UpdateClientService.API/Services/IoT/Commands/KioskFiles/KioskFilesService.cs");
			mqttResponse.Error = ex.Message + ". Logged full exception in UpdateClientService logs, take a look at it if you want to see the full exception details.";
			return mqttResponse;
		}
		_logger.LogInfoWithSource($"Returning ${mqttResponse.Data.Count} files.", "PeekRequestedFilesAsync", "/sln/src/UpdateClientService.API/Services/IoT/Commands/KioskFiles/KioskFilesService.cs");
		return mqttResponse;
	}

	public async Task<MqttResponse<string>> UploadFilesAsync(KioskUploadFileRequest kioskFileRequest)
	{
		if (_ioTProcessStatusService.UploadFilesStatus == IoTProcessStatusEnum.InProgress)
		{
			_logger.LogWarningWithSource("There was another upload files request while uploading files for another request.", "UploadFilesAsync", "/sln/src/UpdateClientService.API/Services/IoT/Commands/KioskFiles/KioskFilesService.cs");
			return new MqttResponse<string>
			{
				Error = "UpdateClientService: Already uploading files for another request, please try again in 1 or 2 minutes."
			};
		}
		if (kioskFileRequest.FileNames.Count > 30)
		{
			return new MqttResponse<string>
			{
				Error = "UpdateClientService: Can't upload more than 30 files at one time. Contact Kiosk Devs for questions."
			};
		}
		MqttResponse<string> mqttResponse = new MqttResponse<string>
		{
			Data = kioskFileRequest.S3PreSignedUrl
		};
		string path;
		string error;
		if (!string.IsNullOrWhiteSpace(kioskFileRequest?.BasePath))
		{
			path = kioskFileRequest.BasePath;
		}
		else if (!TryGetRequestedLogPath(kioskFileRequest.FileType, out path, out error))
		{
			_logger.LogErrorWithSource("An error occurred while trying to upload requested file to S3. Request Model -> " + kioskFileRequest.ToJson() + ". Error -> " + error, "UploadFilesAsync", "/sln/src/UpdateClientService.API/Services/IoT/Commands/KioskFiles/KioskFilesService.cs");
			mqttResponse.Error = "An error occurred while trying to upload requested file to S3. Take a look at UCS logs for full exception details. Error -> " + error;
			_ioTProcessStatusService.UploadFilesStatus = IoTProcessStatusEnum.Errored;
			return mqttResponse;
		}
		try
		{
			_ioTProcessStatusService.UploadFilesStatus = IoTProcessStatusEnum.InProgress;
			if (kioskFileRequest.FileNames.Count > 1 || kioskFileRequest.ZipFiles)
			{
				List<FileInfo> list = new List<FileInfo>();
				foreach (string fileName2 in kioskFileRequest.FileNames)
				{
					string fileName = Path.Combine(path, fileName2);
					list.Add(new FileInfo(fileName));
				}
				await _fileService.ZipAndUploadToS3(list, kioskFileRequest.S3PreSignedUrl);
			}
			else
			{
				string filePath = Path.Combine(path, kioskFileRequest.FileNames[0]);
				await _fileService.UploadFileToS3(kioskFileRequest.S3PreSignedUrl, filePath);
			}
		}
		catch (Exception ex)
		{
			_logger.LogErrorWithSource(ex, "An error occurred while trying to upload requested file to S3. Request Model -> " + kioskFileRequest.ToJson(), "UploadFilesAsync", "/sln/src/UpdateClientService.API/Services/IoT/Commands/KioskFiles/KioskFilesService.cs");
			mqttResponse.Error = "An error occurred while trying to upload requested file to S3. Take a look at UCS logs for full exception details. Error -> " + ex.Message;
			_ioTProcessStatusService.UploadFilesStatus = IoTProcessStatusEnum.Errored;
		}
		finally
		{
			_ioTProcessStatusService.UploadFilesStatus = ((_ioTProcessStatusService.UploadFilesStatus == IoTProcessStatusEnum.Errored) ? IoTProcessStatusEnum.Errored : IoTProcessStatusEnum.Finished);
		}
		_logger.LogInfoWithSource("Returning mqttresponse: " + mqttResponse.ToJson(), "UploadFilesAsync", "/sln/src/UpdateClientService.API/Services/IoT/Commands/KioskFiles/KioskFilesService.cs");
		return mqttResponse;
	}

	private (List<FileInfo> files, string error) GetFilesByFileType(KioskFilePeekRequest logRequestModel)
	{
		List<FileInfo> item = new List<FileInfo>();
		string error;
		try
		{
			if (!TryGetRequestedLogPath(logRequestModel.FileType, out var path, out error))
			{
				return (files: null, error: "Couldn't get path of " + (logRequestModel.FileType ?? "null") + ". Error -> " + error);
			}
			DirectoryInfo directoryInfo = new DirectoryInfo(path);
			item = directoryInfo.GetFiles()?.ToList() ?? new List<FileInfo>();
		}
		catch (Exception ex)
		{
			_logger.LogErrorWithSource(ex, "Exception while getting FileInfos", "GetFilesByFileType", "/sln/src/UpdateClientService.API/Services/IoT/Commands/KioskFiles/KioskFilesService.cs");
			error = ex.Message;
		}
		return (files: item, error: error);
	}

	private async Task<(List<FileInfo> files, string error)> GetFilesByQuery(KioskFilePeekRequest logRequestModel, TimeSpan? timeout = null)
	{
		CancellationTokenSource cancellationTokenSource = (timeout.HasValue ? new CancellationTokenSource(timeout.Value) : new CancellationTokenSource(TimeSpan.FromSeconds(90.0)));
		return await Task.Run(delegate
		{
			List<FileInfo> item = new List<FileInfo>();
			string item2 = null;
			try
			{
				FileQuery fileQuery = logRequestModel.FileQuery;
				DirectoryInfo directoryInfo = new DirectoryInfo(fileQuery.Path);
				item = directoryInfo.GetFiles((!string.IsNullOrWhiteSpace(fileQuery.SearchPattern)) ? fileQuery.SearchPattern : "*", fileQuery.SearchOptions ?? new EnumerationOptions()).ToList() ?? new List<FileInfo>();
			}
			catch (Exception ex)
			{
				_logger.LogErrorWithSource(ex, "Exception while getting FileInfos", "GetFilesByQuery", "/sln/src/UpdateClientService.API/Services/IoT/Commands/KioskFiles/KioskFilesService.cs");
				item2 = ex.Message;
			}
			return (files: item, error: item2);
		}, cancellationTokenSource.Token);
	}

	private static void FilterFilesByDate(ref List<FileInfo> files, DateTime? date, DateTime? endDate)
	{
		if (files == null || files.Count == 0)
		{
			return;
		}
		if (!date.Equals(null) && !endDate.Equals(null))
		{
			files = files.Where(delegate(FileInfo x)
			{
				DateTime lastWriteTime = x.LastWriteTime;
				DateTime? dateTime = date;
				return lastWriteTime >= dateTime && x.LastWriteTime <= endDate.Value.AddDays(1.0);
			}).ToList();
		}
		else if (!date.Equals(null))
		{
			files = files.Where((FileInfo x) => x.LastWriteTime.ToShortDateString() == date?.ToShortDateString()).ToList();
		}
		else
		{
			files = files.ToList();
		}
	}

	private bool TryGetRequestedLogPath(string logType, out string path, out string error)
	{
		path = null;
		error = null;
		if (Enum.TryParse<FileTypeEnum>(logType, out var result) && FilePaths.TypeMappings.TryGetValue(result, out path) && !string.IsNullOrWhiteSpace(path))
		{
			return true;
		}
		error = (logType ?? "null") + " was not found in FilePaths";
		_logger.LogErrorWithSource(error, "TryGetRequestedLogPath", "/sln/src/UpdateClientService.API/Services/IoT/Commands/KioskFiles/KioskFilesService.cs");
		return false;
	}
}
