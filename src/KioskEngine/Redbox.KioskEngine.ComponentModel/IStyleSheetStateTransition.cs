namespace Redbox.KioskEngine.ComponentModel;

public interface IStyleSheetStateTransition
{
	string Name { get; }

	float Duration { get; }

	TweenType TweenType { get; }
}
