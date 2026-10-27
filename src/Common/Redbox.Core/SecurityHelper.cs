using System.Security.Principal;

namespace Redbox.Core;

public static class SecurityHelper
{
	public static bool IsCurrentUserAnAdministrator()
	{
		WindowsIdentity current = WindowsIdentity.GetCurrent();
		if (current == null)
		{
			return false;
		}
		return new WindowsPrincipal(current).IsInRole("BUILTIN\\Administrators");
	}

	public static bool IsCurrentUserPowerUser()
	{
		WindowsIdentity current = WindowsIdentity.GetCurrent();
		if (current == null)
		{
			return false;
		}
		return new WindowsPrincipal(current).IsInRole("BUILTIN\\Power Users");
	}
}
