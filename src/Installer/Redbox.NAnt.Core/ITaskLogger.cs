using NAnt.Core;

namespace Redbox.NAnt.Core;

public interface ITaskLogger
{
	void Log(Level messageLevel, string message, params object[] p);
}
