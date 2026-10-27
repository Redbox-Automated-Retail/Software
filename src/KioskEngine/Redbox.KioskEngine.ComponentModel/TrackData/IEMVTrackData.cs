using System.Collections.Generic;

namespace Redbox.KioskEngine.ComponentModel.TrackData;

public interface IEMVTrackData : IEncryptedTrackData, ITrackData
{
	IDictionary<string, string> Tags { get; set; }

	string AID { get; set; }
}
