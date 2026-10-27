using System;
using DeviceService.ComponentModel;
using DeviceService.ComponentModel.Requests;

namespace Redbox.KioskEngine.ComponentModel.TrackData;

public class CardReadRequestInfo
{
	public int Timeout { get; set; }

	public bool IsZeroTouchRead { get; set; }

	public DeviceInputType InputType { get; set; }

	public VasMode VasMode { get; set; }

	public Guid SessionId { get; set; }
}
