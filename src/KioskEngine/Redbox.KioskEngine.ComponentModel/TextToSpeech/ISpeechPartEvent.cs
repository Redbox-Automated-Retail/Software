namespace Redbox.KioskEngine.ComponentModel.TextToSpeech;

public interface ISpeechPartEvent
{
	string KeyCode { get; set; }

	string Function { get; set; }

	string Value { get; set; }

	string Text { get; set; }
}
