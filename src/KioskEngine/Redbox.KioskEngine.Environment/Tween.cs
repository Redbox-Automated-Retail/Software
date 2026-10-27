using Redbox.KioskEngine.ComponentModel;

namespace Redbox.KioskEngine.Environment;

public class Tween
{
	public string Name { get; set; }

	public bool Enabled { get; set; }

	public float Time { get; set; }

	public float Begin { get; set; }

	public float Finish { get; set; }

	public float Change => Finish - Begin;

	public TweenType Type { get; set; }

	public float Duration { get; set; }

	public event TweenEnd End;

	public event TweenChange ChangeState;

	public bool Update()
	{
		if (!Enabled)
		{
			return true;
		}
		Time++;
		float newValue = TweenFunctions.ExecuteTween(Type, Time, Begin, Change, Duration);
		if (ChangeState != null)
		{
			ChangeState(Name, Time, newValue);
		}
		return Time < Duration;
	}

	public void ClearEnd()
	{
		End = null;
	}

	public void ClearChangeState()
	{
		ChangeState = null;
	}

	public void RaiseEnd()
	{
		if (End != null)
		{
			End(Name);
			Enabled = false;
			Time = 0f;
		}
	}
}
