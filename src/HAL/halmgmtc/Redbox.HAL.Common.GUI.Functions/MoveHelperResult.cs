namespace Redbox.HAL.Common.GUI.Functions;

public class MoveHelperResult
{
	public readonly string Status;

	public string[] Limits { get; internal set; }

	public bool MoveOk => Status == "SUCCESS";

	internal MoveHelperResult(string msg)
	{
		Status = msg;
		Limits = new string[0];
	}
}
