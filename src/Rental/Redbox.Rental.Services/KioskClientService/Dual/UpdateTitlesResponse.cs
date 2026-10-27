using System.Collections.Generic;
using Redbox.Rental.Model.KioskClientService.Dual;
using Redbox.Rental.Services.App;

namespace Redbox.Rental.Services.KioskClientService.Dual;

public class UpdateTitlesResponse : ApiBaseResponse, IUpdateTitlesResponse
{
	public Dictionary<long, int> TitlesCount { get; set; } = new Dictionary<long, int>();
}
