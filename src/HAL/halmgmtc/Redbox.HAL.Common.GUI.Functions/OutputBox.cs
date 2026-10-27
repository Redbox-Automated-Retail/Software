using System.Windows.Forms;

namespace Redbox.HAL.Common.GUI.Functions;

public class OutputBox
{
	private delegate void WriteCallback(string s);

	private readonly ListBox TheBox;

	public void Write(string msg)
	{
		if (TheBox.InvokeRequired)
		{
			TheBox.Invoke(new WriteCallback(WriteToOutput), msg);
		}
		else
		{
			WriteToOutput(msg);
		}
	}

	public void Write(string fmt, params object[] stuff)
	{
		Write(string.Format(fmt, stuff));
	}

	public void Clear()
	{
		TheBox.Items.Clear();
	}

	protected OutputBox(ListBox box)
	{
		TheBox = box;
	}

	protected virtual void PostWrite(string s)
	{
	}

	protected virtual string PrewriteFormat(string msg)
	{
		return msg;
	}

	private void WriteToOutput(string msg)
	{
		TheBox.Items.Insert(0, PrewriteFormat(msg));
		PostWrite(msg);
	}
}
