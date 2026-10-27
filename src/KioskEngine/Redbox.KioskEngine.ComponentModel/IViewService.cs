using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace Redbox.KioskEngine.ComponentModel;

public interface IViewService
{
	int ViewStackCount { get; }

	ReadOnlyCollection<IViewFrameInstance> Stack { get; }

	string LastHitActorName { get; set; }

	object LastHitActorTag { get; set; }

	TaskScheduler UITaskScheduler { get; set; }

	event EventHandler<ViewStateChangedArgs> ViewStateChanged;

	void Reset();

	void ShowFrameInDebug();

	void Show();

	void SetAnalyticsEventProperty(string name, object value);

	string Pop();

	string Peek();

	string Peek(int index);

	IViewFrameInstance PeekViewFrame(int index);

	IViewFrameInstance PeekViewFrameByName(string viewName, out int index);

	void ClearCache(bool clearResourceViewsOnly);

	string PopDiscard();

	void EnterTopOfStock();

	string GetCurrentViewAnalyticsName();

	string GetLastShownView();

	DateTime? GetViewShownTime();

	void ResetViewShownTime();

	int GetActiveHitHandlerCount();

	void Push(string viewName, object parameter, bool isCriticalErrorView = false);

	void Push(string viewName);

	void PushCriticalErrorView(string viewName, object parameter);

	IViewFrameInstance PeekViewFrame();

	IBaseViewFrame GetViewFrame(string viewName);

	bool IsMaintenanceViewShowing();

	bool IsSafeToChangeView();

	string GetStartOverViewName();

	string GetMaintenanceViewName();

	void GoBack(object context, bool popOnly);

	void GoBackSafe(object context, bool popOnly);

	void UnwindTo(string viewName, bool discard, object context);

	void UnwindToSafe(string unwindToViewName, bool discard, object context);

	void CacheViewFrame(IBaseViewFrame baseViewFrame);
}
