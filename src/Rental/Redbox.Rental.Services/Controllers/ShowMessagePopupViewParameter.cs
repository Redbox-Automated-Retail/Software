using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using Redbox.KioskEngine.ComponentModel;

namespace Redbox.Rental.Services.Controllers;

public class ShowMessagePopupViewParameter : IViewAnalyticsName
{
	public string TitleText { get; }

	public string TitleStyle { get; }

	public string MessageText { get; }

	public string MessageStyle { get; }

	public string Button1Text { get; }

	public string Button1TextStyle { get; }

	public string Button1Style { get; }

	public string Button2Text { get; }

	public string Button2TextStyle { get; }

	public string Button2Style { get; }

	public string ButtonGridStyle { get; }

	public string InnerGridStyle { get; }

	public string FloatingButtonGridStyle { get; }

	public string MessageDockPanelStyle { get; }

	public int Timeout { get; }

	public Action Button1Action { get; }

	public Action Button2Action { get; }

	public Action TimeOutAction { get; }

	public string ActorTag { get; }

	public UserControl ContentUserControl { get; }

	public string BorderBrushColor { get; }

	public Thickness MainGridMargin { get; }

	public List<string> AnalyticsContextList { get; }

	public string ViewAnalyticsName { get; }

	public Visibility ButtonGridSeparatorLineVisibility { get; }

	public bool IsAutoSized { get; }

	public ShowMessagePopupViewParameter(string titleText, string titleStyle, string messageText, string messageStyle, string button1Text, string button1TextStyle, string button1Style, string button2Text, string button2TextStyle, string button2Style, string buttonGridStyle, string innerGridStyle, string floatingButtonGridStyle, string messageDockPanelStyle, int timeout, Action button1Action, Action button2Action, Action timeOutAction, string actorTag, UserControl contentUserControl, string borderBrushColor, Thickness mainGridMargin, List<string> analyticsContextList = null, string viewName = null, Visibility buttonGridSeparatorLineVisibility = Visibility.Collapsed, bool isAutoSized = false)
	{
		TitleText = titleText;
		TitleStyle = titleStyle;
		MessageText = messageText;
		MessageStyle = messageStyle;
		Button1Text = button1Text;
		Button1TextStyle = button1TextStyle;
		Button1Style = button1Style;
		Button2Text = button2Text;
		Button2TextStyle = button2TextStyle;
		Button2Style = button2Style;
		ButtonGridStyle = buttonGridStyle;
		InnerGridStyle = innerGridStyle;
		FloatingButtonGridStyle = floatingButtonGridStyle;
		MessageDockPanelStyle = messageDockPanelStyle;
		Timeout = timeout;
		Button1Action = button1Action;
		Button2Action = button2Action;
		TimeOutAction = timeOutAction;
		ActorTag = actorTag;
		ContentUserControl = contentUserControl;
		BorderBrushColor = borderBrushColor;
		MainGridMargin = mainGridMargin;
		ViewAnalyticsName = viewName;
		AnalyticsContextList = analyticsContextList;
		ButtonGridSeparatorLineVisibility = buttonGridSeparatorLineVisibility;
		IsAutoSized = isAutoSized;
	}
}
