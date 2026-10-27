using System;
using System.Collections.Generic;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Rental.Model;
using Redbox.Rental.Model.Health;

namespace Redbox.Rental.Services.Controllers;

public class BaseControlPanelController : BaseViewController
{
	protected class ResultProperties
	{
		public string StoreNumber { get; set; }

		public string Response { get; set; }

		public string UserName { get; set; }

		public string Password { get; set; }

		public bool UseLdap { get; set; }

		public bool IsOnline { get; set; }

		public bool StandAlone { get; set; }
	}

	protected static ResultProperties ConvertResultProperties(IDictionary<string, object> dictionary)
	{
		ResultProperties result = null;
		if (dictionary != null)
		{
			result = new ResultProperties
			{
				StoreNumber = (dictionary.Keys.Contains("store_number") ? ((string)dictionary["store_number"]) : ""),
				Response = (dictionary.Keys.Contains("response") ? ((string)dictionary["response"]) : ""),
				UserName = (dictionary.Keys.Contains("user_name") ? ((string)dictionary["user_name"]) : ""),
				Password = (dictionary.Keys.Contains("password") ? ((string)dictionary["password"]) : ""),
				UseLdap = (dictionary.Keys.Contains("use_ldap") && (bool)dictionary["use_ldap"]),
				IsOnline = (dictionary.Keys.Contains("is_online") && (bool)dictionary["is_online"]),
				StandAlone = (dictionary.Keys.Contains("stand_alone") && (bool)dictionary["stand_alone"])
			};
		}
		return result;
	}

	protected static void UpdateUserName(string userName)
	{
		IControlPanelService service = ServiceLocator.Instance.GetService<IControlPanelService>();
		if (service != null)
		{
			service.LoggedInUser = userName;
		}
	}

	protected static void InnerSendAuditEvent(string storeNumber, string eventType, string userName, string source, string message)
	{
		ServiceLocator.Instance.GetService<IHealthServices>()?.SendAuditEvent(storeNumber, eventType, DateTime.Now, userName, message, source, null);
	}

	protected static bool SecurityCreateUser(string userName, string password, int numberOfDaysToExpire)
	{
		return ServiceLocator.Instance.GetService<ISecurityService>()?.CreateUser(userName, password, numberOfDaysToExpire) ?? false;
	}

	protected static bool SecurityAuthenticate(string userName, string password)
	{
		return ServiceLocator.Instance.GetService<ISecurityService>()?.Authenticate(userName, password) ?? false;
	}

	protected static bool SecurityRemoveUser(string userName)
	{
		return ServiceLocator.Instance.GetService<ISecurityService>()?.RemoveUser(userName) ?? false;
	}

	protected static bool SecurityUserExists(string userName)
	{
		bool isAccountLocked;
		return ServiceLocator.Instance.GetService<ISecurityService>()?.UserExists(userName, out isAccountLocked) ?? false;
	}

	protected static bool SecurityIsAccountLocked(string userName)
	{
		ISecurityService service = ServiceLocator.Instance.GetService<ISecurityService>();
		if (service == null)
		{
			return false;
		}
		service.UserExists(userName, out var isAccountLocked);
		return isAccountLocked;
	}

	protected static bool SecuritySlideExpirationDate(string userName, int numberOfDays)
	{
		return ServiceLocator.Instance.GetService<ISecurityService>()?.SlideExpirationDate(userName, numberOfDays) ?? false;
	}
}
