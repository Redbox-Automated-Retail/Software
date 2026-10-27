using System.Collections.Generic;

namespace Redbox.KioskEngine.ComponentModel.TextToSpeech;

public interface ISpeechControl
{
	string Name { get; set; }

	List<ISpeechPart> SpeechParts { get; }
}
