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
		// var result = await client.SayHelloAsync(new OtherService.HelloRequest()
		// {
		// 	Name = Guid.NewGuid().ToString()
		// });
		var res = TimeVerifier.IsVerified();
		var data = new Data()
		{
			Id = Guid.NewGuid(),
			Value = "dwfwef"
		};
		await dbContext.AddAsync(data);
		await dbContext.SaveChangesAsync();
		data.InfoValues = new();
		data.InfoValues.Add(new InfoValue());
		await dbContext.SaveChangesAsync();
		// var validation = await validator.IsValidAsync(request);
		
		return new HelloReply
		{
			Message = "fewfewf"
		};
	}
}