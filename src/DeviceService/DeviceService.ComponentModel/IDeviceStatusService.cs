using System.Threading.Tasks;
using DeviceService.ComponentModel.Requests;
using DeviceService.ComponentModel.Responses;

namespace DeviceService.ComponentModel;

public interface IDeviceStatusService
{
	Task<StandardResponse> PostDeviceStatus(DeviceStatusRequest request);

	Task<StandardResponse> PostDeviceStatus();

	Task<DeviceStatus> GetDeviceStatus();
}
