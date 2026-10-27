using System;
using System.IO;
using System.Threading.Tasks;

namespace DeviceService.ComponentModel.FileUpdate;

public interface IFileUpdater
{
	Task<bool> WriteStream(Stream stream, string fileName, bool rebootRequired, bool waitForReconnect = false, int reconnectTimeout = 180000);

	DateTime? GetExpectedRebootTime();

	int GetFileUpdateRevisionNumber();

	bool SetFileUpdateRevisionNumber(string revisionNumber);

	void PrepareToUpdateFiles();

	void CompleteFileUpdates();

	Task PostDeviceStatus();
}
