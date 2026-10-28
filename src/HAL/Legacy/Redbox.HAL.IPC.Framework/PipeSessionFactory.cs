using Microsoft.Win32.SafeHandles;

namespace Redbox.HAL.IPC.Framework;

public interface PipeSessionFactory
{
	AbstractNamedPipeServerSession CreateSession(SafeFileHandle pipeHandle);
}
