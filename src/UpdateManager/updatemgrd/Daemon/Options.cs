using System.ComponentModel;
using Redbox.Core;
using Redbox.GetOpts;

namespace Redbox.UpdateManager.Daemon;

[Usage("updatemanagerd [--register] [--forcestart] [--unregister]")]
public class Options
{
	[Option(LongName = "register")]
	[Description("Register the Kiosk Engine Daemon service process in Windows.")]
	public bool Register;

	[Option(LongName = "unregister")]
	[Description("Unregister the Kiosk Engine Daemon service process in Windows.")]
	public bool Unregister;

	[Option(LongName = "console")]
	[Description("Start the service in console mode.")]
	public bool Console;

	[Option(LongName = "forcestart")]
	[Description("Force the service to start if register is passed.")]
	public bool ForceStart;

	public static Options Instance => Singleton<Options>.Instance;

	private Options()
	{
	}
}
