namespace Redbox.KioskEngine.ComponentModel.TrackData;

public interface ITechnicalFallbackTracking
{
	bool SupportsTechnicalFallbackTracking { get; }

	bool IsInTrackableTechnicalFallback { get; }
}
