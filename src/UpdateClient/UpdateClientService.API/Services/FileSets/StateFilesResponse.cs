using System.Collections.Generic;
using Redbox.NetCore.Middleware.Http;

namespace UpdateClientService.API.Services.FileSets;

public class StateFilesResponse : ApiBaseResponse
{
	public List<StateFile> StateFiles { get; set; }
}
