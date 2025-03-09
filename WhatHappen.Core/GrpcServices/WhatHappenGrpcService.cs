using System;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Grpc.Core;
using WhatHappen.Core.Patching;
using WhatHappen.Core.Tracing;

namespace WhatHappen.Core.GrpcServices;

public class WhatHappenGrpcService : WhatHappenService.WhatHappenServiceBase
{
	public override async Task<GetTraceResponse> GetTrace(GetTraceRequest request, ServerCallContext context)
	{
		await Task.CompletedTask;
		var trace = TracingContext.GetTrace(Guid.Parse(request.OperationId));
		if (trace is not null)
		{
            var graph = TraceVisualizer.GenerateGraph(trace);
			return new GetTraceResponse()
			{
				Trace = {},
				GraphViz = Convert.ToBase64String(Encoding.UTF8.GetBytes(graph))
			};
		}
		return new GetTraceResponse();
	}

	public override async Task<ChangePatchResponse> ChangePatch(ChangePatchRequest request, ServerCallContext context)
	{
		await Task.CompletedTask;
		if(!request.EnableHarmonyPatch)
			HarmonyWhatHappenPatcher.Unpatch();
		return new ChangePatchResponse();
	}
}

