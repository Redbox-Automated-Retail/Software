using Redbox.Core;
using Redbox.KioskServices.Proxy.ComponentModel;

namespace Redbox.KioskServices.Proxy;

public static class AuthenticationServicesExtensions
{
	public static void AddAuthenticationServices(this ServiceLocator locator)
	{
		locator.AddService(typeof(IAuthenticationServices), new AuthenticationServices());
	}
}
