using System.Collections.Generic;
using Redbox.KioskEngine.ComponentModel.TrackData;

namespace Redbox.KioskEngine.Environment.CardDataReader;

public class EMVTrackData : EncryptedTrackData, IEMVTrackData, IEncryptedTrackData, ITrackData
{
	public IDictionary<string, string> Tags { get; set; }

	public string AID { get; set; }
}
