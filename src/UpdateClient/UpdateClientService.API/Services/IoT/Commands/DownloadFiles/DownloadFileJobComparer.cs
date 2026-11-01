using System.Collections.Generic;

namespace UpdateClientService.API.Services.IoT.Commands.DownloadFiles;

public class DownloadFileJobComparer : IEqualityComparer<DownloadFileJob>
{
	public bool Equals(DownloadFileJob first, DownloadFileJob second)
	{
		return first?.DownloadFileJobId == second?.DownloadFileJobId;
	}

	public int GetHashCode(DownloadFileJob key)
	{
		long hash = 17L;
		hash = (hash * 23 + key?.DownloadFileJobId.ToUpper().GetHashCode()) ?? 0.GetHashCode();
		return (int)hash;
	}
}
