namespace Redbox.KioskEngine.ComponentModel;

public interface ITweenService
{
	void Reset();

	void CreateTween(string name, TweenType type, float begin, float finish, float duration, TweenChange changeHandler, TweenEnd endHandler);

	void StopTween(string name);

	void StartTween(string name);

	void RemoveTween(string name);
}
