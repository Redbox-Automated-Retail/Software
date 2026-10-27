using System.Collections.Generic;
using Redbox.Rental.Model.Planogram;
using Redbox.Rental.UI;

namespace Redbox.FMA.Models;

public class SelectPlanogramViewModel
{
	public IEnumerable<Planogram> Planograms { get; set; } = new List<Planogram>();

	public DynamicRoutedCommand CloseCommand { get; set; }

	public DynamicRoutedCommand SelectCommand { get; set; }

	public DynamicRoutedCommand ForceSyncCommand { get; set; }
}
