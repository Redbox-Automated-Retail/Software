namespace Redbox.KioskEngine.ComponentModel.TextToSpeech;

public interface ITextToSpeechControl
{
	void ExecuteCommand(string command, string parameter);

	bool IsControlVisible(string controlName);

	bool IsControlEnabled(string controlName);

	string ControlText(string controlName);

	void HandleWPFHit();

	ISpeechControl GetSpeechControl();
}
