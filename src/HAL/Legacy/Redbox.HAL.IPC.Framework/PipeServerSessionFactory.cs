using Microsoft.Win32.SafeHandles;
using Redbox.HAL.Core;

namespace Redbox.HAL.IPC.Framework;

internal class PipeServerSessionFactory : PipeSessionFactory
{
	internal static PipeServerSessionFactory Instance => Singleton<PipeServerSessionFactory>.Instance;

	public AbstractNamedPipeServerSession CreateSession(SafeFileHandle pipeHandle)
	{
		return new NamedPipeServerSession(pipeHandle);
	}

	private PipeServerSessionFactory()
	{
	}
}
