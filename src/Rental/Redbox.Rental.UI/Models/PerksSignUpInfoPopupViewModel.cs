using System.Windows;
using System.Windows.Input;

namespace Redbox.Rental.UI.Models;

public class PerksSignUpInfoPopupViewModel : BaseModel<PerksSignUpInfoPopupViewModel>
{
	public string TitleText { get; set; }

	public string EarnHeading { get; set; }

	public string UseHeading { get; set; }

	public string VisitHeading { get; set; }

	public string BalanceHeading { get; set; }

	public string OKButtonText { get; set; }

	public HorizontalAlignment OKButtonHorizontalAlignment { get; set; }

	public ICommand OKButtonCommand { get; set; }

	public string EarnOption1Desc { get; set; }

	public string EarnOption2Desc { get; set; }

	public string EarnOption3Desc { get; set; }

	public string EarnOption4Desc { get; set; }

	public string UseOption1Desc { get; set; }

	public string UseOption2Desc { get; set; }

	public Visibility EarnOption4DescVisibility { get; set; }
}
