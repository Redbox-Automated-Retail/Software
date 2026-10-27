namespace Redbox.KioskEngine.ComponentModel.TextToSpeech;

public interface IMapKeyPressToActorHit : ISpeechPartEvent
{
	string ActorName { get; set; }

	string Command { get; set; }

	string Parameter { get; set; }
}
