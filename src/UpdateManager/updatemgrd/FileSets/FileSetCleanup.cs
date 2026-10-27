using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Redbox.Core;
using Redbox.UpdateManager.ComponentModel;
using Redbox.UpdateManager.FileCache;

namespace Redbox.UpdateManager.FileSets;

internal class FileSetCleanup
{
	public ErrorList Run()
	{
		ErrorList errorList = new ErrorList();
		try
		{
			LogHelper.Instance.Log("Starting FileSetCleanup");
			errorList.AddRange(StateFile.GetAll(out var stateFiles));
			if (errorList.ContainsError())
			{
				return errorList;
			}
			FileCacheService.Instance.GetAllFileSets().ForEach(delegate(long fileSetId)
			{
				List<long> revisionIds = new List<long>();
				stateFiles.ForEach(delegate(StateFile sf)
				{
					if (sf.FileSetId == fileSetId)
					{
						if (sf.HasActive())
						{
							revisionIds.Add(sf.RevisionId);
						}
						if (sf.HasInProgress())
						{
							revisionIds.Add(sf.InProgressRevisionId);
						}
					}
				});
				revisionIds = revisionIds.Distinct().ToList();
				if (revisionIds.Count != 0)
				{
					List<ClientFileSetRevision> revisions = new List<ClientFileSetRevision>();
					revisionIds.ForEach(delegate(long r)
					{
						ClientFileSetRevision revision = FileSetService.Instance.GetRevision(fileSetId, r);
						if (revision != null)
						{
							revisions.Add(revision);
						}
					});
					if (revisions.Count != 0)
					{
						ClientFileSetRevision clientFileSetRevision = revisions.Last();
						int retentionDays = clientFileSetRevision.RetentionDays;
						int retentionRevisions = clientFileSetRevision.RetentionRevisions;
						if (retentionDays != 0 || retentionRevisions != 0)
						{
							List<FileSetFileInfo> allFileInfos = new List<FileSetFileInfo>();
							FileCacheService.Instance.GetAllFileInfos(fileSetId).ForEach(delegate(string path)
							{
								allFileInfos.Add(FileSetService.Instance.GetFileInfo(path));
							});
							List<FileSetFileInfo> requiredFiles = new List<FileSetFileInfo>();
							List<string> list = (from s in FileCacheService.Instance.GetAllRevisionFiles(fileSetId)
								orderby s descending
								select s).ToList();
							int pos = 1;
							list.ForEach(delegate(string path)
							{
								ClientFileSetRevision revision = FileSetService.Instance.GetRevision(path);
								if (revision != null)
								{
									bool flag = true;
									try
									{
										if (revisions.Exists((ClientFileSetRevision r) => r.RevisionId == revision.RevisionId))
										{
											return;
										}
										bool? flag2 = null;
										bool? flag3 = null;
										if (retentionRevisions > 0 && pos > retentionRevisions)
										{
											flag2 = true;
										}
										int num = (int)(DateTime.Now - File.GetCreationTime(path)).TotalDays;
										if (num <= 1)
										{
											return;
										}
										if (retentionDays > 0 && num > retentionDays)
										{
											flag3 = true;
										}
										if ((flag2 ?? true) && (flag3 ?? true))
										{
											flag = false;
											LogHelper.Instance.Log($"Deleting FileSetId: {fileSetId} RevisionId {revision.RevisionId} PatchRevisionId {revision.PatchRevisionId}", LogEntryType.Debug);
											FileCacheService.Instance.DeleteRevision(fileSetId, revision.RevisionId, revision.PatchRevisionId);
										}
									}
									finally
									{
										if (flag)
										{
											AddRequiredFiles(allFileInfos, requiredFiles, revision);
										}
									}
									pos++;
								}
							});
							RemoveFiles(fileSetId, allFileInfos, requiredFiles);
						}
					}
				}
			});
			LogHelper.Instance.Log("Finished FileSetCleanup");
		}
		catch (Exception e)
		{
			errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("FileSetCleanup.Run", "Unhandled exception occurred", e));
		}
		return errorList;
	}

	private void AddRequiredFiles(List<FileSetFileInfo> allFileInfos, List<FileSetFileInfo> requiredFiles, ClientFileSetRevision revision)
	{
		List<FileSetFileInfo> collection = (from a in revision.Files
			join b in allFileInfos on new { a.FileId, a.FileRevisionId } equals new { b.FileId, b.FileRevisionId }
			select b).ToList();
		requiredFiles.AddRange(collection);
		List<FileSetFileInfo> collection2 = (from a in revision.PatchFiles
			join b in allFileInfos on new
			{
				FileId = a.FileId,
				FileRevisionId = a.PatchFileRevisionId
			} equals new { b.FileId, b.FileRevisionId }
			select b).ToList();
		requiredFiles.AddRange(collection2);
	}

	private void RemoveFiles(long fileSetId, List<FileSetFileInfo> allFileInfos, List<FileSetFileInfo> requiredFiles)
	{
		allFileInfos.Except(requiredFiles.Distinct()).ToList().ForEach(delegate(FileSetFileInfo r)
		{
			LogHelper.Instance.Log($"Deleting FileSetId: {fileSetId} FileId {r.FileId} FileRevisionId {r.FileRevisionId}", LogEntryType.Debug);
			FileCacheService.Instance.DeleteFile(fileSetId, r.FileId, r.FileRevisionId);
		});
	}
}
