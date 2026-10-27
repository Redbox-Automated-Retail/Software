using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Core;

namespace Redbox.HAL.Script.Framework;

internal sealed class SegmentManager : IGetObserver
{
	internal readonly ReadOnlyCollection<MerchFlags> IncreasingPriorityOrder;

	internal readonly ReadOnlyCollection<MerchFlags> DecreasingPriorityOrder;

	private readonly VMZ VMZ;

	private readonly bool Debug;

	private readonly MerchFlags[] IncreasingPriorityFlags = new MerchFlags[3]
	{
		MerchFlags.Thin,
		MerchFlags.ThinRedeploy,
		MerchFlags.Rebalance
	};

	internal static SegmentManager Instance => Singleton<SegmentManager>.Instance;

	public void OnStuck(IGetResult result)
	{
		ServiceLocator.Instance.GetService<IInventoryService>().UpdateEmptyStuck(result.Location);
	}

	public bool OnEmpty(IGetResult result)
	{
		result.Update(ErrorCodes.ItemStuck);
		ServiceLocator.Instance.GetService<IInventoryService>().UpdateEmptyStuck(result.Location);
		return false;
	}

	internal bool IsThinType(MerchFlags flags)
	{
		return !IsNonThinType(flags);
	}

	internal bool IsNonThinType(MerchFlags flags)
	{
		if (flags != MerchFlags.None)
		{
			return MerchFlags.Unload == flags;
		}
		return true;
	}

	internal List<ILocation> GetNonThinItemsInCompressedZone()
	{
		List<ILocation> rv = new List<ILocation>();
		ILocation location = FindHighest();
		if (location == null)
		{
			return rv;
		}
		VMZ.Instance.ForAllBetweenDo(delegate(ILocation loc)
		{
			if (loc.IsEmpty)
			{
				return true;
			}
			if (IsNonThinType(loc.Flags))
			{
				rv.Add(loc);
			}
			return true;
		}, location);
		return rv;
	}

	internal bool InCompressedZone(ILocation target)
	{
		ILocation location = FindHighest();
		if (location == null)
		{
			return false;
		}
		bool found = false;
		VMZ.Instance.ForAllBetweenDo(delegate(ILocation loc)
		{
			if (loc.Equals(target))
			{
				found = true;
				return false;
			}
			return true;
		}, location);
		return found;
	}

	internal ILocation FindHighest()
	{
		ILocation location = null;
		foreach (MerchFlags item in IncreasingPriorityOrder)
		{
			location = MerchSegmentFactory.Get(item).FindHigh();
			if (location != null)
			{
				return location;
			}
		}
		return null;
	}

	internal ILocation SelectLocation(MerchFlags flags, MerchTransferManager manager, out MerchandizeResult result)
	{
		result = MerchandizeResult.Success;
		MerchSegment merchSegment = MerchSegmentFactory.Get(flags);
		ILocation location = merchSegment.FindHigh();
		ILocation location2 = FindLastHigherPriority(merchSegment);
		bool flag = location2 != null;
		if (!flag)
		{
			location2 = VMZ.Instance.First;
		}
		VMZIterator vMZIterator = null;
		MerchandizeResult _mr = result;
		ILocation target = null;
		IInventoryService service = ServiceLocator.Instance.GetService<IInventoryService>();
		if (location != null)
		{
			VMZ.ForAllBetweenDo(delegate(ILocation loc)
			{
				if (loc.IsEmpty)
				{
					target = loc;
					return false;
				}
				if (IsThinType(loc.Flags))
				{
					return true;
				}
				if (!service.IsStuck(loc))
				{
					MerchandizeResult merchandizeResult = MoveNonThin(loc, manager);
					if (merchandizeResult == MerchandizeResult.Success)
					{
						target = loc;
						return false;
					}
					if (MerchandizeResult.EmptyStuck != merchandizeResult)
					{
						_mr = merchandizeResult;
						return false;
					}
				}
				return true;
			}, location2, location);
			result = _mr;
			if (target != null)
			{
				if (Debug)
				{
					LogHelper.Instance.Log("[SelectLocation] Found an interior location at {0} for flags {1}", target.ToString(), flags.ToString());
				}
				return target;
			}
			vMZIterator = new VMZIterator(location);
			if (!vMZIterator.Up())
			{
				result = MerchandizeResult.VMZFull;
				return null;
			}
		}
		else
		{
			vMZIterator = new VMZIterator(location2);
			if (flag && !vMZIterator.Up())
			{
				result = MerchandizeResult.VMZFull;
				return null;
			}
		}
		target = FindTarget(vMZIterator, flags, manager, out result);
		if (target != null && Debug)
		{
			LogHelper.Instance.Log("[SelectLocation] Found an interior location at {0} for flags {1}", target.ToString(), flags.ToString());
		}
		return target;
	}

	internal MerchSegment FindContainingOrHigher(VMZIterator iterator)
	{
		foreach (MerchFlags item in DecreasingPriorityOrder)
		{
			if (ThinHelper.Debug)
			{
				LogHelper.Instance.Log("[FindContainingOrHigher] Look at segment {0}", item.ToString());
			}
			MerchSegment merchSegment = MerchSegmentFactory.Get(item);
			if (merchSegment.ItemCount() == 0)
			{
				continue;
			}
			if (merchSegment.Contains(iterator.Location))
			{
				return merchSegment;
			}
			ILocation location = merchSegment.FindLow();
			if (iterator.RelativeTo(location) == IteratorComareResult.Below)
			{
				if (ThinHelper.Debug)
				{
					LogHelper.Instance.Log("[FindContainingOrHigher] Iterator at {0} is below {1}", iterator.Location.ToString(), location.ToString());
				}
				return merchSegment;
			}
		}
		return null;
	}

	internal MerchandizeResult MoveNonThin(ILocation source, ILocation outside, MerchTransferManager manager)
	{
		using IEmptySearchResult emptySearchResult = ServiceLocator.Instance.GetService<IEmptySearchPatternService>().FindEmptyOutsideOf(outside);
		if (emptySearchResult.FoundEmpty == 0)
		{
			LogHelper.Instance.Log("[SegmentManager]: There are no empty slots available for transfer.");
			return MerchandizeResult.MachineFull;
		}
		_ = source.Flags;
		ITransferResult transferResult = ServiceLocator.Instance.GetService<IControllerService>().Transfer(source, emptySearchResult.EmptyLocations, this);
		if (transferResult.Transferred)
		{
			manager.ProcessTransfer(source, transferResult.Destination);
			return MerchandizeResult.Success;
		}
		switch (transferResult.TransferError)
		{
		case ErrorCodes.SlotEmpty:
		case ErrorCodes.ItemStuck:
			return MerchandizeResult.EmptyStuck;
		case ErrorCodes.SlotInUse:
			return MerchandizeResult.TransferFailure;
		default:
			return MerchandizeResult.HardwareError;
		}
	}

	internal MerchandizeResult MoveNonThin(ILocation source, MerchTransferManager manager)
	{
		ILocation location = FindHighest();
		ILocation outside = ((location != null) ? location : source);
		return MoveNonThin(source, outside, manager);
	}

	internal MerchandizeResult MoveThin(ILocation source, ILocation empty, MerchTransferManager mgr)
	{
		ServiceLocator.Instance.GetService<IInventoryService>();
		ITransferResult transferResult = ServiceLocator.Instance.GetService<IControllerService>().Transfer(source, empty);
		if (transferResult.Transferred)
		{
			if (Debug)
			{
				LogHelper.Instance.Log("MerchMetadata: xfer of {0} [Flags = {1}] to {2} was successful.", source.ToString(), transferResult.Destination.Flags.ToString(), empty.ToString());
			}
		}
		else
		{
			LogHelper.Instance.Log("MerchMetadata: xfer of {0} [Flags = {1}] failed with error {2}", source.ToString(), source.Flags.ToString(), transferResult.TransferError.ToString());
		}
		mgr.Process(transferResult);
		return MerchandizeResult.Success;
	}

	private int Compare(MerchFlags left, MerchFlags right)
	{
		int num = IncreasingPriorityOrder.IndexOf(left);
		int num2 = IncreasingPriorityOrder.IndexOf(right);
		return num - num2;
	}

	private ILocation FindTarget(VMZIterator iterator, MerchFlags flags, MerchTransferManager manager, out MerchandizeResult result)
	{
		result = MerchandizeResult.Success;
		IInventoryService service = ServiceLocator.Instance.GetService<IInventoryService>();
		while (true)
		{
			if (iterator.IsEmpty)
			{
				return iterator.Location;
			}
			if (IsThinType(iterator.Location.Flags))
			{
				if (Compare(iterator.Location.Flags, flags) < 0)
				{
					ILocation empty = MerchSegmentFactory.Get(iterator.Location.Flags).GetEmpty();
					if (empty != null)
					{
						result = MoveThin(iterator.Location, empty, manager);
						if (result == MerchandizeResult.Success)
						{
							return iterator.Location;
						}
						if (MerchandizeResult.EmptyStuck != result)
						{
							return null;
						}
					}
					result = MerchandizeResult.VMZFull;
					break;
				}
			}
			else if (!service.IsStuck(iterator.Location))
			{
				MerchandizeResult merchandizeResult = MoveNonThin(iterator.Location, manager);
				if (merchandizeResult == MerchandizeResult.Success)
				{
					return iterator.Location;
				}
				if (MerchandizeResult.EmptyStuck != merchandizeResult)
				{
					result = merchandizeResult;
					break;
				}
			}
			if (!iterator.Up())
			{
				result = MerchandizeResult.VMZFull;
				break;
			}
		}
		return null;
	}

	private void AddTransfer(ITransferResult xferResult, ExecutionContext context)
	{
		if (xferResult.Transferred)
		{
			context.CreateResult("DiskTransferSuccessful", Instance.InCompressedZone(xferResult.Source) ? "The item was moved internally in the VMZ." : "Transferred disk from VMZ to drum.", xferResult.Source.Deck, xferResult.Source.Slot, "EMPTY", null, xferResult.Destination.ID);
			if (ServiceLocator.Instance.GetService<IDumpbinService>().IsBin(xferResult.Destination))
			{
				context.CreateResult("ThinTransferSuccessful", "The item was transferred from storage to the dump bin.", null, null, xferResult.Destination.ID, null, null);
			}
			else
			{
				context.CreateResult("DiskTransferSuccessful", Instance.InCompressedZone(xferResult.Destination) ? "The item was moved internally in the VMZ." : "Transferred disk from VMZ to drum.", xferResult.Destination.Deck, xferResult.Destination.Slot, xferResult.Destination.ID, null, "EMPTY");
			}
		}
	}

	private ILocation FindLastHigherPriority(MerchSegment start)
	{
		MerchFlags nextHigher = start.NextHigher;
		while (true)
		{
			MerchSegment merchSegment = MerchSegmentFactory.Get(nextHigher);
			if (merchSegment == null)
			{
				break;
			}
			ILocation location = merchSegment.FindHigh();
			if (location != null)
			{
				if (Debug)
				{
					LogHelper.Instance.Log("FindLastHigher: [Context = {0}]: found higher priority [Context = {1}] at {2}", start.Flags.ToString(), nextHigher.ToString(), location.ToString());
				}
				return location;
			}
			nextHigher = merchSegment.NextHigher;
		}
		if (Debug)
		{
			LogHelper.Instance.Log("FindLastHigherPriority: [Context = {0}]: no higher priority segments found.", start.Flags.ToString());
		}
		return null;
	}

	private SegmentManager()
	{
		Debug = true;
		VMZ = VMZ.Instance;
		IncreasingPriorityOrder = Array.AsReadOnly(IncreasingPriorityFlags);
		MerchFlags[] array = new MerchFlags[IncreasingPriorityFlags.Length];
		int num = IncreasingPriorityFlags.Length - 1;
		int num2 = 0;
		while (num >= 0)
		{
			array[num2] = IncreasingPriorityFlags[num];
			num--;
			num2++;
		}
		DecreasingPriorityOrder = Array.AsReadOnly(array);
	}
}
