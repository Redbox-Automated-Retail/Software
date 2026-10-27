using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;

namespace Redbox.KioskEngine.Kernel;

internal static class SecurityServiceFunctions
{
	[KernelFunction(Name = "kernel.securitycreateuser")]
	internal static bool CreateUser(string userName, string password, int numberOfDaysToExpire)
	{
		return ServiceLocator.Instance.GetService<ISecurityService>()?.CreateUser(userName, password, numberOfDaysToExpire) ?? false;
	}

	[KernelFunction(Name = "kernel.securityauthenticate")]
	internal static bool Authenticate(string userName, string password)
	{
		return ServiceLocator.Instance.GetService<ISecurityService>()?.Authenticate(userName, password) ?? false;
	}

	[KernelFunction(Name = "kernel.securityremoveuser")]
	internal static bool RemoveUser(string userName)
	{
		return ServiceLocator.Instance.GetService<ISecurityService>()?.RemoveUser(userName) ?? false;
	}

	[KernelFunction(Name = "kernel.securityuserexists")]
	internal static bool UserExists(string userName)
	{
		bool isAccountLocked;
		return ServiceLocator.Instance.GetService<ISecurityService>()?.UserExists(userName, out isAccountLocked) ?? false;
	}

	[KernelFunction(Name = "kernel.securityisaccountlocked")]
	internal static bool IsAccountLocked(string userName)
	{
		ISecurityService service = ServiceLocator.Instance.GetService<ISecurityService>();
		if (service == null)
		{
			return false;
		}
		service.UserExists(userName, out var isAccountLocked);
		return isAccountLocked;
	}

	[KernelFunction(Name = "kernel.securityuserslideexpirydate")]
	internal static bool SlideExpirationDate(string userName, int numberOfDays)
	{
		return ServiceLocator.Instance.GetService<ISecurityService>()?.SlideExpirationDate(userName, numberOfDays) ?? false;
	}
}
