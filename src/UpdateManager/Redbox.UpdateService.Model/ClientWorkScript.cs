using System.Text;
using Redbox.Core;

namespace Redbox.UpdateService.Model;

public class ClientWorkScript
{
	public long ID { get; set; }

	public string Command { get; set; }

	public string GetScriptText()
	{
		return Encoding.Unicode.GetString(Command.Base64ToBytes());
	}
}
