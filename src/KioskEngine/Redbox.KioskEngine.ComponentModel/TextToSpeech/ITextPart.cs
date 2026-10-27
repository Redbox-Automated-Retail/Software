namespace Redbox.KioskEngine.ComponentModel.TextToSpeech;

public interface ITextPart
{
	string TextAvailable { get; set; }

	string IfActorVisible { get; set; }

	string IfControlEnabled { get; set; }

	string Text { get; set; }
}
