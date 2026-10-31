using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;

namespace Redbox.Remoting.Nrbf;

/// <summary>
/// A .NET Remoting call over the HTTP channel with the binary formatter (HttpChannel + BinaryClientFormatterSinkProvider),
/// without .NET Remoting: the request body is a BinaryMethodCall, the response body a BinaryMethodReturn. This is what
/// KioskServices' HttpChannel client sent to MessageService.rem; the self-hosted backend hands such requests to a real
/// .NET Remoting host (Mono), so the bytes have to be what .NET wrote.
/// </summary>
internal static class RemotingHttpClient
{
	private static readonly HttpClient Client = new HttpClient(new SocketsHttpHandler { UseCookies = false })
	{
		Timeout = Timeout.InfiniteTimeSpan
	};

	/// <summary>Calls <paramref name="methodName"/> on the remote object at <paramref name="url"/> and returns its return value.</summary>
	/// <exception cref="RemotingHttpException">The server returned an exception or an unexpected response.</exception>
	public static object Invoke(string url, string typeName, string methodName, object[] args, TimeSpan timeout)
	{
		byte[] body = NrbfWriter.MethodCall(methodName, typeName, args);
		using HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, url);
		request.Content = new ByteArrayContent(body);
		// The headers .NET's HttpClientTransportSink sent with a binary-formatted message.
		request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
		request.Headers.TryAddWithoutValidation("User-Agent", "Mozilla/4.0+(compatible; MSIE 6.0; Windows " + Environment.OSVersion.Version + "; MS .NET Remoting; MS .NET CLR 4.0.30319.42000 )");
		using CancellationTokenSource cancellation = new CancellationTokenSource(timeout);
		using HttpResponseMessage response = Client.Send(request, cancellation.Token);
		byte[] content = response.Content.ReadAsByteArrayAsync(cancellation.Token).GetAwaiter().GetResult();
		// The remoting host answers a failed call with 500 and a serialized exception in the BinaryMethodReturn.
		if (content.Length == 0 || content[0] != 0)
		{
			throw new RemotingHttpException($"{methodName}: unexpected response {(int)response.StatusCode} {response.ReasonPhrase} ({content.Length} bytes)");
		}
		NrbfMethodReturn result = NrbfReader.ReadMethodReturn(content);
		if (result.IsException)
		{
			throw new RemotingHttpException($"{methodName}: the server returned an exception: {DescribeException(result.Exception)}");
		}
		return result.ReturnValue;
	}

	private static string DescribeException(object exception)
	{
		return exception is NrbfObject graph && graph.TryGetMember("Message", out object message) ? $"{graph.TypeName}: {message}" : exception?.ToString() ?? "(none)";
	}
}

internal sealed class RemotingHttpException : Exception
{
	public RemotingHttpException(string message)
		: base(message)
	{
	}
}
