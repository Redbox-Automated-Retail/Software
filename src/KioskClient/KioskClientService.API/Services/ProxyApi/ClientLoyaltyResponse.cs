using System;
using System.Collections.Generic;
using System.Net;
using KioskClientService.API.App;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace KioskClientService.API.Services.ProxyApi;

public class ClientLoyaltyResponse : BaseLoyaltyResponse
{
	[JsonIgnore]
	public HttpStatusCode StatusCode { get; set; }

	public Guid MessageId { get; set; }

	public long KioskId { get; set; }

	public bool Success { get; private set; } = true;

	public List<Error> Errors { get; set; } = new List<Error>();

	public ClientLoyaltyResponse(Guid messageId, long kioskId)
	{
		MessageId = messageId;
		KioskId = kioskId;
	}

	public void FromServerResponse(ServerLoyaltyResponse source)
	{
		Success = true;
		Errors.Clear();
		Items.Clear();
		Items.AddRange(source.Items);
	}

	public void SetError(string code, string message)
	{
		Success = false;
		Errors.Add(new Error
		{
			Code = code,
			Message = message
		});
	}

	public ObjectResult ToObjectResult()
	{
		return new ObjectResult(this)
		{
			StatusCode = (int)StatusCode
		};
	}
}
