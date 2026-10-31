using log4net.Core;

namespace Redbox.TimeKeeper;

public class LoggingLevel
{
	public static readonly LevelWrapper Default = new LevelWrapper(new Level(100000, "DEFAULT", "DEFAULT"));

	public static readonly LevelWrapper Trace = new LevelWrapper(Level.Trace);

	public static readonly LevelWrapper Verbose = new LevelWrapper(Level.Verbose);
}
