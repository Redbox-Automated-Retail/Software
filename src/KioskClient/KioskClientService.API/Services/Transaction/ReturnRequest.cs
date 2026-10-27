using System;
using Newtonsoft.Json;

namespace KioskClientService.API.Services.Transaction;

public class ReturnRequest : MessageBase
{
	public byte ReturnType { get; set; }

	public string Barcode { get; set; }

	public short Deck { get; set; }

	public short Slot { get; set; }

	public DateTime ReturnDate { get; set; }

	public bool FailedSecurityRead { get; set; }

	public override string ToString()
	{
		return JsonConvert.SerializeObject(this);
	}

	public override object Scrub()
	{
		return ToString();
	}
}
