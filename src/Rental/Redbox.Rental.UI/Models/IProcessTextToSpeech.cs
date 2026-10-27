using Redbox.KioskEngine.ComponentModel.TextToSpeech;

namespace Redbox.Rental.UI.Models;

public interface IProcessTextToSpeech
{
	ISpeechControl ProcessGetSpeechControl();
}
