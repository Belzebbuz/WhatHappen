using Microsoft.EntityFrameworkCore;
using WhatHappen.Core.Tracing;
using WhatHappen.OtherService;
using WhatHappen.TargetApp.Context;

namespace WhatHappen.TargetApp.Services;

public class Validator(IValidatorCalculator calculator, OtherGreeter.OtherGreeterClient client, AppDbContext context) : IValidator
{
	public async Task<bool> IsValidAsync(HelloRequest request)
	{
		var result = await calculator.CalculateValidation(request);
		var parameters = GetParameters();
		if (result % 2 == 0) 
			return await context.Datas.AnyAsync();
		
		var response = await client.SayHelloAsync(new OtherService.HelloRequest()
		{
			Name = parameters
		});
		var verif = TimeVerifier.IsVerified();
		return verif && response.GetHashCode() % 2 == 0;
	}

	private string GetParameters()
	{
		var result = new Random().Next(100000000).ToString();
		return result;
	}
}

public static class TimeVerifier
{
	public static bool IsVerified() => true;
}