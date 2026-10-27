using System;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Script.Framework;

[NativeJob(ProgramName = "test-boards-job")]
internal class TestBoardsJob : NativeJobAdapter
{
	protected override void ExecuteInner()
	{
		IControlSystemRevision revision = base.ControlSystem.GetRevision();
		Array.ForEach(revision.Responses, delegate(IBoardVersionResponse each)
		{
			string arg = (each.ReadSuccess ? "responsive" : "not responsive");
			base.Context.CreateInfoResult("VersionResult", $"{each.BoardName.ToString().ToUpper()} board {arg}");
		});
		if (!revision.Success)
		{
			AddError("One or more comm failures.");
		}
	}

	internal TestBoardsJob(ExecutionResult result, ExecutionContext ctx)
		: base(result, ctx)
	{
	}
}
