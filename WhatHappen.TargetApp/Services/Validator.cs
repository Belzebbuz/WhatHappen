using Microsoft.EntityFrameworkCore;
using WhatHappen.Core.Tracing;
using WhatHappen.OtherService;
using WhatHappen.TargetApp.Context;

namespace WhatHappen.TargetApp.Services;

public class Validator(
	IValidatorCalculator calculator, 
	OtherGreeter.OtherGreeterClient client, 
	AppDbContext context) : IValidator
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

	public Task IsTaskAsync(HelloRequest request)
	{
		throw new NotImplementedException();
	}

	public Task<(bool, string)> IsTaskTupleAsync(HelloRequest request)
	{
		throw new NotImplementedException();
	}

	public IReadOnlyCollection<string> IsGenericAsync(string param1, bool param2)
	{
		throw new NotImplementedException();
	}

	public string IsStringAsync(Func<string, Task<bool>> param1)
	{
		

		throw new NotImplementedException();
	}

	public void VoidMethod(out int param2, string? param1 = null)
	{
		throw new NotImplementedException();
	}
	public void VoidMethod(out int param2, in int param1)
	{
		VoidMethod(out param2,  param1);
	}
	public TResult IsHardGenericAsync<T, TResult>(T param1, params string[] args) where T : class where TResult : notnull
	{
		throw new NotImplementedException();
	}

	public void VoidMethod(string param1)
	{
		throw new NotImplementedException();
	}

	public TResult IsHardGenericAsync<T, TResult>(T param1) where T : class where TResult : notnull
	{
		throw new NotImplementedException();
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