using System.Collections.Generic;
using Newtonsoft.Json;

namespace KioskClientService.API.Services.Campaigns;

[JsonConverter(typeof(ControlListConverter))]
public class ControlList : List<IControl>
{
}
