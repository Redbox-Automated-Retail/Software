using System;
using Redbox.KioskEngine.ComponentModel;

namespace Redbox.KioskEngine.Environment;

public class BaseViewDefinition
{
	private bool _clear = true;

	public string ViewName { get; set; }

	public bool ActiveFlag { get; set; }

	public bool IsStartOverView { get; set; }

	public bool IsMaintenanceModeView { get; set; }

	public string ControlName { get; set; }

	public Action<IViewFrameInstance> OnEnter { get; set; }

	public Action<IViewFrameInstance> OnLeave { get; set; }

	public bool Clear
	{
		get
		{
			return _clear;
		}
		set
		{
			_clear = value;
		}
	}
}
