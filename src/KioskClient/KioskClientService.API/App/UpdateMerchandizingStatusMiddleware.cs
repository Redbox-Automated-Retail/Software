using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using KioskClientService.API.Services.Inventory;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace KioskClientService.API.App;

public class UpdateMerchandizingStatusMiddleware
{
	private readonly RequestDelegate _next;

	private readonly ILogger<UpdateMerchandizingStatusMiddleware> _logger;

	public UpdateMerchandizingStatusMiddleware(RequestDelegate next, ILogger<UpdateMerchandizingStatusMiddleware> logger)
	{
		_next = next ?? throw new ArgumentNullException("next");
		_logger = logger;
	}

	public async Task Invoke(HttpContext context)
	{
		HttpRequest request = context.Request;
		if (request.Path.Value.Contains("UpdateMerchandizingStatus", StringComparison.InvariantCultureIgnoreCase))
		{
			_ = request.Body;
			string originalContent = new StreamReader(request.Body).ReadToEnd();
			Stream body;
			try
			{
				List<MerchandStatusUpdate> list = JsonConvert.DeserializeObject<List<MerchandStatusUpdate>>(originalContent);
				if (list != null && list.Any())
				{
					MerchandStatusUpdate merchandStatusUpdate = list.First();
					if (!merchandStatusUpdate.MessageId.HasValue)
					{
						_logger.LogInformation("Updating UpdateMerchandizingStatus payload to contain a message id for replay");
						list.First().MessageId = Guid.NewGuid();
						StringContent stringContent = new StringContent(list.ToJson(), Encoding.UTF8, "application/json");
						body = await stringContent.ReadAsStreamAsync();
					}
					else
					{
						body = new MemoryStream(Encoding.UTF8.GetBytes(originalContent));
					}
				}
				else
				{
					body = new MemoryStream(Encoding.UTF8.GetBytes(originalContent));
				}
			}
			catch (Exception exception)
			{
				_logger.LogCritical(exception, "Exception adding message id to UpdateMerchandizingStatus payload!");
				body = new MemoryStream(Encoding.UTF8.GetBytes(originalContent));
			}
			request.Body = body;
		}
		await _next(context);
	}
}
