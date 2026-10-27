using Redbox.HAL.IPC.Framework;
using Redbox.IPC.Framework;

namespace Redbox.HAL.Client;

public class HardwareCommand : ClientCommand<HardwareCommandResult>
{
	public HardwareCommand(IPCProtocol protocol, string command)
		: base(protocol, command)
	{
	}
}
