using System;
using System.Text;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Xps.Packaging;
using System.Xml;
using Redbox.Rental.UI.Models;

namespace Redbox.Rental.UI.Controls;

public partial class HelpDocViewUserControl : BaseUserControl, IComponentConnector
{
	private static DependencyProperty CurrentPageNumberProperty = DependencyProperty.Register("CurrentPageNumber", typeof(int), typeof(HelpDocViewUserControl), new PropertyMetadata(0, PageNumberChanged));

	private static DependencyProperty TotalPageCountProperty = DependencyProperty.Register("TotalPageCount", typeof(int), typeof(HelpDocViewUserControl), new PropertyMetadata(0, PageNumberChanged));

	private static DependencyProperty PageCounterTextProperty = DependencyProperty.Register("PageCounterText", typeof(string), typeof(HelpDocViewUserControl), new PropertyMetadata(null, PageNumberChanged));

	private static DependencyProperty XpsDocumentProperty = DependencyProperty.Register("XpsDocument", typeof(XpsDocument), typeof(HelpDocViewUserControl), new PropertyMetadata(null, XpsDocumentChanged));

	private static DependencyProperty CurrentPageTextProperty = DependencyProperty.Register("CurrentPageText", typeof(string), typeof(HelpDocViewUserControl), new PropertyMetadata(null));

	private FixedDocumentSequence m_fixedDocumentSequence;

	public int CurrentPageNumber
	{
		get
		{
			return base.Dispatcher.Invoke(() => (int)GetValue(CurrentPageNumberProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(CurrentPageNumberProperty, value);
			});
		}
	}

	public int TotalPageCount
	{
		get
		{
			return base.Dispatcher.Invoke(() => (int)GetValue(TotalPageCountProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(TotalPageCountProperty, value);
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

	public event HelpViewModel.HelpPaddleButtonPressed OnHelpPaddleButtonPressed;

	public HelpDocViewUserControl()
	{
		InitializeComponent();
	}

	private static void PageNumberChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		if (d is HelpDocViewUserControl helpDocViewUserControl)
		{
			helpDocViewUserControl.SetCurrentPage();
		}
	}

	private static void XpsDocumentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		if (d is HelpDocViewUserControl helpDocViewUserControl)
		{
			helpDocViewUserControl.UpdateXpsDocument();
		}
	}

	private void UpdateXpsDocument()
	{
		m_fixedDocumentSequence = ((XpsDocument != null) ? XpsDocument.GetFixedDocumentSequence() : null);
		TotalPageCount = (m_fixedDocumentSequence?.DocumentPaginator?.PageCount).GetValueOrDefault();
		ChangePage(0);
	}

	private void UpdatePageCounter()
	{
		page_counter.Text = ((!string.IsNullOrEmpty(PageCounterText) && m_fixedDocumentSequence != null) ? string.Format(PageCounterText, CurrentPageNumber, TotalPageCount) : null);
	}

	private int ChangePageNumber(int changeAmount)
	{
		int val = CurrentPageNumber + changeAmount;
		CurrentPageNumber = Math.Min(Math.Max(1, val), (m_fixedDocumentSequence == null) ? 1 : m_fixedDocumentSequence.DocumentPaginator.PageCount);
		return CurrentPageNumber;
	}

	private void ChangePage(int changeAmount)
	{
		ChangePageNumber(changeAmount);
		CurrentPageText = GetCurrentPageText();
		UpdateDocumentPageVisual(CurrentPageNumber);
		UpdatePaddleVisibility();
	}

	private void SetCurrentPage()
	{
		UpdatePageCounter();
		CurrentPageText = GetCurrentPageText();
		UpdateDocumentPageVisual(CurrentPageNumber);
		UpdatePaddleVisibility();
	}

	private void UpdatePaddleVisibility()
	{
		left_arrow.Visibility = ((m_fixedDocumentSequence == null || CurrentPageNumber == 1) ? Visibility.Hidden : Visibility.Visible);
		right_arrow.Visibility = ((m_fixedDocumentSequence == null || CurrentPageNumber == m_fixedDocumentSequence.DocumentPaginator.PageCount) ? Visibility.Hidden : Visibility.Visible);
	}

	private void PageLeftCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
	{
		ChangePage(-1);
		HandleWPFHit();
		RaiseOnHelpPaddleButtonPressed("Left");
	}

	private void PageRightCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
	{
		ChangePage(1);
		HandleWPFHit();
		RaiseOnHelpPaddleButtonPressed("Right");
	}

	private void RaiseOnHelpPaddleButtonPressed(string source)
	{
		OnHelpPaddleButtonPressed?.Invoke(source);
	}

	public string GetCurrentPageText()
	{
		StringBuilder stringBuilder = new StringBuilder();
		if (XpsDocument != null)
		{
			IXpsFixedDocumentSequenceReader fixedDocumentSequenceReader = XpsDocument.FixedDocumentSequenceReader;
			if (fixedDocumentSequenceReader != null)
			{
				IXpsFixedDocumentReader xpsFixedDocumentReader = fixedDocumentSequenceReader.FixedDocuments[0];
				if (xpsFixedDocumentReader != null)
				{
					IXpsFixedPageReader xpsFixedPageReader = xpsFixedDocumentReader.FixedPages[Math.Max(0, CurrentPageNumber - 1)];
					if (xpsFixedPageReader != null)
					{
						XmlReader xmlReader = xpsFixedPageReader.XmlReader;
						if (xmlReader != null)
						{
							while (xmlReader.Read())
							{
								if (xmlReader.Name == "Glyphs" && xmlReader.HasAttributes)
								{
									string attribute = xmlReader.GetAttribute("UnicodeString");
									stringBuilder.Append(attribute);
								}
							}
						}
					}
				}
			}
		}
		return stringBuilder.ToString();
	}

	private void UpdateDocumentPageVisual(int pageNumber)
	{
		VisualBrush fill = null;
		if (m_fixedDocumentSequence != null && m_fixedDocumentSequence.DocumentPaginator != null)
		{
			DocumentPage page = m_fixedDocumentSequence.DocumentPaginator.GetPage(Math.Max(0, CurrentPageNumber - 1));
			if (page != null)
			{
				fill = new VisualBrush(page.Visual);
			}
		}
		DocumentPageVisual.Fill = fill;
		InvalidateVisual();
	}
}
