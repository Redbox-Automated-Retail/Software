using System.Collections.Generic;

namespace KioskClientService.API.Services.Transaction;

public static class ReturnErrorCodes
{
	public const string TransactionCreationFailed = "(RTN001)";

	public const string InvoiceCreationFailed = "(RTN002)";

	public const string CastingError = "(RTN003)";

	public const string NullRequest = "(RTN004)";

	public const string InvalidTransactionId = "(RTN005)";

	public const string InvalidBarcode = "(RTN006)";

	public const string MissingCustomerDisc = "(RTN007)";

	public const string MissingCustomerTransaction = "(RTN008)";

	public const string Exception = "(RTN009)";

	public const string QueuingError = "(RTN010)";

	public static List<string> DoNotQueueErrorCodes = new List<string> { "(RTN004)", "(RTN005)", "(RTN006)", "(RTN007)", "(RTN008)", "(V001)", "(V002)" };
}
