using Redbox.Rental.Model.KioskClientService.Campaign;
using Redbox.Rental.Services.App;

namespace Redbox.Rental.Services.KioskClientService.Campaign;

public class GetInCartsResponse : ApiBaseResponse
{
	public KioskInCartDetails InCart { get; set; }
}
