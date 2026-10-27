using System;
using System.Windows;
using System.Windows.Documents;
using Redbox.Controls;
using Redbox.Rental.Model.Planogram;
using Redbox.Rental.UI;

namespace Redbox.FMA.Models;

internal class ShowPlanogramViewModel : DependencyObject
{
	private static DependencyProperty DocumentScalingProperty = DependencyProperty.Register("DocumentScaling", typeof(DocumentScaling), typeof(ShowPlanogramViewModel), new PropertyMetadata(DocumentScaling.Normal));

	public DynamicRoutedCommand SelectCommand { get; set; }

	public DynamicRoutedCommand CloseCommand { get; set; }

	public string Name { get; set; }

	public DateTime StartDate { get; set; }

	public FileType FileType { get; set; }

	public IDocumentPaginatorSource XpsDocumentSource { get; set; }

	public byte[] rtfDocument { get; set; }

	public DynamicRoutedCommand FitWidthCommand { get; set; }

	public DynamicRoutedCommand FitHeightCommand { get; set; }

	public DynamicRoutedCommand DefaultScaleCommand { get; set; }

	public DocumentScaling DocumentScaling
	{
		get
		{
			return base.Dispatcher.Invoke(() => (DocumentScaling)GetValue(DocumentScalingProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(DocumentScalingProperty, value);
			});
		}
	}
}
