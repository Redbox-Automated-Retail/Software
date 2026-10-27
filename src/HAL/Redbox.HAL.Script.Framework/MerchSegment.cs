using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Script.Framework;

internal abstract class MerchSegment
{
	internal virtual bool CanDump => false;

	internal MerchFlags Flags { get; private set; }

	internal bool Debug { get; private set; }

	protected internal virtual MerchFlags NextHigher => MerchFlags.None;

	protected internal virtual MerchFlags NextLower => MerchFlags.None;

	protected MerchSegment(MerchFlags flags, bool debug)
	{
		Flags = flags;
		Debug = debug;
		if (Debug)
		{
			LogHelper.Instance.Log("Merchdata dump for [Context = {0}]", Flags.ToString());
			LogHelper.Instance.Log(" Lower priority: {0} Higher Priority = {1}", NextLower.ToString(), NextHigher.ToString());
		}
	}

	internal int ItemCount()
	{
		ILocation location = FindHigh();
		int count = 0;
		if (location == null)
		{
			return count;
		}
		ILocation low = FindLow();
		VMZ.Instance.ForAllBetweenDo(delegate(ILocation loc)
		{
			if (loc.Flags == Flags)
			{
				int num = count + 1;
				count = num;
			}
			return true;
		}, low, location);
		return count;
	}

	internal bool Contains(ILocation location)
	{
		ILocation location2 = FindHigh();
		if (location2 == null)
		{
			return false;
		}
		ILocation low = FindLow();
		bool found = false;
		VMZ.Instance.ForAllBetweenDo(delegate(ILocation loc)
		{
			if (location.Equals(loc))
			{
				found = true;
				return false;
			}
			return true;
		}, low, location2);
		return found;
	}

	internal ILocation GetEmpty()
	{
		ILocation empty = null;
		ILocation location = FindHigh();
		if (location == null)
		{
			return empty;
		}
		ILocation low = FindLow();
		VMZ.Instance.ForAllBetweenDo(delegate(ILocation loc)
		{
			if (loc.IsEmpty)
			{
				empty = loc;
				return false;
			}
			return true;
		}, low, location);
		return empty;
	}

	internal ILocation FindHigh()
	{
		return VMZ.Instance.FindHighestLocation(Flags);
	}

	internal ILocation FindLow()
	{
		return VMZ.Instance.FindFirstLocation(Flags);
	}
}
