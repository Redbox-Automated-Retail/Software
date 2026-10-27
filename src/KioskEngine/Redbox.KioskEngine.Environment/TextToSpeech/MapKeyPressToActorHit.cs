using Redbox.KioskEngine.ComponentModel.TextToSpeech;

namespace Redbox.KioskEngine.Environment.TextToSpeech;

public class MapKeyPressToActorHit : SpeechPartEvent, IMapKeyPressToActorHit, ISpeechPartEvent
{
	public string ActorName { get; set; }

	public string Command { get; set; }

	public string Parameter { get; set; }
}
