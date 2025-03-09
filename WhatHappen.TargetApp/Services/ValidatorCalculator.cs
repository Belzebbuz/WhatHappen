using WhatHappen.Core.Tracing;

namespace WhatHappen.TargetApp.Services;


public class ValidatorCalculator(IServiceProvider provider) : IValidatorCalculator
{
	public async Task<int> CalculateValidation(HelloRequest request)
	{
		var keyedService = provider.GetRequiredKeyedService<IKeyedValidation>("a-value");
		var result = await keyedService.ValidateKeyAsync(request.Name, 123);
		return result.Id.GetHashCode();
	}
}
[Track]
public interface IKeyedValidation
{
	public Task<KeyedResponse> ValidateKeyAsync(string name, int value);
}

public class KeyedValidation : IKeyedValidation
{
	public async Task<KeyedResponse> ValidateKeyAsync(string name, int value)
	{
		await Task.CompletedTask;
		return new KeyedResponse(name, Guid.NewGuid());
	}
}

public readonly record struct KeyedResponse(string Value, Guid Id);