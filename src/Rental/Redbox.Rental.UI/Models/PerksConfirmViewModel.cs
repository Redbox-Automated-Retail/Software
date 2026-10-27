using System.Windows;
using System.Windows.Input;

namespace Redbox.Rental.UI.Models;

public class PerksConfirmViewModel : BaseModel<PerksConfirmViewModel>
{
	public string TitleText { get; set; }

	public Style TitleStyle { get; set; }

	public string MessageText { get; set; }

	public string TermsText { get; set; }

	public string AcceptButtonText { get; set; }

	public string BackButtonText { get; set; }

	public string TermsButtonText { get; set; }

	public string LegalText { get; set; }

	public ICommand AcceptButtonCommand { get; set; }

	public ICommand BackButtonCommand { get; set; }

	public ICommand TermsButtonCommand { get; set; }
}
