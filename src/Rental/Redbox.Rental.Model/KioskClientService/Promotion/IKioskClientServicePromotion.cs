using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Redbox.Rental.Model.KioskClientService.Promotion;

public interface IKioskClientServicePromotion
{
	Task<IDictionary<string, object>> ValidatePromotionCode(Guid messageId, string code, long kioskId);
}
