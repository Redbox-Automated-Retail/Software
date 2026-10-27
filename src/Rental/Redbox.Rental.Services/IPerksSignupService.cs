using System;
using Redbox.Rental.Model.Personalization;

namespace Redbox.Rental.Services;

public interface IPerksSignupService
{
	void StartSignupFlow(Action<ISignupFlowResult> callbackAction, SignUpFlowContext signUpFlowContext, Action<ISignupFlowResult> timeoutCallbackAction);
}
