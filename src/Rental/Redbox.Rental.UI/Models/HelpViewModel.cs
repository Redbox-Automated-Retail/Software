using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Xps.Packaging;
using Redbox.KioskEngine.ComponentModel.TextToSpeech;
using Redbox.Rental.Model.Session;

namespace Redbox.Rental.UI.Models;

public class HelpViewModel : BaseModel<HelpViewModel>
{
	public delegate void HelpDocumentChangedHandler(HelpDocuments helpDocument);

	public delegate void HelpDocumentPageChangedHandler(int pageNumber);

	public delegate void HelpPaddleButtonPressed(string source);

	public class HelpDocumentInfo
	{
		public string DocumentName { get; set; }

		public string DefaultDocumentName { get; set; }

		public string Title { get; set; }

		public bool Visible { get; set; }

		public string AnalyticsButtonName { get; set; }

		public string AbeTitle { get; set; }
	}

	public Func<ISpeechControl> OnGetSpeechControl;

	public static DependencyProperty BackButtonTextProperty = BaseModel<HelpViewModel>.CreateDependencyProperty("BackButtonText", TYPES.STRING);

	public static DependencyProperty CloseButtonTextProperty = BaseModel<HelpViewModel>.CreateDependencyProperty("CloseButtonText", TYPES.STRING);

	public static DependencyProperty TitleTextProperty = BaseModel<HelpViewModel>.CreateDependencyProperty("TitleText", TYPES.STRING);

	public static DependencyProperty ContactTextProperty = BaseModel<HelpViewModel>.CreateDependencyProperty("ContactText", TYPES.STRING);

	public static DependencyProperty ShowContactTextProperty = BaseModel<HelpViewModel>.CreateDependencyProperty("ShowContactText", TYPES.BOOL, false);

	public static DependencyProperty StoreNumberLabelProperty = BaseModel<HelpViewModel>.CreateDependencyProperty("StoreNumberLabel", TYPES.STRING);

	public static DependencyProperty StoreNumberTextProperty = BaseModel<HelpViewModel>.CreateDependencyProperty("StoreNumberText", TYPES.STRING);

	public static DependencyProperty PageCounterTextProperty = BaseModel<HelpViewModel>.CreateDependencyProperty("PageCounterText", TYPES.STRING);

	private static DependencyProperty CurrentHelpDocumentPageProperty = DependencyProperty.Register("CurrentHelpDocumentPage", typeof(int), typeof(HelpViewModel), new PropertyMetadata(0, CurrentHelpDocPageChanged));

	public static DependencyProperty HelpDocumentPageCountProperty = BaseModel<HelpViewModel>.CreateDependencyProperty("HelpDocumentPageCount", TYPES.INT, 0);

	public static DependencyProperty XpsDocumentProperty = BaseModel<HelpViewModel>.CreateDependencyProperty("XpsDocument", typeof(XpsDocument));

	public static DependencyProperty CurrentPageTextProperty = BaseModel<HelpViewModel>.CreateDependencyProperty("CurrentPageText", TYPES.STRING);

	private HelpDocuments m_currentHelpDocument;

	public ObservableCollection<HelpButtonData> ButtonList { get; set; }

	public string BackButtonText
	{
		get
		{
			return base.Dispatcher.Invoke(() => (string)GetValue(BackButtonTextProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(BackButtonTextProperty, value);
			});
		}
	}

	public string CloseButtonText
	{
		get
		{
			return base.Dispatcher.Invoke(() => (string)GetValue(CloseButtonTextProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(CloseButtonTextProperty, value);
			});
		}
	}

	public string TitleText
	{
		get
		{
			return base.Dispatcher.Invoke(() => (string)GetValue(TitleTextProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(TitleTextProperty, value);
			});
		}
	}

	public string ContactText
	{
		get
		{
			return base.Dispatcher.Invoke(() => (string)GetValue(ContactTextProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(ContactTextProperty, value);
			});
		}
	}

	public bool ShowContactText
	{
		get
		{
			return base.Dispatcher.Invoke(() => (bool)GetValue(ShowContactTextProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(ShowContactTextProperty, value);
			});
		}
	}

	public string StoreNumberLabel
	{
		get
		{
			return base.Dispatcher.Invoke(() => (string)GetValue(StoreNumberLabelProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(StoreNumberLabelProperty, value);
			});
		}
	}

	public string StoreNumberText
	{
		get
		{
			return base.Dispatcher.Invoke(() => (string)GetValue(StoreNumberTextProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(StoreNumberTextProperty, value);
			});
		}
	}

	public string PageCounterText
	{
		get
		{
			return base.Dispatcher.Invoke(() => (string)GetValue(PageCounterTextProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(PageCounterTextProperty, value);
			});
		}
	}

	public int CurrentHelpDocumentPage
	{
		get
		{
			return base.Dispatcher.Invoke(() => (int)GetValue(CurrentHelpDocumentPageProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(CurrentHelpDocumentPageProperty, value);
			});
		}
	}

	public int HelpDocumentPageCount
	{
		get
		{
			return base.Dispatcher.Invoke(() => (int)GetValue(HelpDocumentPageCountProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(HelpDocumentPageCountProperty, value);
			});
		}
	}

	public XpsDocument XpsDocument
	{
		get
		{
			return base.Dispatcher.Invoke(() => (XpsDocument)GetValue(XpsDocumentProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(XpsDocumentProperty, value);
			});
		}
	}

	public string CurrentPageText
	{
		get
		{
			return base.Dispatcher.Invoke(() => (string)GetValue(CurrentPageTextProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(CurrentPageTextProperty, value);
			});
		}
	}

	public HelpDocuments CurrentHelpDocument
	{
		get
		{
			return m_currentHelpDocument;
		}
		set
		{
			m_currentHelpDocument = value;
			foreach (HelpButtonData button in ButtonList)
			{
				button.IsEnabled = button.DocumentToShow != m_currentHelpDocument;
			}
		}
	}

	public Dictionary<HelpDocuments, HelpDocumentInfo> HelpDocumentInfoList { get; set; } = new Dictionary<HelpDocuments, HelpDocumentInfo>();

	public event HelpDocumentChangedHandler OnHelpDocumentChanged;

	public event HelpDocumentChangedHandler OnHelpDocumentButtonPress;

	public event HelpDocumentPageChangedHandler OnHelpDocumentPageChanged;

	public event HelpPaddleButtonPressed OnHelpPaddleButtonPressed;

	public event Action OnGoBack;

	public event Action OnRedboxLogoCommand;

	public HelpViewModel()
	{
		ButtonList = new ObservableCollection<HelpButtonData>();
	}

	private static void CurrentHelpDocPageChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		((HelpViewModel)d).ProcessHelpDocumentPageChanged((int)e.NewValue);
	}

	public void ProcessHelpDocumentButtonPress(HelpDocuments helpDocument)
	{
		OnHelpDocumentButtonPress?.Invoke(helpDocument);
	}

	public void ProcessHelpDocumentChanged(HelpDocuments helpDocument)
	{
		OnHelpDocumentChanged?.Invoke(helpDocument);
	}

	public void ProcessHelpDocumentPageChanged(int pageNumber)
	{
		OnHelpDocumentPageChanged?.Invoke(pageNumber);
	}

	public void ProcessHelpPaddleButtonPressed(string source)
	{
		OnHelpPaddleButtonPressed?.Invoke(source);
	}

	public void ProcessGoBack()
	{
		OnGoBack?.Invoke();
	}

	public void ProcessRedboxLogoCommand()
	{
		OnRedboxLogoCommand?.Invoke();
	}
}
