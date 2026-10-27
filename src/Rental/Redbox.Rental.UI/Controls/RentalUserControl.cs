using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Rental.Model;
using Redbox.Rental.Model.KioskHealth;

namespace Redbox.Rental.UI.Controls;

public class RentalUserControl : UserControl
{
	private int _isRunning;

	private static System.Timers.Timer DelayTimer { get; set; }

	public bool IsDesign { get; set; }

	public bool IsABEOn { get; set; }

	public IActor Actor { get; set; }

	protected bool IsMultiClickBlocked
	{
		get
		{
			return DelayTimer != null;
		}
		set
		{
			SetDelayTimer(value);
		}
	}

	public event WPFHitHandler OnWPFHit;

	public RentalUserControl()
	{
		IsABEOn = ServiceLocator.Instance.GetService<IApplicationState>()?.IsABEOn ?? false;
		IsDesign = DesignerProperties.GetIsInDesignMode(this);
	}

	public void HandleWPFHit()
	{
		if (Actor != null)
		{
			OnWPFHit?.Invoke(Actor);
		}
		else
		{
			ServiceLocator.Instance.GetService<IRenderingService>()?.ActiveScene.PrcoessWPFHit();
		}
		PostHealthActivity();
	}

	protected void PostHealthActivity()
	{
		ServiceLocator.Instance.GetService<ITouchScreenHealth>()?.PostActivity();
		ServiceLocator.Instance.GetService<IViewHealth>()?.PostActivity("Button Press");
	}

	protected void SetDelayTimer(bool setTimer)
	{
		if (setTimer)
		{
			DelayTimer = new System.Timers.Timer(500.0);
			DelayTimer.Elapsed += delegate
			{
				DisposeDelayTimer();
			};
			DelayTimer.Start();
		}
		else
		{
			DisposeDelayTimer();
		}
	}

	protected void DisposeDelayTimer()
	{
		if (DelayTimer != null)
		{
			DelayTimer.Stop();
			DelayTimer.Dispose();
			DelayTimer = null;
		}
	}

	protected void Log(string message, string prefix = "")
	{
		LogHelper.Instance.Log($"{prefix}WPF View {GetType().Name}: {message}");
	}

	protected static IEnumerable<T> FindLogicalChildren<T>(DependencyObject dependencyObj) where T : DependencyObject
	{
		if (dependencyObj == null)
		{
			yield break;
		}
		if (dependencyObj is T)
		{
			yield return dependencyObj as T;
		}
		foreach (DependencyObject item in LogicalTreeHelper.GetChildren(dependencyObj).OfType<DependencyObject>())
		{
			foreach (T item2 in FindLogicalChildren<T>(item))
			{
				yield return item2;
			}
		}
	}

	protected static DependencyProperty CreateDependencyProperty<TOwner>(string propertyName, Type propertyType, object defaultValue = null, bool affectsRender = true)
	{
		return DependencyProperty.Register(propertyName, propertyType, typeof(TOwner), new FrameworkPropertyMetadata(defaultValue)
		{
			AffectsRender = affectsRender
		});
	}

	protected void RunCommandOnce(Action action)
	{
		if (Interlocked.CompareExchange(ref _isRunning, 1, 0) == 1)
		{
			LogHelper.Instance.Log("RunCommandOnce - Skipping Click...");
		}
		else
		{
			action?.Invoke();
		}
	}

	protected void CompleteRunOnce()
	{
		_isRunning = 0;
	}
}
