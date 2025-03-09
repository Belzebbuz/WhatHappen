using Grpc.Core;
using WhatHappen.OtherService;

namespace WhatHappen.OtherService.Services;

public class GreeterService : OtherGreeter.OtherGreeterBase
{
	private readonly ILogger<GreeterService> _logger;

	public GreeterService(ILogger<GreeterService> logger)
	{
		_logger = logger;
	}

	public override Task<HelloReply> SayHello(HelloRequest request, ServerCallContext context)
	{
		return Task.FromResult(new HelloReply
		{
			Message = "Hello " + request.Name
		});
	}
}