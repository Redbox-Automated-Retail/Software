using Redbox.KioskEngine.ComponentModel.TextToSpeech;

namespace Redbox.KioskEngine.Environment.TextToSpeech;

public class RegularExpression : IRegularExpression
{
	public string Expression { get; set; }

	public string Value { get; set; }

	public string Success { get; set; }

	public string Failure { get; set; }

	public void Clear()
	{
		Expression = null;
		Value = null;
		Success = null;
		Failure = null;
	}
}
