using System.Collections.Generic;

namespace Redbox.Rental.Services.KioskClientService.Loyalty;

internal sealed class PersonalizationResponseError
{
	public string Code { get; set; }

	public string Description { get; set; }

	public List<string> Details { get; set; }

	public string Exception { get; set; }
}
