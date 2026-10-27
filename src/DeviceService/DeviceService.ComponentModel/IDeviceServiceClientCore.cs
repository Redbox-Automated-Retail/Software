using DeviceService.ComponentModel.Responses;

namespace DeviceService.ComponentModel;

public interface IDeviceServiceClientCore
{
	bool IsConnectedToDeviceService { get; }

	event LogHandler OnLog;

	event OnConnected OnConnectedHandler;

	event OnDisconnected OnDisconnectedHandler;

	bool ConnectToDeviceService(string url);

	bool ConnectToDeviceService(string url, int connectionTimeout);

	void StartConnectionToDeviceService(string url);

	void SendCardReaderConnectedEvent();

	void SendCardReaderStateEvent(CardReaderState cardReaderState);

	void SendCardReaderDisconnectedEvent();

	void SendDeviceServiceCanShutDownEvent();

	void SendDeviceServiceShutDownStartingEvent(ShutDownReason shutDownReason);

	void SetCommandQueueState(bool commandQueuePaused);

	void SendDeviceTamperedEvent();
}
