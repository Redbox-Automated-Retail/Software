using System;

namespace Redbox.KioskEngine.ComponentModel.KioskServices;

public class AdImpression
{
	public DateTime ImpressionStart { get; set; }

	public DateTime ImpressionEnd { get; set; }

	public int LocationId { get; set; }

	public int Count { get; set; }
}
