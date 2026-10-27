namespace Redbox.HAL.Client;

public static class HardwareJobExtensions
{
	public static string[] GetStackEntries(this HardwareJob job, int depth)
	{
		return GetStackEntriesInner(job, depth);
	}

	public static string GetTopOfStack(this HardwareJob job)
	{
		return GetStackEntriesInner(job, 1)[0];
	}

	private static string[] GetStackEntriesInner(HardwareJob job, int depth)
	{
		string[] array = new string[depth];
		for (int i = 0; i < depth; i++)
		{
			array[i] = string.Empty;
		}
		if (job.GetStack(out var stack).Success && stack.Count >= depth)
		{
			for (int j = 0; j < depth; j++)
			{
				array[j] = stack.Pop();
			}
		}
		return array;
	}
}
