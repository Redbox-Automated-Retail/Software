using System.Collections.Generic;
using System.Net;
using Redbox.NetCore.Middleware.Http;

namespace UpdateClientService.API.Services;

public class FileVersionDataResponse : ApiBaseResponse
{
	public IEnumerable<FileVersionData> Data { get; set; }

	public FileVersionDataResponse()
	{
		base.StatusCode = HttpStatusCode.OK;
	}
}
