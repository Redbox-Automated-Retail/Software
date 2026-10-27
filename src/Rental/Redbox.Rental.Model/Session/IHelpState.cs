namespace Redbox.Rental.Model.Session;

public interface IHelpState
{
	HelpDocuments CurrentHelpDocument { get; set; }

	int CurrentHelpDocumentPage { get; set; }
}
