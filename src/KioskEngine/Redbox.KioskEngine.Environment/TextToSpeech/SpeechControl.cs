using System.Collections.Generic;
using Redbox.KioskEngine.ComponentModel.TextToSpeech;

namespace Redbox.KioskEngine.Environment.TextToSpeech;

public class SpeechControl : ISpeechControl
{
	private List<ISpeechPart> _speechParts = new List<ISpeechPart>();

	public string Name { get; set; }

	public List<ISpeechPart> SpeechParts => _speechParts;
}
