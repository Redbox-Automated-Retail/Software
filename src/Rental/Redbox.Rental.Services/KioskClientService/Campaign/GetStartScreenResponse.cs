using Redbox.Rental.Model.KioskClientService.Campaign;
using Redbox.Rental.Services.App;

namespace Redbox.Rental.Services.KioskClientService.Campaign;

public class GetStartScreenResponse : ApiBaseResponse
{
	public KioskStartScreenDetails StartScreen { get; set; }
}
