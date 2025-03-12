using Grpc.Core;
using WhatHappen.OtherService;
using WhatHappen.TargetApp.Context;
namespace WhatHappen.TargetApp.Services;

public class GreeterService(
	IValidator validator, 
	AppDbContext dbContext, 
	OtherGreeter.OtherGreeterClient client,
	ILogger<GreeterService> logger) : Greeter.GreeterBase
{

	public override async Task<HelloReply> SayHello(HelloRequest request, ServerCallContext context)
	{
		var result = await client.SayHelloAsync(new OtherService.HelloRequest()
		{
			Name = Guid.NewGuid().ToString()
		});
		var res = TimeVerifier.IsVerified();
		await dbContext.AddAsync(new Data()
		{
			Id = Guid.NewGuid(),
			Value = result.Message + res
		});
		await dbContext.SaveChangesAsync();
		var validation = await validator.IsValidAsync(request);
		
		return new HelloReply
		{
			Message = result.Message + validation
		};
	}
}