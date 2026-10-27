using System;
using System.Threading;
using Redbox.Core;

namespace Redbox.KioskEngine.Environment;

public class LockHelper
{
	public static void TryLockObject(object lockObject, Action<bool> lockAction)
	{
		bool lockTaken = false;
		try
		{
			Monitor.TryEnter(lockObject, ref lockTaken);
			lockAction(lockTaken);
		}
		finally
		{
			if (lockTaken)
			{
				Monitor.Exit(lockObject);
			}
		}
	}

	public static void TryLockObject(object lockObject, Action lockAction, string logMethodName = null)
	{
		bool lockTaken = false;
		try
		{
			Monitor.TryEnter(lockObject, ref lockTaken);
			if (lockTaken)
			{
				lockAction();
			}
			else
			{
				LogHelper.Instance.Log(logMethodName + " lock prevented.  Only one action may execute.");
			}
		}
		finally
		{
			if (lockTaken)
			{
				Monitor.Exit(lockObject);
			}
		}
	}
}
