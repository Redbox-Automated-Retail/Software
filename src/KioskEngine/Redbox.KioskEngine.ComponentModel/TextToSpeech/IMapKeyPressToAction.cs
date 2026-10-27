using System;

namespace Redbox.KioskEngine.ComponentModel.TextToSpeech;

public interface IMapKeyPressToAction : ISpeechPartEvent
{
	Action Action { get; set; }

	Func<string> ActionText { get; set; }
}
