namespace Redbox.Rental.UI.Models;

public class ButtonData
{
	public string Text { get; set; }

	public object Tag { get; set; }

	public bool IsEnabled { get; set; }

	public DynamicRoutedCommand Command { get; set; }

	public string TtsText { get; set; }

	public ButtonData(string text, DynamicRoutedCommand command, object tag = null, bool isEnabled = true, string ttsText = null)
	{
		Text = text;
		Tag = tag;
		Command = command;
		IsEnabled = isEnabled;
		TtsText = ttsText;
	}
}
