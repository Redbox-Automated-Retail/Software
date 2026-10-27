using System;
using System.Collections.Generic;

namespace Redbox.Rental.Model.KioskClientService.Inventory;

public interface IKioskTitlesCountResponse
{
	Guid MessageId { get; }

	List<_KioskTitlesCount> KioskTitlesCount { get; }
}
