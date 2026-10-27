using System.Collections.Generic;
using Newtonsoft.Json;

namespace Redbox.Rental.Model.KioskClientService.Campaign;

[JsonConverter(typeof(ControlListConverter))]
public class ControlList : List<IControl>
{
}
