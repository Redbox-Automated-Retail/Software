using System.Collections.Generic;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Script.Framework;

internal static class MerchSegmentFactory
{
	private sealed class RebalanceSegment : MerchSegment
	{
		internal override bool CanDump => false;

		protected internal override MerchFlags NextLower => MerchFlags.ThinRedeploy;

		internal RebalanceSegment(bool debug)
			: base(MerchFlags.Rebalance, debug)
		{
		}
	}

	private sealed class ThinRedeploySegment : MerchSegment
	{
		internal override bool CanDump => true;

		protected internal override MerchFlags NextHigher => MerchFlags.Rebalance;

		protected internal override MerchFlags NextLower => MerchFlags.Thin;

		internal ThinRedeploySegment(bool debug)
			: base(MerchFlags.ThinRedeploy, debug)
		{
		}
	}

	private sealed class ThinSegment : MerchSegment
	{
		internal override bool CanDump => true;

		protected internal override MerchFlags NextHigher => MerchFlags.ThinRedeploy;

		internal ThinSegment(bool debug)
			: base(MerchFlags.Thin, debug)
		{
		}
	}

	private static readonly Dictionary<MerchFlags, MerchSegment> SegmentMap = new Dictionary<MerchFlags, MerchSegment>();

	internal static MerchSegment Get(MerchFlags flags)
	{
		if (SegmentMap.ContainsKey(flags))
		{
			return SegmentMap[flags];
		}
		MerchSegment merchSegment = null;
		MerchFlags num = flags - 1;
		if ((ulong)num <= 3uL)
		{
			switch ((int)num)
			{
			case 0:
				merchSegment = new ThinSegment(ThinHelper.Debug);
				break;
			case 3:
				merchSegment = new ThinRedeploySegment(ThinHelper.Debug);
				break;
			case 2:
				merchSegment = new RebalanceSegment(ThinHelper.Debug);
				break;
			}
		}
		if (merchSegment != null)
		{
			SegmentMap[flags] = merchSegment;
		}
		return merchSegment;
	}
}
