using System;
using System.Threading;

namespace Redbox.Core;

internal static class Redbox_002EMacros1309_002ERetryableAction
{
	private static Redbox_002EMacros1309_002EIRethrowEvaluator DefaultRethrowEvaluator { get; set; }

	private static Redbox_002EMacros1309_002EISleepEvaluator DefaultSleepEvaluator { get; set; }

	public static void Execute(Action action)
	{
		Execute(action, DefaultRethrowEvaluator, DefaultSleepEvaluator);
	}

	public static void Execute(Action action, Redbox_002EMacros1309_002EIRethrowEvaluator rethrowEvaluator)
	{
		Execute(action, rethrowEvaluator, DefaultSleepEvaluator);
	}

	public static void Execute(Action action, Redbox_002EMacros1309_002EIRethrowEvaluator rethrowEvaluator, Redbox_002EMacros1309_002EISleepEvaluator sleepEvaluator)
	{
		int num = 0;
		while (true)
		{
			try
			{
				action();
				break;
			}
			catch (Exception exception)
			{
				if (rethrowEvaluator.Rethrow(num, exception))
				{
					throw;
				}
				Thread.Sleep(sleepEvaluator.SleepMilliseconds(num, exception));
			}
			num++;
		}
	}

	public static TResult Execute<TResult>(Func<TResult> function, Redbox_002EMacros1309_002EIRethrowEvaluator rethrowEvaluator, Redbox_002EMacros1309_002EISleepEvaluator sleepEvaluator)
	{
		int num = 0;
		while (true)
		{
			try
			{
				return function();
			}
			catch (Exception exception)
			{
				if (rethrowEvaluator.Rethrow(num, exception))
				{
					throw;
				}
				Thread.Sleep(sleepEvaluator.SleepMilliseconds(num, exception));
			}
			num++;
		}
	}

	public static TResult Execute<T1, TResult>(Func<T1, TResult> function, T1 argument1, Redbox_002EMacros1309_002EIRethrowEvaluator rethrowEvaluator, Redbox_002EMacros1309_002EISleepEvaluator sleepEvaluator)
	{
		int num = 0;
		while (true)
		{
			try
			{
				return function(argument1);
			}
			catch (Exception exception)
			{
				if (rethrowEvaluator.Rethrow(num, exception))
				{
					throw;
				}
				Thread.Sleep(sleepEvaluator.SleepMilliseconds(num, exception));
			}
			num++;
		}
	}

	public static TResult Execute<T1, T2, TResult>(Func<T1, T2, TResult> function, T1 argument1, T2 argument2, Redbox_002EMacros1309_002EIRethrowEvaluator rethrowEvaluator, Redbox_002EMacros1309_002EISleepEvaluator sleepEvaluator)
	{
		int num = 0;
		while (true)
		{
			try
			{
				return function(argument1, argument2);
			}
			catch (Exception exception)
			{
				if (rethrowEvaluator.Rethrow(num, exception))
				{
					throw;
				}
				Thread.Sleep(sleepEvaluator.SleepMilliseconds(num, exception));
			}
			num++;
		}
	}

	public static TResult Execute<T1, T2, T3, TResult>(Func<T1, T2, T3, TResult> function, T1 argument1, T2 argument2, T3 argument3, Redbox_002EMacros1309_002EIRethrowEvaluator rethrowEvaluator, Redbox_002EMacros1309_002EISleepEvaluator sleepEvaluator)
	{
		int num = 0;
		while (true)
		{
			try
			{
				return function(argument1, argument2, argument3);
			}
			catch (Exception exception)
			{
				if (rethrowEvaluator.Rethrow(num, exception))
				{
					throw;
				}
				Thread.Sleep(sleepEvaluator.SleepMilliseconds(num, exception));
			}
			num++;
		}
	}

	public static TResult Execute<T1, T2, T3, T4, T5, TResult>(Redbox_002EMacros1309_002EFunc<T1, T2, T3, T4, T5, TResult> function, T1 argument1, T2 argument2, T3 argument3, T4 argument4, T5 argument5, Redbox_002EMacros1309_002EIRethrowEvaluator rethrowEvaluator, Redbox_002EMacros1309_002EISleepEvaluator sleepEvaluator)
	{
		int num = 0;
		while (true)
		{
			try
			{
				return function(argument1, argument2, argument3, argument4, argument5);
			}
			catch (Exception exception)
			{
				if (rethrowEvaluator.Rethrow(num, exception))
				{
					throw;
				}
				Thread.Sleep(sleepEvaluator.SleepMilliseconds(num, exception));
			}
			num++;
		}
	}

	static Redbox_002EMacros1309_002ERetryableAction()
	{
		DefaultRethrowEvaluator = new Redbox_002EMacros1309_002ECustomRethrowEvaluator((int i, Exception e) => i == 4);
		DefaultSleepEvaluator = new Redbox_002EMacros1309_002ERandomExponentialSleepEvaluator(5);
	}
}
