using System;

namespace Redbox.KioskEngine.ComponentModel.TrackData;

public class CardReadCompleteDetails
{
	public Guid? RequestId { get; set; }

	public bool Success { get; set; }

	public bool CardRemoved { get; set; }

	public ITrackData TrackData { get; set; }

	public string VasData { get; set; }
}
