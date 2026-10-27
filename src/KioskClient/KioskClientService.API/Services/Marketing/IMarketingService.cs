using System.Threading.Tasks;
using KioskClientService.API.Models;

namespace KioskClientService.API.Services.Marketing;

public interface IMarketingService
{
	Task<EmailConfirmationResponse> EmailConfirmation(EmailConfirmationPayload payload);
}
