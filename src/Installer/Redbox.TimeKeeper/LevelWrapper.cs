using log4net.Core;

namespace Redbox.TimeKeeper;

public class LevelWrapper
{
	private Level _level;

	public Level Level => _level;

	public LevelWrapper(Level level)
	{
		_level = level;
	}
}
