namespace Redbox.HardwareServices.Proxy.ComponentModel;

public interface ISyncUnknownProcessor
{
	void Initialize();

	void ScheduleJob(bool forceSync = false);

	void ProcessSyncLocationsComplete(string jobId);
}
