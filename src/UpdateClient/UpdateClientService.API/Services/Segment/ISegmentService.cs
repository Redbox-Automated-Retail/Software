using System.Threading.Tasks;
using Redbox.NetCore.Middleware.Http;

namespace UpdateClientService.API.Services.Segment;

public interface ISegmentService
{
	Task<ApiBaseResponse> UpdateKioskSegmentsFromUpdateService();

	Task<KioskSegmentsResponse> GetKioskSegments();

	Task<bool> UpdateKioskSegmentsIfNeeded();
}
