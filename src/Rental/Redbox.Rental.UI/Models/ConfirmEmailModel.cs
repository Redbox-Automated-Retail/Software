using System;
using System.Windows;

namespace Redbox.Rental.UI.Models;

public class ConfirmEmailModel : AbstractViewModel
{
	public static readonly DependencyProperty HeaderTextProperty = DependencyProperty.Register("HeaderText", typeof(string), typeof(ConfirmEmailModel), new FrameworkPropertyMetadata(null));

	public static readonly DependencyProperty BodyTextProperty = DependencyProperty.Register("BodyText", typeof(string), typeof(ConfirmEmailModel), new FrameworkPropertyMetadata(null));

	public static readonly DependencyProperty OptionalBodyTextProperty = DependencyProperty.Register("OptionalBodyText", typeof(string), typeof(ConfirmEmailModel), new FrameworkPropertyMetadata(null));

	public static readonly DependencyProperty OptionalBodyTextVisibilityProperty = DependencyProperty.Register("OptionalBodyTextVisibility", typeof(Visibility), typeof(ConfirmEmailModel), new FrameworkPropertyMetadata(Visibility.Collapsed));

	public static readonly DependencyProperty Button1VisibilityProperty = DependencyProperty.Register("Button1Visibility", typeof(Visibility), typeof(ConfirmEmailModel), new FrameworkPropertyMetadata(Visibility.Collapsed));

	public static readonly DependencyProperty Button2VisibilityProperty = DependencyProperty.Register("Button2Visibility", typeof(Visibility), typeof(ConfirmEmailModel), new FrameworkPropertyMetadata(Visibility.Collapsed));

	public bool HasActionExecuted { get; set; }

	public string HeaderText
	{
		get
		{
			return base.Dispatcher.Invoke(() => (string)GetValue(HeaderTextProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(HeaderTextProperty, value);
			});
		}
	}

	public string BodyText
	{
		get
		{
			return base.Dispatcher.Invoke(() => (string)GetValue(BodyTextProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(BodyTextProperty, value);
			});
		}
	}

	public string OptionalBodyText
	{
		get
		{
			return base.Dispatcher.Invoke(() => (string)GetValue(OptionalBodyTextProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(OptionalBodyTextProperty, value);
			});
		}
	}

	public Visibility OptionalBodyTextVisibility
	{
		get
		{
			return base.Dispatcher.Invoke(() => (Visibility)GetValue(OptionalBodyTextVisibilityProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(OptionalBodyTextVisibilityProperty, value);
			});
		}
	}

	public Visibility Button1Visibility
	{
		get
		{
			return base.Dispatcher.Invoke(() => (Visibility)GetValue(Button1VisibilityProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(Button1VisibilityProperty, value);
			});
		}
	}

	public Visibility Button2Visibility
	{
		get
		{
			return base.Dispatcher.Invoke(() => (Visibility)GetValue(Button2VisibilityProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(Button2VisibilityProperty, value);
			});
		}
	}

	public event Action Button1Action;

	public event Action Button2Action;

	public event Action TimeoutAction;

	public void ProcessButton1Action()
	{
		Button1Action?.Invoke();
	}

	public void ProcessButton2Action()
	{
		Button2Action?.Invoke();
	}

	public void ProcessTimeoutAction()
	{
		TimeoutAction?.Invoke();
	}
}
