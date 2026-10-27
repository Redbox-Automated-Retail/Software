using System.Collections;
using System.Collections.Generic;
using Redbox.Rental.Services.App;

namespace Redbox.Rental.Services.KioskClientService.Loyalty;

internal static class ResponseExtensions
{
	internal static IDictionary<string, object> ToUpdateAccountDictionary(this APIResponse<UpdateAccountResponse> response)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>
		{
			{ "ServerBusy", false },
			{ "IsOnline", true },
			{ "Success", false }
		};
		if (response != null)
		{
			dictionary["ServerName"] = response.HostName;
			if (response.Response != null)
			{
				if (string.IsNullOrEmpty(response.Response.Error))
				{
					dictionary["Success"] = true;
				}
				else
				{
					Dictionary<string, object> value = new Dictionary<string, object> { 
					{
						"Code",
						response.Response.Error
					} };
					dictionary.Add("Errors", new ArrayList { value });
				}
				dictionary.Add("CustomerProfileNumber", response.Response.CustomerProfileNumber);
				dictionary.Add("MobilePhoneNumber", response.Response.MobilePhoneNumber);
				dictionary.Add("TempPassword", response.Response.TempPassword);
			}
		}
		return dictionary;
	}
}
