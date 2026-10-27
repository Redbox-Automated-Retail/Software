using System;
using Redbox.KioskEngine.ComponentModel.KioskServices;

namespace Redbox.Rental.Services.KioskClientService.Transactions;

public class TransactionEntity : ITransactionEntity
{
	public long TransactionEntityId { get; set; }

	public long TransactionId { get; set; }

	public long? DiscId { get; set; }

	public string Name { get; set; }

	public string Value { get; set; }

	public DateTime? LastUpdate { get; set; }

	public long? TitleId { get; set; }
}
