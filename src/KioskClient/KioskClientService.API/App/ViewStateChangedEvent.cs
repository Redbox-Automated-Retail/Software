using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace KioskClientService.API.App;

public class ViewStateChangedEvent : EventArgs, IMessageScrub
{
	[JsonConverter(typeof(StringEnumConverter))]
	public ViewStateChangedType Type { get; set; }

	public string FromView { get; set; }

	public string ToView { get; set; }

	public object Scrub()
	{
		return this.ToJson();
	}
}
