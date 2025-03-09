using WhatHappen.Core;
using WhatHappen.Core.Tracing;

namespace WhatHappen.TargetApp.Services;
[Track]
public interface IValidatorCalculator
{
	Task<int> CalculateValidation(HelloRequest request);
}