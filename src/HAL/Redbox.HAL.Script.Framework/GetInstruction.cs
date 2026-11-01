using System;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Controller.Framework;
using Redbox.HAL.Core;

namespace Redbox.HAL.Script.Framework;

[Serializable]
public sealed class GetInstruction : Instruction
{
	private sealed class GetInstructionObserver : IGetObserver
	{
		private readonly bool TestSlot;

		public bool OnEmpty(IGetResult gr)
		{
			if (!TestSlot)
			{
				return true;
			}
			using PeekOperation peekOperation = new PeekOperation();
			IPeekResult peekResult = peekOperation.Execute();
			if (!peekResult.TestOk || peekResult.IsFull)
			{
				gr.Update(ErrorCodes.ItemStuck);
				ServiceLocator.Instance.GetService<IInventoryService>().UpdateEmptyStuck(gr.Location);
				return false;
			}
			return true;
		}

		public void OnStuck(IGetResult gr)
		{
			if (TestSlot)
			{
				ServiceLocator.Instance.GetService<IInventoryService>().UpdateEmptyStuck(gr.Location);
			}
		}

		internal GetInstructionObserver(bool test)
		{
			TestSlot = test && ControllerConfiguration.Instance.TestSlotOnEmpty;
		}
	}

	public override string Mnemonic => "GET";

	protected override ExpectedOperands Expected => ExpectedOperands.Optional;

	protected override void ParseOperands(TokenList operandTokens, ExecutionResult result)
	{
		if (operandTokens.Count > 0 && !operandTokens[0].IsKeyValuePair)
		{
			result.AddInvalidOperandError($"The {Mnemonic} instruction expects an optional key-value pair.");
		}
	}

	protected override void OnExecute(ExecutionResult result, ExecutionContext context)
	{
		KeyValuePair peekPair = base.Operands.GetKeyValuePair("PEEK");
		if (peekPair != null)
		{
			bool.TryParse(peekPair.Value, out var _);
		}
		bool center = false;
		KeyValuePair centerPair = base.Operands.GetKeyValuePair("CENTER");
		if (centerPair != null && bool.TryParse(centerPair.Value, out var parsedCenter))
		{
			center = parsedCenter;
		}
		bool test = false;
		KeyValuePair testOnEmptyPair = base.Operands.GetKeyValuePair("TESTONEMPTY");
		if (testOnEmptyPair != null && bool.TryParse(testOnEmptyPair.Value, out var parsedTest))
		{
			test = parsedTest;
		}
		bool pushReturnTime = false;
		KeyValuePair getReturnTimePair = base.Operands.GetKeyValuePair("GET-RT");
		if (getReturnTimePair != null && bool.TryParse(getReturnTimePair.Value, out var parsedGetReturnTime))
		{
			pushReturnTime = parsedGetReturnTime;
		}
		if (context.IsImmediate)
		{
			test = true;
		}
		GetInstructionObserver observer = new GetInstructionObserver(test);
		IGetResult getResult = ServiceLocator.Instance.GetService<IControllerService>().Get(observer);
		if (getResult.Success & center)
		{
			ServiceLocator.Instance.GetService<IControlSystem>().Center(CenterDiskMethod.VendDoorAndBack);
		}
		if (pushReturnTime)
		{
			context.PushTop(GetDateRepresentation(getResult.ReturnTime));
		}
		context.PushTop(getResult.Previous);
		context.PushTop(getResult.ToString().ToUpper());
	}
}
