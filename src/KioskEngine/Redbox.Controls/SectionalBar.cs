using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using Redbox.Controls.Utilities;

namespace Redbox.Controls;

[Category("ProgressBar")]
public class SectionalBar : ProgressBar
{
	[Flags]
	private enum SectionFlag
	{
		None = 0,
		Minimum = 1,
		Maximum = 2,
		AutoValue = 4,
		Count = 8,
		All = Minimum | Maximum | AutoValue | Count
	}

	public class RangeLimit : IDisposable
	{
		private static BackgroundWorker BgWorker = new BackgroundWorker();

		private static RangeLimit ActiveItem;

		public SectionalBar Section { get; private set; }

		public double Value { get; set; }

		public double MaxValue { get; set; }

		public double Minimum { get; set; }

		public double Maximum { get; set; }

		public double Threshold { get; set; }

		public RangeLimit Nextlimit { get; set; }

		public bool canDispatch { get; set; } = true;

		public RangeLimit(SectionalBar section)
		{
			Section = section;
		}

		public void Dispose()
		{
			DisposeOfWorker();
		}

		public void UpdateThresholdReached(bool isThresholdReached)
		{
			if (isThresholdReached && canDispatch)
			{
				canDispatch = false;
				Application.Current.Dispatcher.Invoke(delegate
				{
					Section.SetThresholdReached();
				});
			}
		}

		public void UpdateValue(double value)
		{
			Application.Current.Dispatcher.Invoke(delegate
			{
				Section.UpdateValue(value);
			});
		}

		public void ExecutedRipple(double value)
		{
			Application.Current.Dispatcher.Invoke(delegate
			{
				Section.ExecutedRipple(value);
			});
		}

		public static void DisposeOfWorker()
		{
			BgWorker.DoWork -= DoWorkEvent;
			BgWorker.RunWorkerCompleted -= DoWorkEventCompleted;
			BgWorker.Dispose();
		}

		private static void DoWorkEventCompleted(object sender, RunWorkerCompletedEventArgs e)
		{
			DisposeOfWorker();
			Application.Current.Dispatcher.Invoke(delegate
			{
				running = false;
			});
		}

		public static void Run(RangeLimit rangeLimit)
		{
			ActiveItem = rangeLimit;
			BgWorker.DoWork += DoWorkEvent;
			BgWorker.RunWorkerCompleted += DoWorkEventCompleted;
			BgWorker.RunWorkerAsync();
		}

		private double GetValue(double index)
		{
			if (Section.ExpConstant > 0.0 && index > Threshold && Threshold != -1.0)
			{
				if (Value > MaxValue)
				{
					Value = MaxValue;
				}
				if (Maximum != MaxValue)
				{
					Maximum = MaxValue;
				}
				index = Section.ExpConstant + Math.Log10(index - Threshold) / Math.Log10(2.0);
			}
			return index;
		}

		private static void DoWorkEvent(object sender, DoWorkEventArgs e)
		{
			while (ActiveItem != null)
			{
				for (double num = ActiveItem.Minimum; num <= ActiveItem.Maximum; num++)
				{
					double value = ActiveItem.GetValue(num);
					Thread.Sleep(20);
					bool isThresholdReached = value >= ActiveItem.Threshold || ActiveItem.Threshold == -1.0;
					if (num >= ActiveItem.Value)
					{
						ActiveItem.ExecutedRipple(value);
						ActiveItem.UpdateThresholdReached(isThresholdReached);
						break;
					}
					ActiveItem.UpdateValue(value);
					ActiveItem.UpdateThresholdReached(isThresholdReached);
				}
				ActiveItem = ActiveItem.Nextlimit;
			}
		}
	}

	private static Dictionary<string, List<SectionalBar>> groupSections;

	public static readonly RoutedEvent ThresholdReachedEvent;

	public static readonly DependencyProperty IsThresholdReachedProperty;

	public static readonly DependencyProperty ThresholdProperty;

	public static readonly DependencyProperty MaxValueProperty;

	public static readonly DependencyProperty GroupCountProperty;

	public static readonly DependencyProperty GroupIndexProperty;

	public static readonly DependencyProperty IsFrontProperty;

	public static readonly DependencyProperty IsBackProperty;

	public static readonly DependencyProperty FrontCornerRadiusProperty;

	public static readonly DependencyProperty BackCornerRadiusProperty;

	public static readonly DependencyProperty AutoCornerRadiusProperty;

	public static readonly DependencyProperty GroupNameProperty;

	public static readonly DependencyProperty DisposeProperty;

	public static readonly DependencyProperty AutomationValueProperty;

	private static List<RangeLimit> RangeLimits { get; set; }

	private static bool running { get; set; }

	private bool canSetGroupName { get; set; } = true;

	protected double ExpConstant { get; set; }

	private SectionFlag AutomationFlag { get; set; }

	public List<SectionalBar> Sections => GetSections(GroupName);

	public bool IsThresholdReached
	{
		get
		{
			return (bool)GetValue(IsThresholdReachedProperty);
		}
		private set
		{
			SetValue(IsThresholdReachedProperty, value);
		}
	}

	public double Threshold
	{
		get
		{
			return (double)GetValue(ThresholdProperty);
		}
		set
		{
			SetValue(ThresholdProperty, value);
		}
	}

	public double MaxValue
	{
		get
		{
			return (double)GetValue(MaxValueProperty);
		}
		set
		{
			SetValue(MaxValueProperty, value);
		}
	}

	public int GroupCount
	{
		get
		{
			return (int)GetValue(GroupCountProperty);
		}
		set
		{
			SetValue(GroupCountProperty, value);
		}
	}

	public int GroupIndex
	{
		get
		{
			return (int)GetValue(GroupIndexProperty);
		}
		private set
		{
			SetValue(GroupIndexProperty, value);
		}
	}

	public bool IsFront
	{
		get
		{
			return (bool)GetValue(IsFrontProperty);
		}
		private set
		{
			SetValue(IsFrontProperty, value);
		}
	}

	public bool IsBack
	{
		get
		{
			return (bool)GetValue(IsBackProperty);
		}
		private set
		{
			SetValue(IsBackProperty, value);
		}
	}

	public CornerRadius FrontCornerRadius
	{
		get
		{
			return (CornerRadius)GetValue(FrontCornerRadiusProperty);
		}
		set
		{
			SetValue(FrontCornerRadiusProperty, value);
		}
	}

	public CornerRadius BackCornerRadius
	{
		get
		{
			return (CornerRadius)GetValue(BackCornerRadiusProperty);
		}
		set
		{
			SetValue(BackCornerRadiusProperty, value);
		}
	}

	public bool AutoCornerRadius
	{
		get
		{
			return (bool)GetValue(AutoCornerRadiusProperty);
		}
		set
		{
			if (value)
			{
				UpdateCornerRadius();
			}
			SetValue(AutoCornerRadiusProperty, value);
		}
	}

	public bool Dispose
	{
		get
		{
			return (bool)GetValue(DisposeProperty);
		}
		set
		{
			SetValue(DisposeProperty, value);
		}
	}

	public string GroupName
	{
		get
		{
			return (string)GetValue(GroupNameProperty);
		}
		set
		{
			SetValue(GroupNameProperty, value);
		}
	}

	public double AutomationValue
	{
		get
		{
			return (double)GetValue(AutomationValueProperty);
		}
		set
		{
			SetValue(AutomationValueProperty, value);
		}
	}

	public event RoutedEventHandler ThresholdReached
	{
		add
		{
			AddHandler(ThresholdReachedEvent, value);
		}
		remove
		{
			RemoveHandler(ThresholdReachedEvent, value);
		}
	}

	public SectionalBar()
	{
		base.IsVisibleChanged += IsVisiblePropertyChanged;
	}

	public void SetInitState(double value, RoutedEventHandler routedEventHandler = null, bool visible = true)
	{
		if (routedEventHandler != null)
		{
			ThresholdReached += routedEventHandler;
		}
		base.Value = value;
		base.Visibility = ((!visible) ? Visibility.Collapsed : Visibility.Visible);
	}

	static SectionalBar()
	{
		groupSections = new Dictionary<string, List<SectionalBar>>();
		ThresholdReachedEvent = EventManager.RegisterRoutedEvent("ThresholdReached", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(SectionalBar));
		IsThresholdReachedProperty = Dependency<SectionalBar>.CreateDependencyProperty("IsThresholdReached", typeof(bool), false, OnThresholdReachedPropertyChanged);
		ThresholdProperty = Dependency<SectionalBar>.CreateDependencyProperty("Threshold", typeof(double), double.MaxValue, OnThresholdPropertyChanged);
		MaxValueProperty = Dependency<SectionalBar>.CreateDependencyProperty("MaxValue", typeof(double), double.MaxValue);
		GroupCountProperty = Dependency<SectionalBar>.CreateDependencyProperty("GroupCount", typeof(int), 1, OnGroupCountPropertyChanged);
		GroupIndexProperty = Dependency<SectionalBar>.CreateDependencyProperty("GroupIndex", typeof(int), 0);
		IsFrontProperty = Dependency<SectionalBar>.CreateDependencyProperty("IsFront", typeof(bool), false);
		IsBackProperty = Dependency<SectionalBar>.CreateDependencyProperty("IsBack", typeof(bool), false);
		FrontCornerRadiusProperty = Dependency<SectionalBar>.CreateDependencyProperty("FrontCornerRadius", typeof(CornerRadius), new CornerRadius(0.0));
		BackCornerRadiusProperty = Dependency<SectionalBar>.CreateDependencyProperty("BackCornerRadius", typeof(CornerRadius), new CornerRadius(0.0));
		AutoCornerRadiusProperty = Dependency<SectionalBar>.CreateDependencyProperty("AutoCornerRadius", typeof(bool), true, OnAutoCornerRadiusPropertyChanged);
		GroupNameProperty = Dependency<SectionalBar>.CreateDependencyProperty("GroupName", typeof(string), string.Empty, OnGroupNameChanged);
		DisposeProperty = Dependency<SectionalBar>.CreateDependencyProperty("Dispose", typeof(bool), false, OnDisposePropertyChanged);
		AutomationValueProperty = Dependency<SectionalBar>.CreateDependencyProperty("AutomationValue", typeof(double), double.MaxValue, OnAutomationValueChanged);
		Dependency<SectionalBar>.DefaultOverrideMetadata(FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata);
		FrameworkElement.HeightProperty.OverrideMetadata(typeof(SectionalBar), Dependency<SectionalBar>.CreatePropertyMetadata(OnHeightPropertyChanged));
		RangeBase.MaximumProperty.OverrideMetadata(typeof(SectionalBar), Dependency<SectionalBar>.CreatePropertyMetadata(double.MaxValue, OnMaximumPropertyChanged));
		RangeBase.MinimumProperty.OverrideMetadata(typeof(SectionalBar), Dependency<SectionalBar>.CreatePropertyMetadata(double.MaxValue, OnMinimumPropertyChanged));
	}

	private void RaiseThresholdReachedEvent()
	{
		RaiseEvent(new RoutedEventArgs(ThresholdReachedEvent, this));
	}

	private static List<SectionalBar> GetSections(string groupName)
	{
		if (groupSections.ContainsKey(groupName))
		{
			List<SectionalBar> list = groupSections[groupName];
			if (list.Count > 0)
			{
				return list;
			}
		}
		return null;
	}

	protected virtual void ExecutedRipple(double value)
	{
		UpdateValue(value);
	}

	protected virtual void UpdateValue(double value)
	{
		base.Value = value;
	}

	protected virtual void SetThresholdReached()
	{
		List<SectionalBar> sections = Sections;
		if (sections == null)
		{
			return;
		}
		foreach (SectionalBar item in sections)
		{
			if (item.GroupIndex == GroupIndex)
			{
				item.IsThresholdReached = true;
			}
		}
		foreach (SectionalBar item2 in sections)
		{
			if (item2.GroupIndex != GroupIndex)
			{
				item2.IsThresholdReached = false;
			}
		}
	}

	protected void UpdateCornerRadius()
	{
		if (AutoCornerRadius)
		{
			double num = ((base.Height > 2.0) ? (base.Height / 2.0) : 0.0);
			FrontCornerRadius = new CornerRadius(num, 0.0, 0.0, num);
			BackCornerRadius = new CornerRadius(0.0, num, num, 0.0);
		}
	}

	protected static void IndexSections(List<SectionalBar> sections)
	{
		int num = 0;
		foreach (SectionalBar section in sections)
		{
			section.GroupIndex = num++;
			section.IsFront = false;
			section.IsBack = false;
		}
		sections[0].IsFront = true;
		sections[sections.Count - 1].IsBack = true;
	}

	protected static void Register(string groupName, SectionalBar sectionBar)
	{
		List<SectionalBar> list;
		if (!groupSections.ContainsKey(groupName))
		{
			list = new List<SectionalBar>();
			list.Add(sectionBar);
			groupSections.Add(groupName, list);
		}
		else
		{
			list = groupSections[groupName];
			if (!list.Contains(sectionBar))
			{
				list.Add(sectionBar);
			}
		}
		if (list?.Count == sectionBar.GroupCount)
		{
			IndexSections(list);
			sectionBar.InvalidateProperty(RangeBase.ValueProperty);
		}
	}

	protected static void Register(SectionalBar sectionBar)
	{
		Register(sectionBar.GroupName, sectionBar);
	}

	protected static void Unregister(string groupName, SectionalBar sectionBar)
	{
		List<SectionalBar> sections = sectionBar.Sections;
		if (sections != null)
		{
			if (sections.Contains(sectionBar))
			{
				sections.Remove(sectionBar);
			}
			if (sections.Count == 0)
			{
				groupSections.Remove(groupName);
			}
			else
			{
				IndexSections(sections);
			}
		}
	}

	protected static void Unregister(SectionalBar sectionBar)
	{
		Unregister(sectionBar.GroupName, sectionBar);
	}

	protected static void CheckRegister(SectionalBar sectionBar)
	{
		List<SectionalBar> sections = sectionBar.Sections;
		if (sections?.Count == sectionBar.GroupCount)
		{
			sections.Clear();
			groupSections.Remove(sectionBar.GroupName);
		}
	}

	private static bool IsValidDoubleValue(double value)
	{
		if (!double.IsNaN(value))
		{
			return !double.IsInfinity(value);
		}
		return false;
	}

	private static bool IsValidChangeValue(double value)
	{
		if (!double.IsNaN(value) && !double.IsInfinity(value))
		{
			return value >= 0.0;
		}
		return false;
	}

	private static void OnGroupNameChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		SectionalBar sectionalBar = (SectionalBar)d;
		if (sectionalBar.canSetGroupName)
		{
			sectionalBar.canSetGroupName = false;
			CheckRegister(sectionalBar);
			Register(sectionalBar);
		}
	}

	private static void OnThresholdReachedPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		(d as SectionalBar).RaiseThresholdReachedEvent();
	}

	private static void OnHeightPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		(d as SectionalBar).UpdateCornerRadius();
	}

	private static void OnAutoCornerRadiusPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		(d as SectionalBar).UpdateCornerRadius();
	}

	private static void OnDisposePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		Unregister(d as SectionalBar);
	}

	private static void OnMinimumPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		(d as SectionalBar).AutomationFlag |= SectionFlag.Minimum;
	}

	private static void OnMaximumPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		(d as SectionalBar).AutomationFlag |= SectionFlag.Maximum;
	}

	private static void OnThresholdPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
	}

	private static void OnGroupCountPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		(d as SectionalBar).AutomationFlag |= SectionFlag.Count;
	}

	private static void OnAutomationValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		(d as SectionalBar).AutomationFlag |= SectionFlag.AutoValue;
	}

	private void IsVisiblePropertyChanged(object s, DependencyPropertyChangedEventArgs e)
	{
		if ((bool)e.NewValue)
		{
			OnIsVisiblePropertyChanged(this);
		}
		else
		{
			UpdateValue(base.Minimum);
		}
	}

	private double GetMaxValue()
	{
		if (MaxValue != double.MaxValue && IsBack && MaxValue > base.Maximum && MaxValue > Threshold)
		{
			ExpConstant = base.Maximum - Math.Log10(MaxValue - Threshold) / Math.Log10(2.0);
		}
		return MaxValue;
	}

	private static void OnIsVisiblePropertyChanged(SectionalBar sectionBar)
	{
		if (running)
		{
			return;
		}
		List<SectionalBar> sections = sectionBar.Sections;
		if (sections == null || sections.Count != sectionBar.GroupCount)
		{
			return;
		}
		bool flag = true;
		foreach (SectionalBar item in sections)
		{
			if ((item.AutomationFlag & SectionFlag.All) != SectionFlag.All)
			{
				flag = false;
				break;
			}
		}
		if (!flag)
		{
			return;
		}
		running = true;
		RangeLimits = new List<RangeLimit>();
		foreach (SectionalBar item2 in sections)
		{
			RangeLimits.Add(new RangeLimit(item2)
			{
				Value = item2.AutomationValue,
				MaxValue = item2.GetMaxValue(),
				Minimum = item2.Minimum,
				Maximum = item2.Maximum,
				Threshold = item2.Threshold
			});
			item2.IsVisibleChanged -= item2.IsVisiblePropertyChanged;
		}
		for (int i = 1; i < RangeLimits.Count; i++)
		{
			RangeLimits[i - 1].Nextlimit = RangeLimits[i];
		}
		RangeLimit.Run(RangeLimits[0]);
	}
}
