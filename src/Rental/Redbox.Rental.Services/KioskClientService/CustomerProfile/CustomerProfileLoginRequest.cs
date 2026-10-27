using System;
using DeviceService.ComponentModel;
using Newtonsoft.Json;
using Redbox.Core;
using Redbox.Rental.Model;
using Redbox.Rental.Model.KioskClientService.CustomerProfile;
using Redbox.Rental.Services.KioskClientService.Transactions;

namespace Redbox.Rental.Services.KioskClientService.CustomerProfile;

public class CustomerProfileLoginRequest : MessageBase, ICustomerProfileLoginRequest, IMessageScrub
{
	public string ActivityId { get; set; }

	public string EmailAddress { get; set; }

	public string Password { get; set; }

	public string PhoneNumber { get; set; }

	public bool IsEncrypted { get; set; }

	public string Pin { get; set; }

	public string CustomerProfileNumber { get; set; }

	public string LanguageCode { get; set; }

	public override object Scrub()
	{
		try
		{
			CustomerProfileLoginRequest customerProfileLoginRequest = JsonConvert.DeserializeObject<CustomerProfileLoginRequest>(JsonConvert.SerializeObject(this));
			customerProfileLoginRequest?.ScrubData();
			return JsonConvert.SerializeObject(customerProfileLoginRequest, Formatting.Indented);
		}
		catch (Exception arg)
		{
			return $"Exception in CustomerProfileLoginRequest.Scrub: {arg}";
		}
	}

	public override void ScrubData()
	{
		base.ScrubData();
		IObfuscationService service = ServiceLocator.Instance.GetService<IObfuscationService>();
		EmailAddress = service?.ObfuscateStringData(EmailAddress, 1, 0);
		Password = service?.ObfuscateStringData(Password, 0, 0);
		PhoneNumber = service?.ObfuscateStringData(PhoneNumber, 0, 0);
		Pin = service?.ObfuscateStringData(Pin, 0, 0);
	}
}
