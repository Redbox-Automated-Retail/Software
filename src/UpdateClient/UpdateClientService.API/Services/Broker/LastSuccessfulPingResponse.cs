using System;
using Redbox.NetCore.Middleware.Http;

namespace UpdateClientService.API.Services.Broker;

public class LastSuccessfulPingResponse : ApiBaseResponse
{
	public DateTime? LastSuccessfulPingUTC { get; set; }
}
