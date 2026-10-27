using System.Globalization;
using System.Resources;

namespace Redbox.Controls.BaseControls;

internal static class SR
{
	private static ResourceManager _resourceManager = new ResourceManager("ExceptionStringTable", typeof(SR).Assembly);

	internal static ResourceManager ResourceManager => _resourceManager;

	internal static string Get(string id)
	{
		return _resourceManager.GetString(id) ?? _resourceManager.GetString("Unavailable");
	}

	internal static string Get(string id, params object[] args)
	{
		string text = _resourceManager.GetString(id);
		if (text == null)
		{
			text = _resourceManager.GetString("Unavailable");
		}
		else if (args != null && args.Length != 0)
		{
			text = string.Format(CultureInfo.CurrentCulture, text, args);
		}
		return text;
	}
}
