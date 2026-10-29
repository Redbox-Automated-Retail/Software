using System.Collections;
using System.Collections.Generic;

namespace Redbox.Extensibility.ComponentModel;

public interface ICommandBarList : IList<ICommandBar>, ICollection<ICommandBar>, IEnumerable<ICommandBar>, IEnumerable
{
}
