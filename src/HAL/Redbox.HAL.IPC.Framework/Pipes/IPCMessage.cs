using System;
using System.Text;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.IPC.Framework.Pipes;

internal sealed class IPCMessage : IIPCMessage
{
	public MessageType Type { get; internal set; }

	public MessageSeverity Severity { get; internal set; }

	public string Message { get; internal set; }

	public Guid UID { get; internal set; }

	public DateTime Timestamp { get; internal set; }

	public override string ToString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendLine(" IPC Message ");
		stringBuilder.AppendFormat("   Type       = {0}{1}", Type.ToString(), Environment.NewLine);
		stringBuilder.AppendFormat("   Severity   = {0}{1}", Severity.ToString(), Environment.NewLine);
		stringBuilder.AppendFormat("   Message    = {0}{1}", Message, Environment.NewLine);
		stringBuilder.AppendFormat("   UID        = {0}{1}", UID.ToString(), Environment.NewLine);
		stringBuilder.AppendFormat("   Timestamp  = {0}{1}", Timestamp.ToString(), Environment.NewLine);
		return stringBuilder.ToString();
	}
}
