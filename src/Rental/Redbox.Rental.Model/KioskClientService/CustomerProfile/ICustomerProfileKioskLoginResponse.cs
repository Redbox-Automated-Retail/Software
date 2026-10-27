using System;
using DeviceService.ComponentModel;

namespace Redbox.Rental.Model.KioskClientService.CustomerProfile;

public interface ICustomerProfileKioskLoginResponse : IBaseResponse, IMessageScrub
{
	Guid MessageId { get; set; }

	KioskLoginResponse Response { get; set; }
}
