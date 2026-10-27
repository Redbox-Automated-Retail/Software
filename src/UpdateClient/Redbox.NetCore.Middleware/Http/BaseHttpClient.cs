using System.Collections.Generic;
using System.Globalization;
using Redbox.NetCore.Middleware.Abstractions;
using Redbox.NetCore.Middleware.Extensions;

namespace Redbox.NetCore.Middleware.Http;

public abstract class BaseHttpClient
{
	public string ActivityId { get; private set; }

	public long? KioskId { get; private set; }

	public BaseHttpClient(IMiddlewareData middlewareData)
	{
		if (!string.IsNullOrEmpty(middlewareData.ActivityId))
		{
			ActivityId = middlewareData.ActivityId;
		}
		KioskId = middlewareData.KioskId;
	}

	protected List<Header> CreateHeaders(string bearer = null, string apikey = null, string applicationName = null)
	{
		List<Header> list = new List<Header>();
		if (bearer != null)
		{
			if (!bearer.StartsWith("bearer", ignoreCase: true, CultureInfo.InvariantCulture))
			{
				bearer = "Bearer " + bearer;
			}
			list.AddAuthorizationHeader(bearer);
		}
		if (apikey != null)
		{
			list.AddApiKeyHeader(apikey);
		}
		if (applicationName != null)
		{
			list.AddAppNameHeader(applicationName);
		}
		if (!string.IsNullOrEmpty(ActivityId))
		{
			list.AddActivityIdHeader(ActivityId);
		}
		if (KioskId.HasValue)
		{
			list.AddKioskIdHeader(KioskId.Value);
		}
		return list;
	}
}
