using System.Collections;
using System.Collections.Generic;

namespace Redbox.Rental.Model.Session;

public interface ISessionStartTriggerActionList : IList<ISessionStartTriggerAction>, ICollection<ISessionStartTriggerAction>, IEnumerable<ISessionStartTriggerAction>, IEnumerable
{
	ISessionStartTriggerAction FindByName(string name);
}
