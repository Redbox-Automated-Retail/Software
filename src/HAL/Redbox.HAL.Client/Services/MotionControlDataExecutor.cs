using System.Collections.Generic;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Component.Model.Extensions;

namespace Redbox.HAL.Client.Services;

public sealed class MotionControlDataExecutor : JobExecutor
{
	protected override string JobName => "motion-control-get-data";

	public IControllerPosition Position { get; private set; }

	public IMotionControlLimitResponse Limits { get; private set; }

	public string CurrentLocation { get; private set; }

	protected override void OnJobCompleted()
	{
		if (HardwareJobStatus.Completed != base.EndStatus)
		{
			return;
		}
		List<IMotionControlLimit> list = new List<IMotionControlLimit>();
		bool ok = true;
		bool readOk = true;
		int? x = null;
		int? y = null;
		foreach (ProgramResult result3 in base.Results)
		{
			if (result3.Code == "PositionReadError")
			{
				ok = false;
				continue;
			}
			if (result3.Code == "PositionX")
			{
				if (int.TryParse(result3.Message, out var result))
				{
					x = result;
				}
				continue;
			}
			if (result3.Code == "PositionY")
			{
				if (int.TryParse(result3.Message, out var result2))
				{
					y = result2;
				}
				continue;
			}
			if (result3.Code == "LimitReadError")
			{
				readOk = false;
				continue;
			}
			if (result3.Code == "CurrentLocation")
			{
				CurrentLocation = result3.Message;
				continue;
			}
			MotionControlLimits l = Enum<MotionControlLimits>.Parse(result3.Code, MotionControlLimits.None);
			if (l != MotionControlLimits.None)
			{
				IMotionControlLimit motionControlLimit = list.Find((IMotionControlLimit each) => each.Limit == l);
				if (motionControlLimit == null)
				{
					list.Add(new ClientControlLimit(l, result3.Message == "BLOCKED"));
				}
			}
		}
		Position = new ClientControllerPosition(ok, x, y);
		Limits = new LimitResponse(readOk, list.ToArray());
	}

	public MotionControlDataExecutor(HardwareService service)
		: base(service)
	{
	}
}
