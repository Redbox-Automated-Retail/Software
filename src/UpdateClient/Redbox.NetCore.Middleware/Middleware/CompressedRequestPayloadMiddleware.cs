using System;
using System.IO.Compression;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

namespace Redbox.NetCore.Middleware.Middleware;

public class CompressedRequestPayloadMiddleware
{
	private static class Headers
	{
		public static class ContentEncodingValues
		{
			public const string GZip = "gzip";

			public const string Deflate = "deflate";

			public static bool Contains(string value)
			{
				if (!(value == "gzip"))
				{
					return value == "deflate";
				}
				return true;
			}
		}

		public const string ContentEncoding = "Content-Encoding";
	}

	private readonly RequestDelegate next;

	public CompressedRequestPayloadMiddleware(RequestDelegate next)
	{
		this.next = next ?? throw new ArgumentNullException("next");
	}

	public async Task Invoke(HttpContext context)
	{
		if (context.Request.Headers.Keys.Contains("Content-Encoding"))
		{
			StringValues stringValues = context.Request.Headers["Content-Encoding"];
			if (Headers.ContentEncodingValues.Contains(stringValues))
			{
				string text = stringValues;
				if (!(text == "gzip"))
				{
					if (text == "deflate")
					{
						context.Request.Body = new DeflateStream(context.Request.Body, CompressionMode.Decompress, leaveOpen: true);
					}
				}
				else
				{
					context.Request.Body = new GZipStream(context.Request.Body, CompressionMode.Decompress, leaveOpen: true);
				}
			}
		}
		await next(context);
	}
}
