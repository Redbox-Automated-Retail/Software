using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Redbox.NetCore.Logging.Extensions;

namespace UpdateClientService.API.Services.Files;

public class FileService : IFileService
{
	private readonly ILogger<FileService> _logger;

	public FileService(ILogger<FileService> logger)
	{
		_logger = logger;
	}

	public async Task ZipAndUploadToS3(List<FileInfo> fileToUpload, string presignedS3)
	{
		int num = new Random().Next();
		string zipPath = Path.GetTempPath() + $"{num}.zip";
		_logger.LogInfoWithSource("Created temporary zip path: " + zipPath, "ZipAndUploadToS3", "/sln/src/UpdateClientService.API/Services/Files/FileService.cs");
		Exception exception = null;
		try
		{
			using (ZipArchive zipTo = ZipFile.Open(zipPath, ZipArchiveMode.Create))
			{
				foreach (FileInfo item in fileToUpload)
				{
					_logger.LogInfoWithSource("Adding " + item.Name + " into " + zipPath + " zip file.", "ZipAndUploadToS3", "/sln/src/UpdateClientService.API/Services/Files/FileService.cs");
					if (item.Length > 30000000)
					{
						throw new Exception($"File: {item.Name} size is {item.Length} byte, exceeds 30 mb limit. Can't zip file that is larger than 30mb. Contact kiosk team for questions.");
					}
					ZipArchiveEntry zipArchiveEntry = zipTo.CreateEntry(item.Name);
					using FileStream fileStream = File.Open(item.FullName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
					using Stream streamTo = zipArchiveEntry.Open();
					await fileStream.CopyToAsync(streamTo);
				}
			}
			await UploadFileToS3(presignedS3, zipPath);
		}
		catch (Exception ex)
		{
			exception = ex;
			_logger.LogErrorWithSource(ex, "Exception while creating zip file", "ZipAndUploadToS3", "/sln/src/UpdateClientService.API/Services/Files/FileService.cs");
		}
		finally
		{
			_logger.LogInfoWithSource("Deleting temporary zip file: " + zipPath, "ZipAndUploadToS3", "/sln/src/UpdateClientService.API/Services/Files/FileService.cs");
			File.Delete(zipPath);
		}
		if (exception != null)
		{
			throw exception;
		}
	}

	public async Task UploadFileToS3(string presignedS3, string filePath)
	{
		_logger.LogInfoWithSource("Uploading file: $" + filePath + " to given s3 presigned Url", "UploadFileToS3", "/sln/src/UpdateClientService.API/Services/Files/FileService.cs");
		using (new HttpWebResponse())
		{
			HttpWebRequest httpRequest = WebRequest.Create(presignedS3) as HttpWebRequest;
			httpRequest.Method = "PUT";
			using (Stream dataStream = httpRequest.GetRequestStream())
			{
				byte[] buffer = new byte[8000];
				using FileStream fileStream = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
				int count;
				while ((count = await fileStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
				{
					await dataStream.WriteAsync(buffer, 0, count);
				}
			}
			httpRequest.GetResponse();
			_logger.LogInfoWithSource("Successfully uploaded " + filePath + " to the s3", "UploadFileToS3", "/sln/src/UpdateClientService.API/Services/Files/FileService.cs");
		}
	}
}
