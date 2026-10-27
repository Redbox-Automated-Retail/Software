using System;
using System.Collections.Generic;
using System.Reflection;
using Redbox.KioskEngine.ComponentModel;

namespace Redbox.KioskEngine.Environment;

internal static class TweenFunctions
{
	private static IDictionary<TweenType, TweenFunctor> m_functors;

	internal static IDictionary<TweenType, TweenFunctor> Functors
	{
		get
		{
			if (m_functors == null)
			{
				m_functors = new Dictionary<TweenType, TweenFunctor>();
				MethodInfo[] methods = typeof(TweenFunctions).GetMethods(BindingFlags.Static | BindingFlags.NonPublic);
				foreach (MethodInfo methodInfo in methods)
				{
					TweenFunctionAttribute tweenFunctionAttribute = (TweenFunctionAttribute)Attribute.GetCustomAttribute(methodInfo, typeof(TweenFunctionAttribute));
					if (tweenFunctionAttribute != null)
					{
						m_functors[tweenFunctionAttribute.Type] = (TweenFunctor)Delegate.CreateDelegate(typeof(TweenFunctor), methodInfo);
					}
				}
			}
			return m_functors;
		}
	}

	internal static float ExecuteTween(TweenType type, float time, float begin, float change, float duration)
	{
		if (Functors.ContainsKey(type))
		{
			return Functors[type](time, begin, change, duration);
		}
		return 0f;
	}

	[TweenFunction(Type = TweenType.Linear)]
	internal static float Linear(float time, float begin, float change, float duration)
	{
		return change * time / duration + begin;
	}

	[TweenFunction(Type = TweenType.EaseInQuad)]
	internal static float EaseInQuad(float time, float begin, float change, float duration)
	{
		return change * (time /= duration) * time + begin;
	}

	[TweenFunction(Type = TweenType.EaseOutQuad)]
	internal static float EaseOutQuad(float time, float begin, float change, float duration)
	{
		return (0f - change) * (time /= duration) + (time - 2f) + begin;
	}

	[TweenFunction(Type = TweenType.EaseInOutQuad)]
	internal static float EaseInOutQuad(float time, float begin, float change, float duration)
	{
		if ((time /= duration / 2f) < 1f)
		{
			return change / 2f * time * time + begin;
		}
		return (0f - change) / 2f * (--time * (time - 2f) - 1f) + begin;
	}

	[TweenFunction(Type = TweenType.EaseInCubic)]
	internal static float EaseInCubic(float time, float begin, float change, float duration)
	{
		return change * (time /= duration) * time * time + begin;
	}

	[TweenFunction(Type = TweenType.EaseOutCubic)]
	internal static float EaseOutCubic(float time, float begin, float change, float duration)
	{
		return change * ((time = time / duration - 1f) * time * time + 1f) + begin;
	}

	[TweenFunction(Type = TweenType.EaseInOutCubic)]
	internal static float EaseInOutCubic(float time, float begin, float change, float duration)
	{
		if ((time /= duration / 2f) < 1f)
		{
			return change / 2f * time * time * time + begin;
		}
		return change / 2f * ((time -= 2f) * time * time + 2f) + begin;
	}

	[TweenFunction(Type = TweenType.EaseInQuart)]
	internal static float EaseInQuart(float time, float begin, float change, float duration)
	{
		return change * (time /= duration) * time * time * time + begin;
	}

	[TweenFunction(Type = TweenType.EaseOutQuart)]
	internal static float EaseOutQuart(float time, float begin, float change, float duration)
	{
		return (0f - change) * ((time = time / duration - 1f) * time * time * time - 1f) + begin;
	}

	[TweenFunction(Type = TweenType.EaseInOutQuart)]
	internal static float EaseInOutQuart(float time, float begin, float change, float duration)
	{
		if ((time /= duration / 2f) < 1f)
		{
			return change / 2f * time * time * time * time + begin;
		}
		return (0f - change) / 2f * ((time -= 2f) * time * time * time - 2f) + begin;
	}

	[TweenFunction(Type = TweenType.EaseInQuint)]
	internal static float EaseInQuint(float time, float begin, float change, float duration)
	{
		return change * (time /= duration) * time * time * time * time + begin;
	}

	[TweenFunction(Type = TweenType.EaseOutQuint)]
	internal static float EaseOutQuint(float time, float begin, float change, float duration)
	{
		return change * ((time = time / duration - 1f) * time * time * time * time + 1f) + begin;
	}

	[TweenFunction(Type = TweenType.EaseInOutQuint)]
	internal static float EaseInOutQuint(float time, float begin, float change, float duration)
	{
		if ((time /= duration / 2f) < 1f)
		{
			return change / 2f * time * time * time * time * time + begin;
		}
		return change / 2f * ((time -= 2f) * time * time * time * time + 2f) + begin;
	}

	[TweenFunction(Type = TweenType.EaseInSine)]
	internal static float EaseInSine(float time, float begin, float change, float duration)
	{
		return (0f - change) * (float)Math.Cos((double)(time / duration) * 1.57) + change + begin;
	}

	[TweenFunction(Type = TweenType.EaseOutSine)]
	internal static float EaseOutSine(float time, float begin, float change, float duration)
	{
		return change * (float)Math.Sin((double)(time / duration) * 1.57) + begin;
	}

	[TweenFunction(Type = TweenType.EaseInOutSine)]
	internal static float EaseInOutSine(float time, float begin, float change, float duration)
	{
		return (0f - change) / 2f * (float)(Math.Cos(3.14 * (double)time / (double)duration) - 1.0) + begin;
	}

	[TweenFunction(Type = TweenType.EaseInBounce)]
	internal static float EaseInBounce(float time, float begin, float change, float duration)
	{
		return change - EaseInOutBounce(duration - time, 0f, change, duration) + begin;
	}

	[TweenFunction(Type = TweenType.EaseOutBounce)]
	internal static float EaseOutBounce(float time, float begin, float change, float duration)
	{
		if ((double)(time /= duration) < 0.36363636363636365)
		{
			return change * (7.5625f * time * time) + begin;
		}
		if ((double)time < 0.7272727272727273)
		{
			return change * (7.5625f * (time -= 0.54545456f) * time + 0.75f) + begin;
		}
		if ((double)time < 0.9090909090909091)
		{
			return change * (7.5625f * (time -= 0.8181818f) * time + 0.9375f) + begin;
		}
		return change * (7.5625f * (time -= 21f / 22f) * time + 63f / 64f) + begin;
	}

	[TweenFunction(Type = TweenType.EaseInOutBounce)]
	internal static float EaseInOutBounce(float time, float begin, float change, float duration)
	{
		if (time < duration / 2f)
		{
			return EaseInBounce(time * 2f, 0f, change, duration) * 0.5f + begin;
		}
		return EaseOutBounce(time * 2f - duration, 0f, change, duration) * 0.5f + change * 0.5f + begin;
	}
}
