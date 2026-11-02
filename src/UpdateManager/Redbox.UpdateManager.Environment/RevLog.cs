using System.Collections.Generic;
using System.Text;
using Redbox.Core;
using Redbox.UpdateManager.ComponentModel;

namespace Redbox.UpdateManager.Environment;

public class RevLog : IRevLog
{
	// The properties below are filled by the object initializer or the JSON reader.
	public string Hash { get; set; } = null!;

	public string Label { get; set; } = null!;

	public List<ChangeItem> Changes { get; set; } = null!;

	public string HashFileName(string name)
	{
		string text = Encoding.ASCII.GetBytes(name).ToASCIISHA1Hash();
		LogHelper.Instance.Log("{0} generated for file: {1} in IRevLog.HashFileName", text, name);
		return $"{Hash}-{text}.patch";
	}
}
