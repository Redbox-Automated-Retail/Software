using System.Collections.Generic;
using System.Text;
using Redbox.Core;
using Redbox.UpdateManager.ComponentModel;

namespace Redbox.UpdateManager.Environment;

public class RevLog : IRevLog
{
	public string Hash { get; set; }

	public string Label { get; set; }

	public List<ChangeItem> Changes { get; set; }

	public string HashFileName(string name)
	{
		string text = Encoding.ASCII.GetBytes(name).ToASCIISHA1Hash();
		LogHelper.Instance.Log("{0} generated for file: {1} in IRevLog.HashFileName", text, name);
		return $"{Hash}-{text}.patch";
	}
}
