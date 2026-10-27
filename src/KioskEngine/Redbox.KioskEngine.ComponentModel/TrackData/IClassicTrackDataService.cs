using System;

namespace Redbox.KioskEngine.ComponentModel.TrackData;

public interface IClassicTrackDataService : ITrackDataService
{
	bool AllowTrackDataParsing { get; }

	bool ProcessKey(char keyChar, Action onTrackEnd);
}
