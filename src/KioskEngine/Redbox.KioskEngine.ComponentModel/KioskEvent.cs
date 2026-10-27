using System;

namespace Redbox.KioskEngine.ComponentModel;

public class KioskEvent
{
	public Guid Id { get; set; }

	public int KioskEventType { get; set; }

	public int KioskEventSubType { get; set; }

	public bool Success { get; set; }

	public DateTime EventTime { get; set; }

	public int? Elapsed { get; set; }

	public Guid? RelatedId { get; set; }

	public string Data { get; set; }

	public KioskEvent()
	{
		Id = Guid.NewGuid();
		EventTime = DateTime.UtcNow;
	}

	public static KioskEvent NewEvent(int type, int kioskEventSubType, bool success, int? elapsed = null, Guid? relatedId = null, string data = null)
	{
		return new KioskEvent
		{
			KioskEventType = type,
			KioskEventSubType = kioskEventSubType,
			Success = success,
			Elapsed = elapsed,
			RelatedId = relatedId,
			Data = data
		};
	}
}
