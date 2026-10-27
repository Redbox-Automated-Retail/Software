namespace Redbox.HAL.IPC.Framework;

internal sealed class ServerResponse : AbstractIPCResponse
{
	internal string Command { get; private set; }

	protected override void OnClear()
	{
		Command = string.Empty;
	}

	protected override bool OnTestResponse()
	{
		ReadBuilder.ToString();
		if (!BufferHasMoreLines())
		{
			while (true)
			{
				string nextBufferLine = GetNextBufferLine();
				if (nextBufferLine == string.Empty)
				{
					break;
				}
				Command = nextBufferLine;
			}
			return true;
		}
		return false;
	}

	internal ServerResponse(bool logDetails)
		: base(logDetails)
	{
		Command = string.Empty;
	}

	internal ServerResponse()
		: this(logDetails: true)
	{
	}
}
