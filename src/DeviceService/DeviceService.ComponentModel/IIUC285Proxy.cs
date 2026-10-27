using System;
using System.Threading;
using System.Threading.Tasks;
using DeviceService.ComponentModel.Requests;
using DeviceService.ComponentModel.Responses;

namespace DeviceService.ComponentModel;

public interface IIUC285Proxy
{
	bool IsConnected { get; }

	UnitDataModel UnitData { get; }

	bool SupportsEMV { get; }

	bool SupportsVas { get; }

	bool HasValidVasRKI { get; }

	bool HasInvalidVasRKI { get; }

	bool HasIntermediateVasRepairKey { get; }

	event OnConnected OnConnectedHandler;

	event OnDisconnected OnDisconnectedHandler;

	bool UpdateDeviceConfiguration();

	UnitHealthModel GetUnitHealth();

	Task<bool> ReadCard(ICardReadRequest request, CancellationToken token, Action<Base87CardReadModel> jobCompleted, Action<string, string> eventsCallback);

	bool ReadConfig(string group, string index, out string Data, bool sendOffline = true);

	bool WriteConfig(string group, string index, string Data, bool sendOffline = true);

	Task<bool> WriteFile(string filePath, bool rebootAfterWrite = false);

	bool Reconnect();

	bool Reboot(RebootType reboot = RebootType.Manual);

	void ReadCancel();

	string GetVariable_29(string varID);

	InsertedStatus CheckIfCardInserted();

	bool PingDevice();

	void StartHealthTimer();

	void StopHealthTimer();

	Task<DeviceStatus> CheckDeviceStatus(long kioskId);

	void SetAuthorizationResponse(bool success);

	CardReaderState GetCardReaderState();
}
