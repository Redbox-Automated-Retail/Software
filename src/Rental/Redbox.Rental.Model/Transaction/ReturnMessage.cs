using System;

namespace Redbox.Rental.Model.Transaction;

public class ReturnMessage
{
	public string MessageType { get; set; }

	public Guid MessageId { get; set; }

	public int KioskId { get; set; }

	public string EngineVersion { get; set; }

	public string BundleVersion { get; set; }

	public DateTime ReturnDate { get; set; }

	public ReturnType ReturnType { get; set; }

	public string Barcode { get; set; }

	public int Deck { get; set; }

	public int Slot { get; set; }

	public bool FailedSecurityRead { get; set; }
}
