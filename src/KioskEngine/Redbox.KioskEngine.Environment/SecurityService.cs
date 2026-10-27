using System;
using System.DirectoryServices.AccountManagement;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;

namespace Redbox.KioskEngine.Environment;

public class SecurityService : ISecurityService
{
	public static SecurityService Instance => Singleton<SecurityService>.Instance;

	public bool CreateUser(string userName, string password, int numberOfDaysToExpire)
	{
		if (string.IsNullOrEmpty(userName) || string.IsNullOrEmpty(password))
		{
			return false;
		}
		try
		{
			using PrincipalContext context = new PrincipalContext(ContextType.Machine);
			UserPrincipal userPrincipal = new UserPrincipal(context);
			userPrincipal.Name = userName;
			userPrincipal.DisplayName = userName;
			userPrincipal.SetPassword(password);
			userPrincipal.UserCannotChangePassword = true;
			userPrincipal.AccountExpirationDate = DateTime.Now.AddDays(numberOfDaysToExpire);
			userPrincipal.Save();
			return true;
		}
		catch
		{
			return false;
		}
	}

	public bool Authenticate(string userName, string password)
	{
		if (string.IsNullOrEmpty(userName) || string.IsNullOrEmpty(password))
		{
			return false;
		}
		try
		{
			using PrincipalContext principalContext = new PrincipalContext(ContextType.Machine);
			return principalContext.ValidateCredentials(userName, password);
		}
		catch
		{
			return false;
		}
	}

	public bool UserExists(string userName, out bool isAccountLocked)
	{
		isAccountLocked = false;
		if (string.IsNullOrEmpty(userName))
		{
			return false;
		}
		try
		{
			using PrincipalContext context = new PrincipalContext(ContextType.Machine);
			UserPrincipal userPrincipal = UserPrincipal.FindByIdentity(context, IdentityType.Name, userName);
			if (userPrincipal == null)
			{
				return false;
			}
			isAccountLocked = userPrincipal.IsAccountLockedOut();
		}
		catch
		{
			return false;
		}
		return true;
	}

	public bool RemoveUser(string userName)
	{
		if (string.IsNullOrEmpty(userName))
		{
			return false;
		}
		try
		{
			using PrincipalContext context = new PrincipalContext(ContextType.Machine);
			UserPrincipal userPrincipal = UserPrincipal.FindByIdentity(context, IdentityType.Name, userName);
			if (userPrincipal == null)
			{
				return false;
			}
			userPrincipal.Delete();
			return true;
		}
		catch
		{
			return false;
		}
	}

	public bool SlideExpirationDate(string userName, int numberOfDays)
	{
		if (string.IsNullOrEmpty(userName))
		{
			return false;
		}
		try
		{
			using PrincipalContext context = new PrincipalContext(ContextType.Machine);
			UserPrincipal userPrincipal = UserPrincipal.FindByIdentity(context, IdentityType.Name, userName);
			if (userPrincipal == null)
			{
				return false;
			}
			userPrincipal.AccountExpirationDate = DateTime.Now.AddDays(numberOfDays);
			userPrincipal.Save();
			return true;
		}
		catch
		{
			return false;
		}
	}

	private SecurityService()
	{
	}
}
