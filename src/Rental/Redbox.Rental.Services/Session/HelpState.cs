using Redbox.Rental.Model.Session;

namespace Redbox.Rental.Services.Session;

public class HelpState : IHelpState
{
	private int m_currentHelpDocumentPage = 1;

	public HelpDocuments CurrentHelpDocument { get; set; }

	public int CurrentHelpDocumentPage
	{
		get
		{
			return m_currentHelpDocumentPage;
		}
		set
		{
			m_currentHelpDocumentPage = value;
		}
	}
}
