using System;

namespace Redbox.Rental.Model;

public interface IMessageBase
{
	Guid MessageId { get; set; }

	long KioskId { get; set; }
}
