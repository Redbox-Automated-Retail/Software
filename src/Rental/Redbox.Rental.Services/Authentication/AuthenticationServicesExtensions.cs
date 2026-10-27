using Redbox.Core;
using Redbox.Rental.Model.Authentication;

namespace Redbox.Rental.Services.Authentication;

public static class AuthenticationServicesExtensions
{
	public static void AddAuthenticationServices(this ServiceLocator locator)
	{
		locator.AddService(typeof(IAuthenticationServices), new AuthenticationServices());
	}
}
