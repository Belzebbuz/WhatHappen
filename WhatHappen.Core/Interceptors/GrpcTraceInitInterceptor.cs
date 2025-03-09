using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Grpc.Core;
using Grpc.Core.Interceptors;
using Microsoft.Extensions.Logging;
using WhatHappen.Core.Tracing;

namespace WhatHappen.Core.Interceptors;

internal sealed class GrpcTraceInitInterceptor: Interceptor
{
	public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(TRequest request, ServerCallContext context,
		UnaryServerMethod<TRequest, TResponse> continuation)
	{
		var usingTrace = context.RequestHeaders.FirstOrDefault(x => x.Key == "x-what-happen")?.Value;
		if(usingTrace is null || !usingTrace.Equals("true", System.StringComparison.InvariantCultureIgnoreCase))
			return await base.UnaryServerHandler(request, context, continuation);
		
		return await HandleWithTracing(request, context, continuation);
	}

	private async Task<TResponse> HandleWithTracing<TRequest, TResponse>(TRequest request, ServerCallContext context,
		UnaryServerMethod<TRequest, TResponse> continuation) where TRequest : class where TResponse : class
	{
		TracingContext.StartNewTrace();
		var initStep = new InitTraceGrpcStep()
		{
			Method = context.Method,
			Request = request,
			IsCompleted = false
		};
		TracingContext.AddStep(initStep);
		
		
		var result = await base.UnaryServerHandler(request, context, continuation);
		
		
		TracingContext.CompleteStep(result, initStep.StepId);
		var operationId = TracingContext.GetCurrentTrace()?.OperationId.ToString();
		if(operationId is not null)
			context.ResponseTrailers.Add(new Metadata.Entry("x-what-happen-id", operationId));
		return result;
	}
}