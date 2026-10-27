using System.Threading.Tasks;
using DeviceService.ComponentModel.Requests;
using DeviceService.ComponentModel.Responses;

namespace Status.Client;

public interface IStatusClient
{
	Task<StandardResponse> PostDeviceStatus(DeviceStatusRequest request);

	Task<StandardResponse> PostRebootStatus(RebootStatusRequest request);
}
