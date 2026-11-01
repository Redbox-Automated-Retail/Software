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
		List<IMotionControlLimit> motionControlLimits = new List<IMotionControlLimit>();
		bool ok = true;
		bool readOk = true;
		int? x = null;
		int? y = null;
		foreach (ProgramResult programResult in base.Results)
		{
			if (programResult.Code == "PositionReadError")
			{
				ok = false;
				continue;
			}
			if (programResult.Code == "PositionX")
			{
				if (int.TryParse(programResult.Message, out var parsedX))
				{
					x = parsedX;
				}
				continue;
			}
			if (programResult.Code == "PositionY")
			{
				if (int.TryParse(programResult.Message, out var parsedY))
				{
					y = parsedY;
				}
				continue;
			}
			if (programResult.Code == "LimitReadError")
			{
				readOk = false;
				continue;
			}
			if (programResult.Code == "CurrentLocation")
			{
				CurrentLocation = programResult.Message;
				continue;
			}
			MotionControlLimits l = Enum<MotionControlLimits>.Parse(programResult.Code, MotionControlLimits.None);
			if (l != MotionControlLimits.None)
			{
				IMotionControlLimit motionControlLimit = motionControlLimits.Find((IMotionControlLimit each) => each.Limit == l);
				if (motionControlLimit == null)
				{
					motionControlLimits.Add(new ClientControlLimit(l, programResult.Message == "BLOCKED"));
				}
			}
		}
		Position = new ClientControllerPosition(ok, x, y);
		Limits = new LimitResponse(readOk, motionControlLimits.ToArray());
	}

	public MotionControlDataExecutor(HardwareService service)
		: base(service)
	{
	}
}
