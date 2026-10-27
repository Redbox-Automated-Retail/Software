using System;
using System.Collections.Generic;
using System.Security;
using System.Threading.Tasks;
using Redbox.Rental.Model.KioskClientService.Transactions;
using Redbox.Rental.Model.Personalization;

namespace Redbox.Rental.Model.KioskClientService.Loyalty;

public interface IKioskClientServiceLoyalty
{
	Task<IDictionary<string, object>> EstimateAccrual(CartMessage request);

	Task<IDictionary<string, object>> EstimateRedemption(CartMessage request);

	Task<IDictionary<string, object>> UpdateAccount(Guid messageId, long kioskId, string customerProfileNumber, string emailAddress, string phoneNumber, SecureString pin, bool punchcardOptIn, bool textClubOptIn, AccountSourceType source, bool replay);
}
