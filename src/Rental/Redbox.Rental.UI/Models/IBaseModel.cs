using System;
using Redbox.KioskEngine.ComponentModel.TextToSpeech;

namespace Redbox.Rental.UI.Models;

public interface IBaseModel
{
	Func<ISpeechControl> ProcessGetSpeechControl { get; set; }
}
