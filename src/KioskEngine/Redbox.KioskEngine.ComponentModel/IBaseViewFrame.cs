namespace Redbox.KioskEngine.ComponentModel;

public interface IBaseViewFrame
{
	string ViewName { get; }

	IScene Scene { get; }

	string ActiveFlag { get; }

	ErrorList Errors { get; }

	bool Clear { get; }

	bool IsMaintenanceModeView { get; set; }

	bool IsStartOverView { get; set; }

	void UpdateScene(IViewFrameInstance viewFrameInstance);

	bool RaiseOnEnter(IViewFrameInstance viewFrameInstance);

	void RaiseOnLeave(IViewFrameInstance viewFrameInstance);
}
