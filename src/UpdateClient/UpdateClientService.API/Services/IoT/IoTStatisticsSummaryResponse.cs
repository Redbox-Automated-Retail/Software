using System.Collections.Generic;
using Redbox.NetCore.Middleware.Http;

namespace UpdateClientService.API.Services.IoT;

public class IoTStatisticsSummaryResponse : ApiBaseResponse
{
	public List<IoTStatisticsSummary> ioTStatisticsSummaries { get; set; } = new List<IoTStatisticsSummary>();
}
