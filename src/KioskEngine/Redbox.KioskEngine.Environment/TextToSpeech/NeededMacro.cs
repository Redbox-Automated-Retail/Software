using Redbox.KioskEngine.ComponentModel;

namespace Redbox.KioskEngine.Environment.TextToSpeech;

public class NeededMacro : INeededMacro
{
	public string Name { get; set; }

	public object Default { get; set; }
}
