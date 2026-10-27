using Redbox.KioskEngine.ComponentModel.TextToSpeech;

namespace Redbox.KioskEngine.Environment.TextToSpeech;

public class SpeechPartEvent : ISpeechPartEvent
{
	public string KeyCode { get; set; }

	public string Function { get; set; }

	public string Value { get; set; }

	public string Text { get; set; }
}
