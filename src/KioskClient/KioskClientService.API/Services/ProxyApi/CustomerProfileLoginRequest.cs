using System;
using KioskClientService.API.App;
using Newtonsoft.Json;

namespace KioskClientService.API.Services.ProxyApi;

public class CustomerProfileLoginRequest : IMessageScrub
{
	public Guid MessageId { get; set; }

	public long KioskId { get; set; }

	public string ActivityId { get; set; }

	public string EmailAddress { get; set; }

	public string Password { get; set; }

	public string PhoneNumber { get; set; }

	public bool IsEncrypted { get; set; }

	public string Pin { get; set; }

	public string CustomerProfileNumber { get; set; }

	public string LanguageCode { get; set; }

	public object Scrub()
	{
		try
		{
			CustomerProfileLoginRequest customerProfileLoginRequest = JsonConvert.DeserializeObject<CustomerProfileLoginRequest>(this.ToJson());
			customerProfileLoginRequest?.ScrubData();
			return customerProfileLoginRequest?.ToJsonIndented();
		}
		catch (Exception arg)
		{
			return $"Exception in CustomerProfileLoginRequest.Scrub: {arg}";
		}
	}

	public void ScrubData()
	{
		EmailAddress = ObfuscationHelper.ObfuscateStringData(EmailAddress, 1, 0);
		Password = ObfuscationHelper.ObfuscateStringData(Password, 1, 0);
		PhoneNumber = ObfuscationHelper.ObfuscateNumericData(PhoneNumber, 1, 0);
		Pin = ObfuscationHelper.ObfuscateNumericData(Pin, 1, 0);
	}
}
