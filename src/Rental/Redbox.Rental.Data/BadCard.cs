using System;
using Redbox.Rental.Model.Local;

namespace Redbox.Rental.Data;

public class BadCard : IBadCard
{
	public string HashId { get; set; }

	public int? NumberOfAttempts { get; set; }

	public DateTime? LastAttempt { get; set; }
}
