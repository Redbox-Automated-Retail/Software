using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Shapes;
using Redbox.Controls;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Rental.UI.ControllersLogic;
using Redbox.Rental.UI.Models;

namespace Redbox.Rental.UI.Views;

[ThemedControl(ThemeName = "Redbox Classic", ControlName = "MemberPerksView")]
public partial class MemberPerksViewControl : ViewUserControl, IComponentConnector
{
	public sealed class TierControl
	{
		private StackPanel _panelElem;

		private MemberPerksViewControl PerksView { get; set; }

		public Image ImageElem { get; set; }

		public TextBlock NameElem { get; set; }

		public TextBlock TextElem { get; set; }

		public Ellipse EllipseElem { get; set; }

		public StackPanel PanelElem
		{
			get
			{
				return _panelElem;
			}
			set
			{
				if (value != _panelElem)
				{
					EllipseElem = PerksView.CreateBackgroundDots(_panelElem = value);
				}
			}
		}

		public TierControl(MemberPerksViewControl perksView)
		{
			PerksView = perksView;
		}
	}

	private List<TierControl> PanelList = new List<TierControl>();

	private MemberPerksModel Model => base.DataContext as MemberPerksModel;

	public MemberPerksViewControl()
	{
		InitializeComponent();
		ApplyTheme<Control>(MainControl);
	}

	private Ellipse CreateBackgroundDots(StackPanel dots)
	{
		Thickness margin = new Thickness(1.0, 0.0, 1.0, 0.0);
		SolidColorBrush solidColorBrush = new SolidColorBrush();
		solidColorBrush.Color = (Color)FindResource("RubineRed");
		for (int i = 0; i < 12; i++)
		{
			dots.Children.Add(new Ellipse
			{
				Width = 5.0,
				Height = 5.0,
				Fill = solidColorBrush,
				Margin = margin,
				Opacity = 0.35
			});
		}
		Ellipse ellipse = new Ellipse
		{
			Width = 10.0,
			Height = 10.0,
			Fill = solidColorBrush,
			Margin = margin,
			Opacity = 0.35
		};
		dots.Children.Add(ellipse);
		for (int j = 0; j < 12; j++)
		{
			dots.Children.Add(new Ellipse
			{
				Width = 5.0,
				Height = 5.0,
				Fill = solidColorBrush,
				Margin = margin,
				Opacity = 0.35
			});
		}
		return ellipse;
	}

	protected override void DataContextHasChanged()
	{
	}

	private void SectionalBar_ThresholdReached(object sender, RoutedEventArgs e)
	{
		if (!(sender is SectionalBar { GroupIndex: >0 } sectionalBar) || !(base.DataContext is MemberPerksModel))
		{
			return;
		}
		MemberPerksModel memberPerksModel = base.DataContext as MemberPerksModel;
		int num = sectionalBar.GroupIndex - 1;
		bool flag = true;
		TierType memberTier = memberPerksModel.MemberTier;
		if (memberTier != TierType.Member && (uint)(memberTier - 1) <= 2u && sectionalBar.Threshold == -1.0)
		{
			flag = false;
			if (sectionalBar.IsThresholdReached && num == (int)(memberPerksModel.MemberTier - 1))
			{
				TierControl tierControl = PanelList[num];
				tierControl.ImageElem.Opacity = 1.0;
				tierControl.NameElem.Opacity = 1.0;
			}
		}
		if (flag)
		{
			TierControl tierControl2 = PanelList[num];
			if (sectionalBar.IsThresholdReached)
			{
				tierControl2.ImageElem.Opacity = 1.0;
				tierControl2.NameElem.Opacity = 1.0;
				tierControl2.TextElem.Opacity = 1.0;
				tierControl2.EllipseElem.Opacity = 1.0;
			}
			else
			{
				tierControl2.ImageElem.Opacity = 0.35;
				tierControl2.NameElem.Opacity = 0.35;
				tierControl2.TextElem.Opacity = 0.35;
			}
		}
	}

	private UIElement InitOpacity(UIElement element)
	{
		if (element != null)
		{
			element.Opacity = 0.35;
		}
		return element;
	}

	private void TierControl_Loaded(object sender, RoutedEventArgs e)
	{
		UIElementCollection children = (sender as GridExt).Children;
		TierControl tierControl = new TierControl(this)
		{
			ImageElem = (Image)InitOpacity(children[0]),
			NameElem = (TextBlock)InitOpacity(children[1]),
			TextElem = (TextBlock)InitOpacity(children[3]),
			PanelElem = (StackPanel)children[2]
		};
		tierControl.EllipseElem.Opacity = 0.35;
		PanelList.Add(tierControl);
	}

	private void BarControl_Loaded(object sender, RoutedEventArgs e)
	{
		((sender as GridExt).Children[0] as SectionalBar).SetInitState(0.0, SectionalBar_ThresholdReached);
	}

	private void SnapshotButton_Pressed(object sender, RoutedEventArgs e)
	{
		HandleWPFHit();
		if (Model != null)
		{
			ViewUserControl.InvokeRoutedCommand(Model.SelectedButtonCommand, "SnapshotButton");
		}
	}

	private void RedboxPlusButton_Pressed(object sender, RoutedEventArgs e)
	{
		HandleWPFHit();
		if (Model != null)
		{
			ViewUserControl.InvokeRoutedCommand(Model.SelectedButtonCommand, "RedboxPlusButton");
		}
	}

	private void PerksButton_Pressed(object sender, RoutedEventArgs e)
	{
		HandleWPFHit();
		if (Model != null)
		{
			ViewUserControl.InvokeRoutedCommand(Model.SelectedButtonCommand, "PerksButton");
		}
	}

	private void PromosButton_Pressed(object sender, RoutedEventArgs e)
	{
		HandleWPFHit();
		if (Model != null)
		{
			ViewUserControl.InvokeRoutedCommand(Model.SelectedButtonCommand, "PromosButton");
		}
	}
}
