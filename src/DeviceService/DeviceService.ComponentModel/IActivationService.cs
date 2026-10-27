using System.Threading.Tasks;
using DeviceService.ComponentModel.Requests;

namespace DeviceService.ComponentModel;

public interface IActivationService
{
	Task<bool> CheckAndActivate(IBluefinActivationRequest request);

	Task<bool> CheckAndActivate(IBluefinActivationRequest request, string mfgSerialNumber, string injectedSerialNumber);
}
