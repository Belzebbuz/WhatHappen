using WhatHappen.Core;
using WhatHappen.Core.Tracing;

namespace WhatHappen.TargetApp.Services;

[Track]
public interface IValidator
{
	Task<bool> IsValidAsync(HelloRequest request);
}