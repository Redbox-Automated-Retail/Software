using Redbox.Rental.Model.KioskClientService.Transactions;
using Redbox.Rental.Services.App;

namespace Redbox.Rental.Services.KioskClientService.Transactions;

public class ReturnPromoCheckResponse : ApiBaseResponse
{
	public ReturnVisitPromo Promotion { get; set; }

	public bool IsLastDisc { get; set; }
}
