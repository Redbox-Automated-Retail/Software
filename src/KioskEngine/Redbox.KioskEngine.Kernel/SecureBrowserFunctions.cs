using System.Collections.Generic;
using System.Windows.Forms;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Lua;

namespace Redbox.KioskEngine.Kernel;

internal static class SecureBrowserFunctions
{
	[KernelFunction(Name = "kernel.launchsecurebrowser")]
	internal static void LaunchSecureBrowser(LuaTable approvedUrls, NavigateUrlCallback navigateUrlCallback)
	{
		IConfigurationService service = ServiceLocator.Instance.GetService<IConfigurationService>();
		bool value = false;
		bool flag = service?.TryGetValue<bool>("system", "General", "DisableFieldVisitForm", out value) ?? false;
		Dictionary<string, string> dictionary = new Dictionary<string, string>();
		for (int i = 1; i <= approvedUrls.Keys.Count; i++)
		{
			LuaTable luaTable = (LuaTable)approvedUrls[i];
			if (!(flag & value) || !((string)luaTable["title"] == "------------Field Visit Form------------"))
			{
				dictionary[(string)luaTable["title"]] = (string)luaTable["url"];
			}
		}
		ServiceLocator.Instance.GetService<IEnvironmentNotificationService>().LaunchSecureBrowser(dictionary, navigateUrlCallback);
		approvedUrls.Dispose();
	}

	[KernelFunction(Name = "kernel.closesecurebrowser")]
	internal static void CloseSecureBrowser()
	{
		while (Application.OpenForms["SecureBrowser"] != null)
		{
			Application.OpenForms["SecureBrowser"].Close();
		}
	}

	internal static void Reset()
	{
		Application.OpenForms["SecureBrowser"]?.Close();
	}
}
