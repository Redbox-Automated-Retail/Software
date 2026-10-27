using System;

namespace Redbox.KioskEngine.ComponentModel;

public interface IViewFrameInstance
{
	Guid Id { get; set; }

	IBaseViewFrame ViewFrame { get; set; }

	object Parameter { get; set; }
}
