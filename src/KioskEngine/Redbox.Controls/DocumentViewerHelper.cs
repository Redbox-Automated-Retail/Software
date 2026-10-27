using System.Windows;
using System.Windows.Controls;

namespace Redbox.Controls;

public class DocumentViewerHelper : DependencyObject
{
	public static readonly DependencyProperty DocumentScalingProperty = DependencyProperty.RegisterAttached("DocumentScaling", typeof(DocumentScaling), typeof(DocumentViewerHelper), new FrameworkPropertyMetadata
	{
		BindsTwoWayByDefault = true,
		PropertyChangedCallback = delegate(DependencyObject obj, DependencyPropertyChangedEventArgs e)
		{
			DocumentViewer documentViewer = (DocumentViewer)obj;
			switch (GetDocumentScaling(documentViewer))
			{
			case DocumentScaling.Normal:
				documentViewer.Zoom = 100.0;
				break;
			case DocumentScaling.FitHeight:
				documentViewer.FitToHeight();
				break;
			case DocumentScaling.FitWidth:
				documentViewer.FitToWidth();
				break;
			}
		}
	});

	public static DocumentScaling GetDocumentScaling(DependencyObject obj)
	{
		return (DocumentScaling)obj.GetValue(DocumentScalingProperty);
	}

	public static void SetDocumentScaling(DependencyObject obj, DocumentScaling value)
	{
		obj.SetValue(DocumentScalingProperty, value);
	}
}
