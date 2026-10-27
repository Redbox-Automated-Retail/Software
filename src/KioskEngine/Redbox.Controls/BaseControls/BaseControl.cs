using System;
using System.ComponentModel;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;

namespace Redbox.Controls.BaseControls;

[Browsable(false)]
public class BaseControl : Control
{
	private object[] objArray = new object[1] { true };

	private object UpdateVisualStateInfo;

	public new static readonly DependencyProperty IsEnabledProperty = CreateDependencyProperty("IsEnabled", typeof(bool), false, OnVisualStatePropertyChanged);

	public static readonly DependencyProperty IsMouseOverPropertyKey = CreateDependencyProperty("IsMouseOver", typeof(bool), false, OnVisualStatePropertyChanged);

	public static DependencyProperty CreateDependencyProperty(string propertyName, Type propertyType, object defaultValue, PropertyChangedCallback callback = null)
	{
		FrameworkPropertyMetadata typeMetadata = ((callback == null) ? new FrameworkPropertyMetadata(defaultValue)
		{
			AffectsRender = true
		} : new FrameworkPropertyMetadata(defaultValue, callback.Invoke)
		{
			AffectsRender = true
		});
		return DependencyProperty.Register(propertyName, propertyType, typeof(BaseControl), typeMetadata);
	}

	public static void OnVisualStatePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		if (d is BaseControl baseControl)
		{
			baseControl.UpdateVisualState();
		}
	}

	public void UpdateVisualState()
	{
		UpdateVisualState(useTransitions: true);
	}

	public void UpdateVisualState(bool useTransitions)
	{
		if (UpdateVisualStateInfo == null)
		{
			UpdateVisualStateInfo = GetType().GetMethod("UpdateVisualState", new Type[1] { typeof(bool) });
		}
		objArray[0] = useTransitions;
		((MethodInfo)UpdateVisualStateInfo).Invoke(this, objArray);
	}

	protected virtual void ChangeVisualState(bool useTransitions)
	{
	}

	protected void ChangeValidationVisualState(bool useTransitions)
	{
		if (Validation.GetHasError(this))
		{
			if (base.IsKeyboardFocused)
			{
				VisualStateManager.GoToState(this, "InvalidFocused", useTransitions);
			}
			else
			{
				VisualStateManager.GoToState(this, "InvalidUnfocused", useTransitions);
			}
		}
		else
		{
			VisualStateManager.GoToState(this, "Valid", useTransitions);
		}
	}
}
