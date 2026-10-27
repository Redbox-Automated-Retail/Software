using Redbox.Core;

namespace Redbox.Shell;

public class Options
{
	public static Options Instance => Singleton<Options>.Instance;

	private Options()
	{
	}
}
