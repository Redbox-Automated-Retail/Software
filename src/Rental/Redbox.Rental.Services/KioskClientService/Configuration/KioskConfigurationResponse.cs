using Redbox.Rental.Model.KioskClientService.Configuration;
using Redbox.Rental.Services.App;

namespace Redbox.Rental.Services.KioskClientService.Configuration;

public class KioskConfigurationResponse : ApiBaseResponse
{
	public KioskConfiguration Kioskconfiguration { get; set; }
}
