using Redbox.KioskEngine.ComponentModel.TextToSpeech;

namespace Redbox.KioskEngine.Environment.TextToSpeech;

public class TextPart : ITextPart
{
	public string TextAvailable { get; set; }

	public string IfActorVisible { get; set; }

	public string IfControlEnabled { get; set; }

	public string Text { get; set; }
}
