using WhatHappen.Core;
using WhatHappen.Core.Tracing;

namespace WhatHappen.TargetApp.Services;

public interface IValidatorCalculator
{
	Task<int> CalculateValidation(HelloRequest request);
}