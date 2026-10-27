using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using UpdateClientService.API.Services.Utilities;

namespace UpdateClientService.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class PowerShellController
{
	private readonly ICommandLineService _cmd;

	public PowerShellController(ICommandLineService cmd)
	{
		_cmd = cmd;
	}

	[HttpPost("file")]
	[ProducesResponseType(typeof(OkResult), 200)]
	[ProducesResponseType(typeof(StatusCodeResult), 500)]
	public async Task<ActionResult> ExecuteFile(IFormFile file)
	{
		string temp = Path.GetTempFileName().Replace(".tmp", ".ps1");
		using (FileStream s = File.Create(temp))
		{
			await file.CopyToAsync(s);
		}
		bool flag = _cmd.TryExecutePowerShellScriptFromFile(temp);
		File.Delete(temp);
		return flag ? new OkResult() : new StatusCodeResult(500);
	}

	[HttpPost("script")]
	[ProducesResponseType(typeof(OkResult), 200)]
	[ProducesResponseType(typeof(StatusCodeResult), 500)]
	public ActionResult ExecuteScript(string script)
	{
		if (!_cmd.TryExecutePowerShellScript(script))
		{
			return new StatusCodeResult(500);
		}
		return new OkResult();
	}
}
