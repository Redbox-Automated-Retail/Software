using DeviceService.ComponentModel;
using Redbox.KioskEngine.ComponentModel.KioskServices;
using Redbox.Rental.Model;

namespace Redbox.Rental.Services.KioskClientService.Transactions;

public class ReturnResult : BaseResponse, IReturnResult, IMessageScrub
{
	public object Scrub()
	{
		return this;
	}
}
