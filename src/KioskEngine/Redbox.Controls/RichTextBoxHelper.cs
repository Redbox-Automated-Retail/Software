using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace Redbox.Controls;

public class RichTextBoxHelper : DependencyObject
{
	public static readonly DependencyProperty DocumentRtfProperty = DependencyProperty.RegisterAttached("DocumentRtf", typeof(byte[]), typeof(RichTextBoxHelper), new FrameworkPropertyMetadata
	{
		BindsTwoWayByDefault = true,
		PropertyChangedCallback = delegate(DependencyObject obj, DependencyPropertyChangedEventArgs e)
		{
			RichTextBox richTextBox = (RichTextBox)obj;
			byte[] documentRtf = GetDocumentRtf(richTextBox);
			FlowDocument doc = new FlowDocument();
			TextRange range = new TextRange(doc.ContentStart, doc.ContentEnd);
			range.Load(new MemoryStream(documentRtf), DataFormats.Rtf);
			richTextBox.Document = doc;
			range.Changed += delegate
			{
				if (richTextBox.Document == doc)
				{
					MemoryStream memoryStream = new MemoryStream();
					range.Save(memoryStream, DataFormats.Rtf);
					SetDocumentRtf(richTextBox, memoryStream.ToArray());
				}
			};
		}
	});

	public static byte[] GetDocumentRtf(DependencyObject obj)
	{
		return (byte[])obj.GetValue(DocumentRtfProperty);
	}

	public static void SetDocumentRtf(DependencyObject obj, byte[] value)
	{
		obj.SetValue(DocumentRtfProperty, value);
	}
}
