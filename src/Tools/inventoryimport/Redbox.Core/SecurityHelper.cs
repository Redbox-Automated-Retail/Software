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
		WindowsPrincipal windowsPrincipal = new WindowsPrincipal(current);
		return windowsPrincipal.IsInRole("BUILTIN\\Administrators");
	}

	public static bool IsCurrentUserPowerUser()
	{
		WindowsIdentity current = WindowsIdentity.GetCurrent();
		if (current == null)
		{
			return false;
		}
		WindowsPrincipal windowsPrincipal = new WindowsPrincipal(current);
		return windowsPrincipal.IsInRole("BUILTIN\\Power Users");
	}
}
