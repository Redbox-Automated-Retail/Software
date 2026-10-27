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
		long num = 17L;
		num = (num * 23 + key?.DownloadFileJobId.ToUpper().GetHashCode()) ?? 0.GetHashCode();
		return (int)num;
	}
}
