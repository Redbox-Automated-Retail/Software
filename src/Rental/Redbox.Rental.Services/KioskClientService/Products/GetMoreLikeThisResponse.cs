using System.Collections.Generic;
using Redbox.Rental.Model;

namespace Redbox.Rental.Services.KioskClientService.Products;

public class GetMoreLikeThisResponse : BaseResponse
{
	public List<long> MoreLikeThis { get; set; }
}
