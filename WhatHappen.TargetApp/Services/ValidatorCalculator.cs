using DecoratorsGenerators;
using WhatHappen.Core.Tracing;

namespace WhatHappen.TargetApp.Services;


public class ValidatorCalculator(
	IServiceProvider provider, 
	IGenericsValidator<GenRequest, GenResponse> genValidator,
	IGenericsValidator<GenRequestV2, GenResponse> genValidatorV2
	) : IValidatorCalculator
{
	public async Task<int> CalculateValidation(HelloRequest request)
	{
		await foreach (var resp in genValidator.GetAsync(new GenRequest("VA"), "asf"))
		{
			Console.WriteLine(resp);			
		}
		await foreach (var resp in genValidatorV2.GetAsync(new GenRequestV2("VA"), "asf"))
		{
			Console.WriteLine(resp);			
		}
		var keyedService = provider.GetRequiredKeyedService<IKeyedValidation>("a-value");
		var keyedServiceA = provider.GetRequiredKeyedService<IGenericsValidator<GenRequest, GenResponse>>("a-value");
		await foreach (var resp in keyedServiceA.GetAsync(new GenRequest("VA"), "asf"))
		{
			Console.WriteLine(resp);			
		}

		var resr = keyedServiceA.Get();
		Console.WriteLine(resr.First());
		var result = await keyedService.ValidateKeyAsync(request.Name, 123);
		return result.Id.GetHashCode();
	}
}
[Track]
[GenerateTrack]
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