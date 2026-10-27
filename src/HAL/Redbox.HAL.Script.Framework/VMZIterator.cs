using System;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Script.Framework;

internal class VMZIterator : IDisposable
{
	private readonly VMZ VMZ;

	internal ILocation Location { get; private set; }

	internal bool IsEmpty => Location.ID == "EMPTY";

	internal MerchFlags Flags => Location.Flags;

	public void Dispose()
	{
		Location = null;
	}

	public override string ToString()
	{
		if (Location != null)
		{
			return $"{Location.ToString()} Flags = {Location.Flags.ToString()}";
		}
		return "NONE";
	}

	internal void ResetToFirst()
	{
		ILocation first = VMZ.First;
		ResetTo(first);
	}

	internal void ResetTo(ILocation location)
	{
		Location = location;
	}

	internal bool Up()
	{
		Location = VMZ.ComputeNextHigherLocation(Location);
		return Location != null;
	}

	internal void Invalidate()
	{
		Location = null;
	}

	internal IteratorComareResult RelativeTo(ILocation reference)
	{
		return VMZ.RelativeTo(Location, reference);
	}

	internal VMZIterator(ILocation location)
	{
		VMZ = VMZ.Instance;
		Location = location;
	}

	internal VMZIterator()
	{
		VMZ = VMZ.Instance;
		ResetToFirst();
	}
}
