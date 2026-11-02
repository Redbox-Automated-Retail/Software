using System;
using Redbox.Rental.Model.Local;

namespace Redbox.Rental.Data;

public class BadCard : IBadCard
{
	public string HashId { get; set; } = null!; // set by the object initializer or the reader

	public int? NumberOfAttempts { get; set; }

	public DateTime? LastAttempt { get; set; }
}
