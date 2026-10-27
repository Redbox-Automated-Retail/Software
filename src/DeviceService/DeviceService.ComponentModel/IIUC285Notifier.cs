using System;
using DeviceService.ComponentModel.Responses;

namespace DeviceService.ComponentModel;

public interface IIUC285Notifier
{
	void SendCardReaderConnectedEvent();

	void SendCardReaderStateEvent(CardReaderState cardReaderState);

	void SendDeviceServiceCanShutDownEvent();

	void SendDeviceServiceShutDownStartingEvent(ShutDownReason shutDownReason);

	void SendCardReaderDisconnectedEvent(Exception exception);

	void SetCommandQueueState(bool commandQueuePaused);

	void SendDeviceTamperedEvent();
}
