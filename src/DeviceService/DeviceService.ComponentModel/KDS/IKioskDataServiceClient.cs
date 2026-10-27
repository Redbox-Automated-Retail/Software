using System.Threading.Tasks;
using DeviceService.ComponentModel.Responses;

namespace DeviceService.ComponentModel.KDS;

public interface IKioskDataServiceClient
{
	Task<StandardResponse> PostDeviceStatus(DeviceStatus deviceStatus);

	Task<StandardResponse> PostRebootStatus(RebootStatus rebootStatus);

	Task<StandardResponse> PostCardStats(CardStats cardStats);
}
