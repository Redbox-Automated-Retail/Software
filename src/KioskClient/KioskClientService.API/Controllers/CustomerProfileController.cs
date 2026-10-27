using System.Threading.Tasks;
using KioskClientService.API.Services.CustomerProfile;
using KioskClientService.API.Services.ProxyApi;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace KioskClientService.API.Controllers;

[Route("api/customerprofile")]
[ApiController]
public class CustomerProfileController : ControllerBase
{
	private readonly ILogger _logger;

	private readonly ICustomerProfileService _customerProfileService;

	public CustomerProfileController(ILogger<CustomerProfileController> logger, ICustomerProfileService customerProfileService)
	{
		_logger = logger;
		_customerProfileService = customerProfileService;
	}

	[HttpPost("login")]
	public async Task<ActionResult<CustomerProfileKioskLoginResponse>> Login([FromBody] CustomerProfileLoginRequest request)
	{
		_logger?.LogInformation("CustomerProfileController.Login");
		return new ObjectResult((await _customerProfileService.Login(request))?.Response);
	}
}
