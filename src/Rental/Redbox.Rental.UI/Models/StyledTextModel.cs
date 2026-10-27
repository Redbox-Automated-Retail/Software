using System.Windows;

namespace Redbox.Rental.UI.Models;

public class StyledTextModel
{
	public Style Style { get; set; }

	public string Text { get; set; }

	public string Asterisk { get; set; }

	public Style AsteriskStyle { get; set; }
}
