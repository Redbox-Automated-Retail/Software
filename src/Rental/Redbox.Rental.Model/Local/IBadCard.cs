using System;

namespace Redbox.Rental.Model.Local;

public interface IBadCard
{
	string HashId { get; set; }

	int? NumberOfAttempts { get; set; }

	DateTime? LastAttempt { get; set; }
}
