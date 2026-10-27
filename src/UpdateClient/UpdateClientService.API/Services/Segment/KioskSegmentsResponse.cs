using System.Collections.Generic;
using Redbox.NetCore.Middleware.Http;

namespace UpdateClientService.API.Services.Segment;

public class KioskSegmentsResponse : ApiBaseResponse
{
	public List<KioskSegmentModel> Segments { get; set; } = new List<KioskSegmentModel>();
}
