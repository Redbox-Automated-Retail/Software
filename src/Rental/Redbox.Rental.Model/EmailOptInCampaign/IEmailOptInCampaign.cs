namespace Redbox.Rental.Model.EmailOptInCampaign;

public interface IEmailOptInCampaign
{
	string ProgramName { get; set; }

	string TemplateName { get; set; }

	string CampaignId { get; set; }

	string ImageName { get; set; }
}
