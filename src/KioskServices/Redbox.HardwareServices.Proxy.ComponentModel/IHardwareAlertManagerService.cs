namespace Redbox.HardwareServices.Proxy.ComponentModel;

public interface IHardwareAlertManagerService
{
	void SendJobAlert(string alertType, string source, string description, string jobId, string subType);

	void SendAlert(string alertType, string source, string description, string jobResults, string jobErrors, string subType);

	void SendAlert2(string typeOfSync, string jobId);

	string FormatJobResult(IHardwareResult jobResult);

	string FormatErrors(string jobId);
}
