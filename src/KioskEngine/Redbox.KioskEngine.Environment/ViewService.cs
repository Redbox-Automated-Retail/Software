using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Rental.Model;
using Redbox.Rental.Model.Analytics;
using Redbox.Rental.Model.KioskHealth;

namespace Redbox.KioskEngine.Environment;

public class ViewService : IViewService
{
	private string _startOverViewName;

	private string _maintenanceViewName;

	private string m_lastShownView;

	private DateTime? m_viewShownOn;

	private readonly IDictionary<string, IBaseViewFrame> m_viewFrameCache = new Dictionary<string, IBaseViewFrame>();

	private bool m_isCriticalErrorViewShown;

	private bool m_isCriticalErrorViewOnStack;

	public static ViewService Instance => Singleton<ViewService>.Instance;

	public TaskScheduler UITaskScheduler { get; set; }

	public int ViewStackCount => InnerStack.Count;

	public ReadOnlyCollection<IViewFrameInstance> Stack
	{
		get
		{
			List<IViewFrameInstance> list = new List<IViewFrameInstance>();
			foreach (IViewFrameInstance item in InnerStack)
			{
				list.Add(item);
			}
			return list.AsReadOnly();
		}
	}

	public string LastHitActorName { get; set; }

	public object LastHitActorTag { get; set; }

	internal ViewStack InnerStack { get; private set; }

	public event EventHandler<ViewStateChangedArgs> ViewStateChanged;

	public void Reset()
	{
		InnerStack.Clear();
		ClearCache(clearResourceViewsOnly: true);
		m_isCriticalErrorViewShown = false;
		m_isCriticalErrorViewOnStack = false;
		RaiseViewStateChanged(new ViewStateChangedArgs
		{
			Type = ViewStateChangedType.Reset
		});
	}

	public void ShowFrameInDebug()
	{
		ServiceLocator.Instance.GetService<IRenderingService>();
		IViewFrameInstance viewFrameInstance = InnerStack.Peek();
		LogShowViewFrameInstance(viewFrameInstance);
		viewFrameInstance.ViewFrame.UpdateScene(viewFrameInstance);
		if (viewFrameInstance.ViewFrame.Errors.ContainsError())
		{
			ErrorForm errorForm = new ErrorForm();
			errorForm.Errors = viewFrameInstance?.ViewFrame?.Errors;
			errorForm.Text = $"Errors for view resource: {viewFrameInstance?.ViewFrame?.ViewName}";
			errorForm.Show();
			errorForm.BringToFront();
		}
	}

	private void LogShowViewFrameInstance(IViewFrameInstance viewFrameInstance)
	{
		if (viewFrameInstance?.ViewFrame is IViewFrame viewFrame)
		{
			LogHelper.Instance.Log("Showing view '{0}' from TOS: clear = {1}, viewWindow = {2}, width = {3}, height = {4}, scene = {5}, onEnter = {6}, onLeave = {7}.", viewFrame.ViewName, viewFrame.Clear, viewFrame.ViewWindow, viewFrame.Width, viewFrame.Height, viewFrame.SceneName, viewFrame.OnEnterResourceName, viewFrame.OnLeaveResourceName);
		}
		else if (viewFrameInstance?.ViewFrame is IWPFViewFrame iWPFViewFrame)
		{
			LogHelper.Instance.Log("Showing view '{0}' from TOS", iWPFViewFrame.ViewName);
		}
	}

	public void Show()
	{
		IApplicationState service = ServiceLocator.Instance.GetService<IApplicationState>();
		if ((service != null && service.IsInMaintenanceMode) || !m_isCriticalErrorViewShown)
		{
			IResourceBundleService service2 = ServiceLocator.Instance.GetService<IResourceBundleService>();
			IShoppingSession shoppingSession = null;
			IShoppingSessionService service3 = ServiceLocator.Instance.GetService<IShoppingSessionService>();
			if (service3 != null)
			{
				shoppingSession = service3.GetCurrentSession();
				if (shoppingSession != null)
				{
					shoppingSession.ViewsShown++;
				}
			}
			if (shoppingSession != null && m_viewShownOn.HasValue)
			{
				m_lastShownView = null;
			}
			IRenderingService service4 = ServiceLocator.Instance.GetService<IRenderingService>();
			if (InnerStack.Peek() == null)
			{
				LogHelper.Instance.Log("View stack is empty.");
				service2.Filter["view_name"] = null;
				service4.ActiveScene.SuspendDrawing();
				service4.ActiveScene.Clear();
				service4.ActiveScene.ResumeDrawing();
				shoppingSession?.AddEvent(ShoppingSessionEventType.ShowView, "View stack empty.");
				return;
			}
			IViewFrameInstance viewFrameInstance = InnerStack.Peek();
			service2.Filter["view_name"] = viewFrameInstance?.ViewFrame?.ViewName;
			LogShowViewFrameInstance(viewFrameInstance);
			viewFrameInstance.ViewFrame.UpdateScene(viewFrameInstance);
			if (shoppingSession != null)
			{
				ServiceLocator.Instance.GetService<IAnalyticsService>()?.AddView(GetCurrentViewAnalyticsName());
			}
			if (!viewFrameInstance.ViewFrame.RaiseOnEnter(viewFrameInstance))
			{
				LogHelper.Instance.Log("No on enter script specified for view named: {0}", viewFrameInstance.ViewFrame.ViewName);
			}
			m_viewShownOn = DateTime.Now;
			m_lastShownView = viewFrameInstance.ViewFrame.ViewName;
			TTSRunSpeech(viewFrameInstance);
			ServiceLocator.Instance.GetService<IViewHealth>()?.PostActivity("ShowView", "", shoppingSession?.Id.ToString());
			ShowErrors(viewFrameInstance);
			if (m_isCriticalErrorViewOnStack)
			{
				m_isCriticalErrorViewShown = true;
			}
		}
		else
		{
			LogHelper.Instance.Log("View not shown due to critical error mode.");
		}
	}

	public void SetAnalyticsEventProperty(string name, object value)
	{
		ServiceLocator.Instance.GetService<IAnalyticsService>()?.AddEventProperty(name, value);
	}

	private void TTSRunSpeech(IViewFrameInstance viewFrameInstance)
	{
		ITextToSpeechService service = ServiceLocator.Instance.GetService<ITextToSpeechService>();
		if (service != null && service.TTSEnabled)
		{
			service.RunSpeechWorkflow("tts_" + viewFrameInstance.ViewFrame.ViewName);
		}
	}

	private void ShowErrors(IViewFrameInstance viewFrameInstance)
	{
		if (viewFrameInstance.ViewFrame.Errors.ContainsError())
		{
			ErrorForm errorForm = new ErrorForm();
			errorForm.Errors = viewFrameInstance.ViewFrame.Errors;
			errorForm.Text = $"Errors for view resource: {viewFrameInstance.ViewFrame.ViewName}";
			errorForm.Show();
			errorForm.BringToFront();
			RaiseViewStateChanged(new ViewStateChangedArgs
			{
				Type = ViewStateChangedType.ShowErrors
			});
		}
	}

	public string Pop()
	{
		m_isCriticalErrorViewOnStack = false;
		m_isCriticalErrorViewShown = false;
		if (InnerStack.Peek() == null)
		{
			LogHelper.Instance.Log("View stack is empty; nothing to pop.");
			return null;
		}
		IViewFrameInstance viewFrameInstance = InnerStack.Pop();
		if (viewFrameInstance.ViewFrame is IViewFrame viewFrame)
		{
			LogHelper.Instance.Log("Pop view '{0}' from TOS; executing onLeave = {1}", viewFrameInstance.ViewFrame.ViewName, viewFrame.OnLeaveResourceName);
		}
		viewFrameInstance.ViewFrame.RaiseOnLeave(viewFrameInstance);
		LogHelper.Instance.Log(ViewStackContentsToString());
		IViewFrameInstance viewFrameInstance2 = InnerStack.Peek();
		if (viewFrameInstance2?.ViewFrame is IViewFrame { Clear: false })
		{
			string text = Pop();
			LogHelper.Instance.Log("Redraw view '{0}' down the stack for transparent TOS view.", text);
			Show();
			Push(text);
			Show();
		}
		if (viewFrameInstance2?.ViewFrame is IWPFViewFrame { Clear: false })
		{
			IBaseViewFrame viewFrame3 = viewFrameInstance.ViewFrame;
			if (viewFrame3 != null)
			{
				string text2 = (viewFrame3 as IViewFrame)?.Scene?.Actors?.First?.Value?.Name;
				object tag = ((viewFrame3 is IWPFViewFrame) ? ((object)viewFrameInstance.Id) : text2);
				FrameworkElement wPFGridChild = viewFrame3.Scene.GetWPFGridChild(tag);
				if (wPFGridChild != null)
				{
					viewFrame3.Scene.RemoveWPFGridChild(wPFGridChild);
				}
			}
		}
		RaiseViewStateChanged(new ViewStateChangedArgs
		{
			Type = ViewStateChangedType.Pop,
			FromView = viewFrameInstance.ViewFrame.ViewName,
			ToView = InnerStack.Peek()?.ViewFrame?.ViewName
		});
		return viewFrameInstance.ViewFrame.ViewName;
	}

	public string Peek()
	{
		return InnerStack.PeekViewName();
	}

	public string Peek(int index)
	{
		return PeekViewFrame(index)?.ViewFrame?.ViewName;
	}

	public void ClearCache(bool clearResourceViewsOnly)
	{
		LogHelper.Instance.Log("Clearing ViewFrame cache. ResourceViewsOnly: {0}", clearResourceViewsOnly);
		if (!clearResourceViewsOnly)
		{
			m_viewFrameCache.Clear();
		}
		else
		{
			List<string> list = new List<string>();
			foreach (KeyValuePair<string, IBaseViewFrame> item in m_viewFrameCache)
			{
				if (item.Value is IViewFrame)
				{
					list.Add(item.Key);
				}
			}
			foreach (string item2 in list)
			{
				if (m_viewFrameCache.ContainsKey(item2))
				{
					m_viewFrameCache.Remove(item2);
				}
			}
		}
		RaiseViewStateChanged(new ViewStateChangedArgs
		{
			Type = ViewStateChangedType.ClearCache
		});
	}

	public string PopDiscard()
	{
		m_isCriticalErrorViewOnStack = false;
		m_isCriticalErrorViewShown = false;
		IViewFrameInstance viewFrameInstance = InnerStack.Pop();
		if (viewFrameInstance?.ViewFrame != null)
		{
			RaiseViewStateChanged(new ViewStateChangedArgs
			{
				Type = ViewStateChangedType.PopDiscard
			});
			viewFrameInstance.ViewFrame.RaiseOnLeave(viewFrameInstance);
			return viewFrameInstance.ViewFrame.ViewName;
		}
		return null;
	}

	public void EnterTopOfStock()
	{
		IViewFrameInstance viewFrameInstance = InnerStack.Peek();
		if (viewFrameInstance != null)
		{
			if (viewFrameInstance.ViewFrame is IViewFrame viewFrame)
			{
				viewFrame.Scene.MakeDirty(null);
			}
			viewFrameInstance.ViewFrame.RaiseOnEnter(viewFrameInstance);
			RaiseViewStateChanged(new ViewStateChangedArgs
			{
				Type = ViewStateChangedType.EnterTopOfStock
			});
		}
	}

	public string GetCurrentViewAnalyticsName()
	{
		IViewFrameInstance viewFrameInstance = InnerStack.Peek();
		object obj = (viewFrameInstance?.Parameter as IViewAnalyticsName)?.ViewAnalyticsName;
		if (obj == null)
		{
			if (viewFrameInstance == null)
			{
				return null;
			}
			IBaseViewFrame viewFrame = viewFrameInstance.ViewFrame;
			if (viewFrame == null)
			{
				return null;
			}
			obj = viewFrame.ViewName;
		}
		return (string)obj;
	}

	public string GetLastShownView()
	{
		return m_lastShownView;
	}

	public DateTime? GetViewShownTime()
	{
		return m_viewShownOn;
	}

	public void ResetViewShownTime()
	{
		m_viewShownOn = DateTime.Now;
	}

	public int GetActiveHitHandlerCount()
	{
		int result = 0;
		IRenderingService service = ServiceLocator.Instance.GetService<IRenderingService>();
		if (service != null)
		{
			result = service.ActiveScene.ActiveHitHandlers;
		}
		return result;
	}

	public void Push(string viewName)
	{
		Push(viewName, null);
	}

	public void PushCriticalErrorView(string viewName, object parameter)
	{
		Push(viewName, parameter, isCriticalErrorView: true);
	}

	public void Push(string viewName, object parameter, bool isCriticalErrorView = false)
	{
		if ((!m_isCriticalErrorViewOnStack | isCriticalErrorView) || viewName == GetMaintenanceViewName())
		{
			m_isCriticalErrorViewOnStack = m_isCriticalErrorViewShown | isCriticalErrorView;
			IViewFrameInstance viewFrameInstance = InnerStack.Peek();
			if (viewFrameInstance?.ViewFrame != null)
			{
				viewFrameInstance.ViewFrame.RaiseOnLeave(viewFrameInstance);
			}
			Guid guid = Guid.NewGuid();
			InnerStack.Push(guid, viewName, parameter);
			LogHelper.Instance.Log("Push view '{0}' {1} to TOS.", viewName, guid);
			LogHelper.Instance.Log(ViewStackContentsToString());
			RaiseViewStateChanged(new ViewStateChangedArgs
			{
				Type = ViewStateChangedType.Push,
				FromView = viewFrameInstance?.ViewFrame?.ViewName,
				ToView = viewName
			});
		}
		else
		{
			LogHelper.Instance.Log("View " + viewName + " not shown due to critical error view or maintenance mode.");
		}
	}

	public string ViewStackContentsToString()
	{
		StringBuilder stringBuilder = new StringBuilder($"View Stack Count: {InnerStack?.Count} Contents: ");
		int num = 0;
		if (InnerStack != null)
		{
			foreach (IViewFrameInstance item in InnerStack)
			{
				if (num < 10)
				{
					string text = ((num > 0) ? ", " : null);
					stringBuilder.Append(text + item?.ViewFrame?.ViewName);
				}
				else if (num == 10)
				{
					stringBuilder.Append(", ...");
					break;
				}
				num++;
			}
		}
		return stringBuilder.ToString();
	}

	public IViewFrameInstance PeekViewFrame()
	{
		return InnerStack.Peek();
	}

	public IViewFrameInstance PeekViewFrame(int index)
	{
		IViewFrameInstance result = null;
		int num = 0;
		foreach (IViewFrameInstance item in InnerStack)
		{
			if (index == num)
			{
				result = item;
				break;
			}
			num++;
		}
		return result;
	}

	public IViewFrameInstance PeekViewFrameByName(string viewName, out int index)
	{
		index = -1;
		IViewFrameInstance result = null;
		int num = 0;
		if (ViewStackCount > 0)
		{
			foreach (IViewFrameInstance item in InnerStack)
			{
				if (item?.ViewFrame?.ViewName == viewName)
				{
					index = num;
					result = item;
					break;
				}
				num++;
			}
		}
		return result;
	}

	public void CacheViewFrame(IBaseViewFrame baseViewFrame)
	{
		if (baseViewFrame != null && !string.IsNullOrEmpty(baseViewFrame.ViewName))
		{
			m_viewFrameCache[baseViewFrame.ViewName] = baseViewFrame;
		}
	}

	public IBaseViewFrame GetViewFrame(string viewName)
	{
		IBaseViewFrame baseViewFrame = null;
		if (m_viewFrameCache.ContainsKey(viewName))
		{
			baseViewFrame = m_viewFrameCache[viewName];
		}
		else
		{
			LogHelper.Instance.Log("Cache miss, building cached ViewFrame for view named: {0}", viewName);
			ViewFrame viewFrame = new ViewFrame();
			viewFrame.ViewName = viewName;
			viewFrame.Parse();
			baseViewFrame = viewFrame;
			if (baseViewFrame != null)
			{
				m_viewFrameCache[viewName] = baseViewFrame;
			}
		}
		return baseViewFrame;
	}

	public bool IsMaintenanceViewShowing()
	{
		return PeekViewFrame()?.ViewFrame?.IsMaintenanceModeView == true;
	}

	public bool IsSafeToChangeView()
	{
		return !(ServiceLocator.Instance.GetService<IApplicationState>()?.IsInMaintenanceMode ?? false);
	}

	public string GetStartOverViewName()
	{
		if (_startOverViewName == null && m_viewFrameCache != null)
		{
			foreach (KeyValuePair<string, IBaseViewFrame> item in m_viewFrameCache)
			{
				IBaseViewFrame value = item.Value;
				if (value != null && value.IsStartOverView)
				{
					_startOverViewName = item.Value.ViewName;
					break;
				}
			}
		}
		return _startOverViewName;
	}

	public string GetMaintenanceViewName()
	{
		if (_maintenanceViewName == null && m_viewFrameCache != null)
		{
			foreach (KeyValuePair<string, IBaseViewFrame> item in m_viewFrameCache)
			{
				IBaseViewFrame value = item.Value;
				if (value != null && value.IsMaintenanceModeView)
				{
					_maintenanceViewName = item.Value.ViewName;
					break;
				}
			}
		}
		return _maintenanceViewName;
	}

	public void GoBack(object context, bool popOnly)
	{
		LogHelper.Instance.Log("> view_goback; context: {0}, {1}", context, popOnly);
		if (Peek() == GetStartOverViewName())
		{
			Show();
			return;
		}
		Pop();
		if (!popOnly)
		{
			Show();
		}
	}

	public void GoBackSafe(object context, bool popOnly)
	{
		if (IsSafeToChangeView())
		{
			GoBack(context, popOnly);
			return;
		}
		LogHelper.Instance.Log("view_goback_safe({0}) denied.", context);
	}

	public void UnwindToSafe(string unwindToViewName, bool discard, object context)
	{
		if (IsSafeToChangeView())
		{
			UnwindTo(unwindToViewName, discard, context);
		}
		else
		{
			LogHelper.Instance.Log("UnwindToSafe( " + context?.ToString() + " ) denied.");
		}
	}

	public void UnwindTo(string unwindToViewName, bool discard, object context)
	{
		string text = Peek();
		if (!(text != unwindToViewName) || text == null)
		{
			return;
		}
		while (text != null && text != unwindToViewName)
		{
			if (discard)
			{
				LogHelper.Instance.Log("> PopViewDiscard {0}; context: {1}", text, context);
				PopDiscard();
			}
			else
			{
				LogHelper.Instance.Log("> PopView {0}; context: {1}", text, context);
				Pop();
			}
			text = Peek();
		}
		Show();
	}

	private ViewService()
	{
		InnerStack = new ViewStack();
	}

	private void RaiseViewStateChanged(ViewStateChangedArgs e = null)
	{
		if (ViewStateChanged != null)
		{
			ViewStateChanged(this, e);
		}
	}
}
