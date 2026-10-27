using Redbox.HAL.Component.Model;

namespace Redbox.HAL.IPC.Framework.Client;

internal sealed class ClientResponse : AbstractIPCResponse
{
	private readonly ClientSession Session;

	internal MessageList Messages { get; private set; }

	protected override void OnClear()
	{
		Messages.Clear();
	}

	protected override bool OnTestResponse()
	{
		string nextBufferLine;
		do
		{
			nextBufferLine = GetNextBufferLine();
			if (nextBufferLine == string.Empty && BufferHasMoreLines())
			{
				return false;
			}
			if (LogDetails)
			{
				LogHelper.Instance.Log("[ClientResponse] Processing Message = {0}", nextBufferLine);
			}
			if (nextBufferLine.StartsWith("[MSG]"))
			{
				if (Session.SupportsEvents)
				{
					int num = nextBufferLine.IndexOf("]");
					if (num != -1)
					{
						Session.OnServerEvent(nextBufferLine.Substring(num + 1).Trim());
					}
				}
			}
			else
			{
				Messages.Add(nextBufferLine);
			}
		}
		while (!nextBufferLine.StartsWith("Welcome!") && !nextBufferLine.StartsWith("Goodbye!") && !nextBufferLine.StartsWith("545 Command") && !nextBufferLine.StartsWith("203 Command"));
		return true;
	}

	internal ClientResponse(ClientSession session)
		: this(session, logDetailed: true)
	{
	}

	internal ClientResponse(ClientSession session, bool logDetailed)
		: base(logDetailed)
	{
		Session = session;
		Messages = new MessageList();
	}
}
