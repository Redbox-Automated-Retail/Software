using Redbox.Rental.Model;

namespace Redbox.Rental.Services.KioskClientService.Transactions;

public class ValidateReservationResult : BaseResponse
{
	public bool IsValid { get; set; }
}
