namespace Redbox.Rental.Model;

public class MaintenanceModeCodes
{
	public const string TamperedCardReader = "0099CDRR";

	public const string CorruptDb = "SOFT-FILE01";

	public const string CorruptDb2 = "SOFT-FILE02";

	public const string CreditCardReaderErrorCode = "0072CDRR";

	public const string DisconnectedCardReaderOverThresholdErrorCode = "0073CDRR";

	public const string CreditCardReaderCardNotRemovedErrorCode = "0100CDRR";
}
