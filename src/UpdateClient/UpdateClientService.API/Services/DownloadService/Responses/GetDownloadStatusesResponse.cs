using System.Collections.Generic;
using Redbox.NetCore.Middleware.Http;

namespace UpdateClientService.API.Services.DownloadService.Responses;

public class GetDownloadStatusesResponse : ApiBaseResponse
{
	public List<DownloadData> Statuses { get; set; }
}
