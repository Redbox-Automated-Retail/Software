using System.Windows.Forms;

namespace Redbox.HAL.Common.GUI.Functions;

public static class TextBoxExtensions
{
	public static int GetInteger(this TextBox tb, string friendlyName, OutputBox box)
	{
		if (string.IsNullOrEmpty(tb.Text))
		{
			return -1;
		}
		if (!int.TryParse(tb.Text, out var result))
		{
			box.Write($"The value '{tb.Text}' in box {friendlyName} is not an integer.");
			return -1;
		}
		return result;
	}
}
