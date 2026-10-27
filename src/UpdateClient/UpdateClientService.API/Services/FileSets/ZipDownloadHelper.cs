using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using Microsoft.Extensions.Logging;
using Redbox.NetCore.Logging.Extensions;
using UpdateClientService.API.Services.FileCache;

namespace UpdateClientService.API.Services.FileSets;

public class ZipDownloadHelper : IZipDownloadHelper
{
	private class ExtractionData
	{
		public bool IsSuccess { get; set; } = true;

		public string InfoText { get; set; }

		public byte[] FileData { get; set; }

		public Dictionary<string, object> InfoValues { get; set; } = new Dictionary<string, object>();

		public ZipArchive ZipArchive { get; set; }

		public ZipArchiveEntry ZipArchiveEntry { get; set; }

		public bool IsRevison => InfoValues.ContainsKey("RevisionId");

		public bool IsPatchFile => InfoValues.ContainsKey("PatchFileRevisionId");

		public long FileId => Convert.ToInt64(InfoValues["FileId"]);

		public long FileRevisionId => Convert.ToInt64(InfoValues["FileRevisionId"]);

		public long PatchFileRevisionId => Convert.ToInt64(InfoValues["PatchFileRevisionId"]);
	}

	private readonly IFileCacheService _fileCacheService;

	private readonly ILogger<ZipDownloadHelper> _logger;

	public ZipDownloadHelper(ILogger<ZipDownloadHelper> logger, IFileCacheService fileCacheService)
	{
		_fileCacheService = fileCacheService;
		_logger = logger;
	}

	public bool Extract(string zipPath, RevisionChangeSetKey revisionChangeSetKey)
	{
		bool result = true;
		try
		{
			ZipArchive zipArchive = ZipFile.OpenRead(zipPath);
			try
			{
				List<ZipArchiveEntry> list = zipArchive.Entries.Where((ZipArchiveEntry eachZipArchiveEntry) => Path.GetExtension(eachZipArchiveEntry.Name).ToLower() == ".json").ToList();
				list.ForEach(delegate(ZipArchiveEntry eachJsonZipArchiveEntry)
				{
					try
					{
						ExtractionData extractionData = new ExtractionData
						{
							ZipArchive = zipArchive,
							ZipArchiveEntry = eachJsonZipArchiveEntry
						};
						GetInfoText(extractionData);
						GetFileData(extractionData);
						if (extractionData.IsRevison)
						{
							if (!_fileCacheService.AddRevision(revisionChangeSetKey, extractionData.FileData, extractionData.InfoText))
							{
								_logger.LogErrorWithSource("Unable to add revision for " + revisionChangeSetKey?.IdentifyingText(), "Extract", "/sln/src/UpdateClientService.API/Services/FileSets/ZipDownloadHelper.cs");
								extractionData.IsSuccess = false;
							}
						}
						else if (extractionData.IsPatchFile)
						{
							if (!_fileCacheService.AddFilePatch(revisionChangeSetKey.FileSetId, extractionData.FileId, extractionData.FileRevisionId, extractionData.PatchFileRevisionId, extractionData.FileData, extractionData.InfoText))
							{
								_logger.LogErrorWithSource($"Unable to add file patch for FileSetId {revisionChangeSetKey.FileSetId}, FileId {extractionData.FileId}, FileRevisionId {extractionData.FileRevisionId}, PatchFileRevision {extractionData.PatchFileRevisionId}", "Extract", "/sln/src/UpdateClientService.API/Services/FileSets/ZipDownloadHelper.cs");
								extractionData.IsSuccess = false;
							}
						}
						else if (!_fileCacheService.AddFile(revisionChangeSetKey.FileSetId, extractionData.FileId, extractionData.FileRevisionId, extractionData.FileData, extractionData.InfoText))
						{
							_logger.LogErrorWithSource($"Unable to add file for FileSetId {revisionChangeSetKey.FileSetId}, FileId {extractionData.FileId}, FileRevisionId {extractionData.FileRevisionId}", "Extract", "/sln/src/UpdateClientService.API/Services/FileSets/ZipDownloadHelper.cs");
							extractionData.IsSuccess = false;
						}
						result &= extractionData.IsSuccess;
					}
					catch (Exception ex2)
					{
						_logger.LogErrorWithSource(ex2, "Exception while extracting " + eachJsonZipArchiveEntry.Name, "Extract", "/sln/src/UpdateClientService.API/Services/FileSets/ZipDownloadHelper.cs");
						result = false;
					}
				});
			}
			finally
			{
				if (zipArchive != null)
				{
					((IDisposable)zipArchive).Dispose();
				}
			}
		}
		catch (Exception ex)
		{
			_logger.LogErrorWithSource(ex, "Exception while extracting files for " + revisionChangeSetKey?.IdentifyingText() + ".", "Extract", "/sln/src/UpdateClientService.API/Services/FileSets/ZipDownloadHelper.cs");
			result = false;
		}
		return result;
	}

	private void GetInfoText(ExtractionData extractionData)
	{
		using (Stream stream = extractionData.ZipArchiveEntry.Open())
		{
			extractionData.InfoText = stream.ReadToEnd();
		}
		extractionData.InfoValues = extractionData.InfoText.ToObject<Dictionary<string, object>>();
	}

	private void GetFileData(ExtractionData extractionData)
	{
		string fileEntryName = Path.GetFileNameWithoutExtension(extractionData.ZipArchiveEntry.Name);
		ZipArchiveEntry zipArchiveEntry = extractionData.ZipArchive.Entries.Where((ZipArchiveEntry e) => e.Name.Equals(fileEntryName)).FirstOrDefault();
		if (zipArchiveEntry == null)
		{
			_logger.LogErrorWithSource(fileEntryName + " is missing from zip file.", "GetFileData", "/sln/src/UpdateClientService.API/Services/FileSets/ZipDownloadHelper.cs");
			extractionData.IsSuccess = false;
			return;
		}
		using Stream stream = zipArchiveEntry.Open();
		extractionData.FileData = stream.GetBytes();
	}
}
