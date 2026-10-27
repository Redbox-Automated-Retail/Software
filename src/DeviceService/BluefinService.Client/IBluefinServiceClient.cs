using System;
using System.Threading.Tasks;
using DeviceService.ComponentModel.Requests;
using DeviceService.ComponentModel.Responses;

namespace BluefinService.Client;

public interface IBluefinServiceClient
{
	Task<StandardResponse> Activate(IBluefinActivationRequest request, string mfgSerialNumber, string injectedSerialNumber, DateTime InstallDate);

	Task<StandardResponse> Deactivate(IBluefinActivationRequest request, string mfgSerialNumber);
}
