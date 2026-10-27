using System.Collections.Generic;

namespace KioskClientService.API.Services.Transaction;

public class ReturnPromoCheckApiResponse : ReturnPromoCheckResponse
{
	public List<DiscReturnStatus> Discs { get; set; }
}
