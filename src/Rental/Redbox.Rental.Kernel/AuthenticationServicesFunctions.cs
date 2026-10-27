using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.ComponentModel.KioskServices;
using Redbox.Lua;
using Redbox.Rental.Model;
using Redbox.Rental.Model.Authentication;

namespace Redbox.Rental.Kernel;

public static class AuthenticationServicesFunctions
{
	[KernelFunction(Name = "kernel.authsvcs_authenticateuser")]
	internal static void AuthenticateUser(string storeNumber, string userName, string password, bool useLdapAuthentication, LuaFunction authenticationCallback)
	{
		IAuthenticationServices service = ServiceLocator.Instance.GetService<IAuthenticationServices>();
		if (service == null)
		{
			return;
		}
		ICallbackService callbackService = ServiceLocator.Instance.GetService<ICallbackService>();
		if (callbackService != null)
		{
			service.Authenticate(storeNumber, userName, password, useLdapAuthentication, delegate(IRemoteServiceResult r)
			{
				ICallbackService callbackService2 = callbackService;
				ResultCallbackEntry resultCallbackEntry = new ResultCallbackEntry
				{
					Function = authenticationCallback
				};
				object[] parameters = new LuaTable[1] { r.AsTable() };
				resultCallbackEntry.Parameters = parameters;
				callbackService2.EnqueueCallback(resultCallbackEntry);
			});
		}
	}
}
