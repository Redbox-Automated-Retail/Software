using System.Windows.Forms;
using Redbox.HAL.Common.GUI.Functions;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.MSHALTester;

internal sealed class TesterOutputBox : OutputBox
{
	private readonly Form1 TheForm;

	protected override void PostWrite(string s)
	{
		LogHelper.Instance.Log(s);
	}

	internal TesterOutputBox(ListBox box, Form1 form)
		: base(box)
	{
		TheForm = form;
	}
}
