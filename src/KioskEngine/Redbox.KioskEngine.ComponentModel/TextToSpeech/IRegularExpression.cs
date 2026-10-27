namespace Redbox.KioskEngine.ComponentModel.TextToSpeech;

public interface IRegularExpression
{
	string Expression { get; set; }

	string Value { get; set; }

	string Success { get; set; }

	string Failure { get; set; }

	void Clear();
}
