namespace Redbox.KioskEngine.ComponentModel.TrackData;

public interface IDeviceServiceTrackDataService : ITrackDataService
{
	bool IsInTechnicalFallback { get; }
}
