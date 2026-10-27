using System.Drawing;

namespace Redbox.KioskEngine.ComponentModel;

public class MeasureTextResult
{
	public SizeF Size { get; private set; }

	public int CharactersFitted { get; private set; }

	public int LinesFilled { get; private set; }

	public MeasureTextResult(SizeF size, int charactersFitted, int linesFilled)
	{
		Size = size;
		CharactersFitted = charactersFitted;
		LinesFilled = linesFilled;
	}
}
