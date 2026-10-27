namespace DeviceService.ComponentModel.Analytics;

public interface IAnalyticsService
{
	void StartWebHost();

	void StopWebHost();

	void ClientConnectedToHub();

	void ClientDisconnectedFromHub();

	void ServiceStarted();

	void ServiceStarting();

	void ServiceStopping();

	void ServiceStopped();
}
