using DecoratorsGenerators;
using WhatHappen.Core;
using WhatHappen.Core.Tracing;

namespace WhatHappen.TargetApp.Services;
[Track]
[GenerateTrack]
public interface IValidatorCalculator
{
	Task<int> CalculateValidation(HelloRequest request);
}