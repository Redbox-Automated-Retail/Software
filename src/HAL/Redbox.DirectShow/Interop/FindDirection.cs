using System;
using System.Runtime.InteropServices;

namespace Redbox.DirectShow.Interop;

[ComVisible(false)]
public static class FindDirection
{
	public static readonly Guid UpstreamOnly = new Guid(2893646816u, 39139, 4561, 179, 241, 0, 170, 0, 55, 97, 197);

	public static readonly Guid DownstreamOnly = new Guid(2893646817u, 39139, 4561, 179, 241, 0, 170, 0, 55, 97, 197);
}
