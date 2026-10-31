using System;
using System.Collections;
using System.Collections.Generic;

namespace Redbox.Extensibility.ComponentModel;

public interface ICommandList : IList<ICommand>, ICollection<ICommand>, IEnumerable<ICommand>, IEnumerable
{
	ICommand[] ToArray();

	ICommand GetById(int id);

	bool RemoveById(int id);

	ICommand[] GetByName(string name);

	ICommand GetByGroupAndId(Guid groupId, int id);
}
