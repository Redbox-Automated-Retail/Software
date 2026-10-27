using Redbox.KioskEngine.ComponentModel;

namespace Redbox.KioskEngine.Environment;

public class StyleSheetStateTransition : IStyleSheetStateTransition
{
	public string Name { get; internal set; }

	public float Duration { get; internal set; }

	public TweenType TweenType { get; internal set; }
}
