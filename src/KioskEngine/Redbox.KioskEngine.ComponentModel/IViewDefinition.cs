using System;

namespace Redbox.KioskEngine.ComponentModel;

public interface IViewDefinition
{
	string ViewName { get; }

	bool ActiveFlag { get; }

	bool IsStartOverView { get; }

	bool IsMaintenanceModeView { get; }

	string ControlName { get; }

	Action<IViewFrameInstance> OnEnter { get; }

	Action<IViewFrameInstance> OnLeave { get; }

	bool Clear { get; }
}
