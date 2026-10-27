using Newtonsoft.Json;

namespace Redbox.Rental.Services.KioskClientService.Transactions;

public class ReturnRequest : MessageBase
{
	public byte ReturnType { get; set; }

	public string Barcode { get; set; }

	public int Deck { get; set; }

	public int Slot { get; set; }

	public string ReturnDate { get; set; }

	public bool FailedSecurityRead { get; set; }

	public override string ToString()
	{
		return JsonConvert.SerializeObject(this);
	}
}
