using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Security.Principal;

namespace Redbox.Core;

public class NTUserImpersonator : IDisposable
{
	private const int LOGON32_LOGON_INTERACTIVE = 2;

	private const int LOGON32_PROVIDER_DEFAULT = 0;

#if NETFRAMEWORK
	private WindowsImpersonationContext impersonationContext;
#else
	// .NET (Core) has no WindowsImpersonationContext; the thread token is set and reverted directly, which is what
	// WindowsIdentity.Impersonate() / Undo() did.
	private bool impersonating;

	[DllImport("advapi32.dll", SetLastError = true)]
	private static extern bool ImpersonateLoggedOnUser(IntPtr hToken);
#endif

	public NTUserImpersonator(string userName, string domainName, string password)
	{
		ImpersonateValidUser(userName, domainName, password);
	}

	public void Dispose()
	{
		UndoImpersonation();
	}

	[DllImport("advapi32.dll", SetLastError = true)]
	private static extern int LogonUser(string lpszUserName, string lpszDomain, string lpszPassword, int dwLogonType, int dwLogonProvider, ref IntPtr phToken);

	[DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
	private static extern int DuplicateToken(IntPtr hToken, int impersonationLevel, ref IntPtr hNewToken);

	[DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
	private static extern bool RevertToSelf();

	[DllImport("kernel32.dll", CharSet = CharSet.Auto)]
	private static extern bool CloseHandle(IntPtr handle);

	private void ImpersonateValidUser(string userName, string domain, string password)
	{
		WindowsIdentity windowsIdentity = null;
		IntPtr phToken = IntPtr.Zero;
		IntPtr hNewToken = IntPtr.Zero;
		try
		{
			if (RevertToSelf())
			{
				if (LogonUser(userName, domain, password, 2, 0, ref phToken) != 0)
				{
					if (DuplicateToken(phToken, 2, ref hNewToken) != 0)
					{
#if NETFRAMEWORK
						windowsIdentity = new WindowsIdentity(hNewToken);
						impersonationContext = windowsIdentity.Impersonate();
						return;
#else
						if (ImpersonateLoggedOnUser(hNewToken))
						{
							impersonating = true;
							return;
						}
#endif
					}
					throw new Win32Exception(Marshal.GetLastWin32Error());
				}
				throw new Win32Exception(Marshal.GetLastWin32Error());
			}
			throw new Win32Exception(Marshal.GetLastWin32Error());
		}
		finally
		{
			if (phToken != IntPtr.Zero)
			{
				CloseHandle(phToken);
			}
			if (hNewToken != IntPtr.Zero)
			{
				CloseHandle(hNewToken);
			}
		}
	}

	private void UndoImpersonation()
	{
#if NETFRAMEWORK
		if (impersonationContext != null)
		{
			impersonationContext.Undo();
		}
#else
		if (impersonating)
		{
			RevertToSelf();
			impersonating = false;
		}
#endif
	}
}
