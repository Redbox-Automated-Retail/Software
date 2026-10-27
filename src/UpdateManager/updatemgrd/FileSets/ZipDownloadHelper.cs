using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Ionic.Zip;
using Redbox.Core;
using Redbox.UpdateManager.ComponentModel;
using Redbox.UpdateManager.FileCache;

namespace Redbox.UpdateManager.FileSets;

internal class ZipDownloadHelper
{
	private string _zipPath;

	private long _fileSetId;

	private long _revisionId;

	private long _patchRevisionId;

	private IFileCacheService _fileCacheService;

	internal IFileCacheService FileCacheService
	{
		get
		{
			if (_fileCacheService == null)
			{
				_fileCacheService = ServiceLocator.Instance.GetService<IFileCacheService>();
			}
			return _fileCacheService;
		}
	}

	internal ZipDownloadHelper(string zipPath, long fileSetId, long revisionId, long patchRevisionId)
	{
		_zipPath = zipPath;
		_fileSetId = fileSetId;
		_revisionId = revisionId;
		_patchRevisionId = patchRevisionId;
	}

	public ErrorList Extract()
	{
		ErrorList errors = new ErrorList();
		try
		{
			ZipFile zip = ZipFile.Read(_zipPath);
			try
			{
				zip.Where((ZipEntry zipEntry) => Path.GetExtension(zipEntry.FileName).ToLower() == ".txt").ToList().ForEach(delegate(ZipEntry entry)
				{
					try
					{
						string text;
						Dictionary<string, object> dictionary;
						using (MemoryStream memoryStream = new MemoryStream())
						{
							entry.Extract(memoryStream);
							memoryStream.Position = 0L;
							text = Encoding.ASCII.GetString(memoryStream.GetBytes());
							dictionary = text.ToDictionary();
						}
						string fileEntryName = Path.GetFileNameWithoutExtension(entry.FileName);
						ZipEntry zipEntry = zip.Where((ZipEntry zipEntry2) => zipEntry2.FileName.Equals(fileEntryName)).FirstOrDefault();
						if (fileEntryName == null)
						{
							errors.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("ZipDownloadHelper.Extract", $"File {fileEntryName} is missing from zip file"));
						}
						else
						{
							byte[] bytes;
							using (MemoryStream memoryStream2 = new MemoryStream())
							{
								zipEntry.Extract(memoryStream2);
								memoryStream2.Position = 0L;
								bytes = memoryStream2.GetBytes();
							}
							if (dictionary.ContainsKey("RevisionId"))
							{
								errors.AddRange(FileCacheService.AddRevision(_fileSetId, _revisionId, _patchRevisionId, bytes, text));
							}
							else
							{
								long fileId = Convert.ToInt64(dictionary["FileId"]);
								long fileRevisionId = Convert.ToInt64(dictionary["FileRevisionId"]);
								if (dictionary.ContainsKey("PatchFileRevisionId"))
								{
									long filePatchRevisionId = Convert.ToInt64(dictionary["PatchFileRevisionId"]);
									errors.AddRange(FileCacheService.AddFilePatch(_fileSetId, fileId, fileRevisionId, filePatchRevisionId, bytes, text));
								}
								else
								{
									errors.AddRange(FileCacheService.AddFile(_fileSetId, fileId, fileRevisionId, bytes, text));
								}
							}
						}
					}
					catch (Exception e2)
					{
						errors.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("ZipDownloadHelper.Extract", $"An unhandled exception occurred extracting {entry.FileName}", e2));
					}
				});
			}
			finally
			{
				if (zip != null)
				{
					((IDisposable)zip).Dispose();
				}
			}
		}
		catch (Exception e)
		{
			errors.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("ZipDownloadHelper.Extract", "An unhandled exception occurred", e));
		}
		return errors;
	}
}
