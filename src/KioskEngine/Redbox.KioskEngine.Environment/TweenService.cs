using System.Collections.Generic;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;

namespace Redbox.KioskEngine.Environment;

public class TweenService : ITweenService
{
	private IDictionary<string, Tween> m_tweens;

	private readonly object m_syncObject = new object();

	public static TweenService Instance => Singleton<TweenService>.Instance;

	internal IDictionary<string, Tween> Tweens
	{
		get
		{
			if (m_tweens == null)
			{
				m_tweens = new Dictionary<string, Tween>();
			}
			return m_tweens;
		}
	}

	public void Reset()
	{
		lock (m_syncObject)
		{
			Tweens.Clear();
		}
	}

	public void CreateTween(string name, TweenType type, float begin, float finish, float duration, TweenChange changeHandler, TweenEnd endHandler)
	{
		lock (m_syncObject)
		{
			if (Tweens.ContainsKey(name))
			{
				Tween tween = Tweens[name];
				tween.Time = 0f;
				tween.Enabled = true;
				tween.Type = type;
				tween.Begin = begin;
				tween.Finish = finish;
				tween.Duration = duration;
				if (endHandler != null)
				{
					tween.ClearEnd();
					tween.End += endHandler;
				}
				if (changeHandler != null)
				{
					tween.ClearChangeState();
					tween.ChangeState += changeHandler;
				}
			}
			else
			{
				Tween tween2 = new Tween
				{
					Time = 0f,
					Type = type,
					Name = name,
					Begin = begin,
					Finish = finish,
					Duration = duration
				};
				if (endHandler != null)
				{
					tween2.End += endHandler;
				}
				if (changeHandler != null)
				{
					tween2.ChangeState += changeHandler;
				}
				Tweens[name] = tween2;
			}
		}
	}

	public void StopTween(string name)
	{
		lock (m_syncObject)
		{
			if (Tweens.ContainsKey(name))
			{
				Tweens[name].Enabled = false;
			}
		}
	}

	public void StartTween(string name)
	{
		lock (m_syncObject)
		{
			if (Tweens.ContainsKey(name))
			{
				Tweens[name].Enabled = true;
			}
		}
	}

	public void RemoveTween(string name)
	{
		lock (m_syncObject)
		{
			if (Tweens.ContainsKey(name))
			{
				Tweens[name].ClearChangeState();
				Tweens[name].ClearEnd();
				Tweens.Remove(name);
			}
		}
	}

	public void UpdateTweens()
	{
		lock (m_syncObject)
		{
			List<Tween> list = new List<Tween>(Tweens.Values);
			for (int i = 0; i < list.Count && i < list.Count; i++)
			{
				if (!list[i].Update())
				{
					list[i].RaiseEnd();
					list[i].Enabled = false;
				}
			}
		}
	}

	private TweenService()
	{
	}
}
