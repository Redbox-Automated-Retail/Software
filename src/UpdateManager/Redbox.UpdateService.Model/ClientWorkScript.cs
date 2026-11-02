using System.Text;
using Redbox.Core;

namespace Redbox.UpdateService.Model;

public class ClientWorkScript
{
	public long ID { get; set; }

	// The properties below are filled by the JSON reader or by the sender's object initializer.
	public string Command { get; set; } = null!;

	public string GetScriptText()
	{
		return Encoding.Unicode.GetString(Command.Base64ToBytes());
	}
}
