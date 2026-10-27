using System.Collections.Generic;
using KioskClientService.API.App;

namespace KioskClientService.API.Services.Dual;

public class UpdateTitlesResponse : ApiBaseResponse
{
	public Dictionary<long, int> TitlesCount { get; set; } = new Dictionary<long, int>();

	public override object Scrub()
	{
		return this.ToJson();
	}
}
