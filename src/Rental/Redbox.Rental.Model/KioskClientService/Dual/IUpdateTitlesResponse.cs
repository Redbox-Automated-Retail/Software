using System.Collections.Generic;

namespace Redbox.Rental.Model.KioskClientService.Dual;

public interface IUpdateTitlesResponse
{
	Dictionary<long, int> TitlesCount { get; }
}
