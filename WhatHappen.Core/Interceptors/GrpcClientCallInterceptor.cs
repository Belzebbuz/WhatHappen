using System.Threading.Tasks;
using Grpc.Core;
using Grpc.Core.Interceptors;
using WhatHappen.Core.Tracing;

namespace WhatHappen.Core.Interceptors;

internal sealed class GrpcClientCallInterceptor : Interceptor
{
	public override AsyncUnaryCall<TResponse> AsyncUnaryCall<TRequest, TResponse>(TRequest request, ClientInterceptorContext<TRequest, TResponse> context,
		AsyncUnaryCallContinuation<TRequest, TResponse> continuation)
	{
		var call = continuation(request, context);

		return new AsyncUnaryCall<TResponse>(
			HandleResponse(call.ResponseAsync, request, context),
			call.ResponseHeadersAsync,
			call.GetStatus,
			call.GetTrailers,
			call.Dispose);
	}
	
	private async Task<TResponse> HandleResponse<TRequest, TResponse>(
		Task<TResponse> inner,
		TRequest request,
		ClientInterceptorContext<TRequest, TResponse> context) 
		where TRequest : class where TResponse : class
	{
		var response = await inner;
		var grpcCallStep = new TraceGrpcCallStep()
		{
			Method = context.Method.Name,
			Request = request,
			Response = response,
			Service = context.Method.ServiceName,
			IsCompleted = true
		};
		TracingContext.AddStep(grpcCallStep);
		return response;
	}
}