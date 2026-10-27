namespace Redbox.Rental.Model.EmailOptInCampaign;

public class EmailOptInCampaign : IEmailOptInCampaign
{
	public string ProgramName { get; set; }

	public string TemplateName { get; set; }

	public string CampaignId { get; set; }

	public string ImageName { get; set; }
}
