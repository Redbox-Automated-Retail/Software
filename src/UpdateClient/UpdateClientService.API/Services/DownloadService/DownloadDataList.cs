using System;
using System.Collections.Generic;
using System.Linq;
using UpdateClientService.API.Services.FileSets;

namespace UpdateClientService.API.Services.DownloadService;

public class DownloadDataList : List<DownloadData>
{
	public bool IsDownloading => this.Any((DownloadData eachDownloadData) => eachDownloadData.DownloadState == DownloadState.Downloading || eachDownloadData.DownloadState == DownloadState.Error);

	public DownloadDataList GetByFileSetId(long fileSetId)
	{
		DownloadDataList downloadDataList = new DownloadDataList();
		using Enumerator enumerator = GetEnumerator();
		while (enumerator.MoveNext())
		{
			DownloadData current = enumerator.Current;
			if (current.ParseFileSetIdFromDownloadDataKey() == fileSetId)
			{
				downloadDataList.Add(current);
			}
		}
		return downloadDataList;
	}

	public DownloadData GetByRevisionChangeSetKey(RevisionChangeSetKey revisionChangeSetKey)
	{
		string key = DownloadData.GetRevisionKey(revisionChangeSetKey);
		return this.Where((DownloadData d) => d.Key == key).FirstOrDefault();
	}

	public DownloadData GetByBitsGuid(Guid bitsGuid)
	{
		return this.FirstOrDefault((DownloadData downloadData) => downloadData.BitsGuid == bitsGuid);
	}

	public bool ExistsByKey(string key)
	{
		return this.Any((DownloadData downloadData) => downloadData.Key == key);
	}
}
