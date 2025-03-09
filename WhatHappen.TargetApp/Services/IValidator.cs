using WhatHappen.Core;
using WhatHappen.Core.Tracing;

namespace WhatHappen.TargetApp.Services;


public interface IValidator
{
	Task<bool> IsValidAsync(HelloRequest request);
}