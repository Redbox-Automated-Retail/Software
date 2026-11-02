using System.Collections.Generic;
using Redbox.Rental.Model.Planogram;
using Redbox.Rental.UI;

namespace Redbox.FMA.Models;

public class SelectPlanogramViewModel
{
	public IEnumerable<Planogram> Planograms { get; set; } = new List<Planogram>();

	// The commands are set in SelectPlanogramViewController.ConfigureView (object initializer).
	public DynamicRoutedCommand CloseCommand { get; set; } = null!;

	public DynamicRoutedCommand SelectCommand { get; set; } = null!;

	public DynamicRoutedCommand ForceSyncCommand { get; set; } = null!;
}
