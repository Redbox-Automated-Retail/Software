using System;

namespace Redbox.KioskEngine.ComponentModel.KioskServices;

public interface ITransactionEntity
{
	long TransactionEntityId { get; set; }

	long TransactionId { get; set; }

	long? DiscId { get; set; }

	string Name { get; set; }

	string Value { get; set; }

	DateTime? LastUpdate { get; set; }

	long? TitleId { get; set; }
}
