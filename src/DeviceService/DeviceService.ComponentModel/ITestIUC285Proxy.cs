using DeviceService.ComponentModel.Responses;

namespace DeviceService.ComponentModel;

public interface ITestIUC285Proxy : IIUC285Proxy
{
	bool Connect();

	bool SetAmount(string amount);

	Base87CardReadModel ParseOnGuardData(string cardData);
}
