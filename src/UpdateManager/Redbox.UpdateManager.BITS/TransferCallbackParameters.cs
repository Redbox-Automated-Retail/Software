using System.Collections.Generic;
using Redbox.UpdateManager.ComponentModel;

namespace Redbox.UpdateManager.BITS;

public class TransferCallbackParameters : ITransferCallbackParameters
{
	public string Path { get; set; }

	public string Executable { get; set; }

	public List<string> Arguments { get; set; }
}
