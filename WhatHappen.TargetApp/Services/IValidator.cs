using DecoratorsGenerators;
using WhatHappen.Core.Tracing;

namespace WhatHappen.TargetApp.Services;

[Track]
[GenerateTrack]
public interface IValidator
{
	Task<bool> IsValidAsync(HelloRequest request);
	Task IsTaskAsync(HelloRequest request);
	Task<(bool, string)> IsTaskTupleAsync(HelloRequest request);
	IReadOnlyCollection<string> IsGenericAsync(string param1, bool param2);
	string IsStringAsync(Func<string, Task<bool>> param1);
	void VoidMethod(out int param2, string? param1 = null);
	TResult IsHardGenericAsync<T, TResult>(T param1, params string[] args)
		where T : class
		where TResult : notnull;
}

[GenerateTrack]
public interface IGenericsValidator<in TRequest, out TResult> 
	where TRequest : class 
	where TResult : notnull
{
	public IAsyncEnumerable<TResult> GetAsync(TRequest request, string? filter);
	public IEnumerable<TResult> Get();
}

public record GenRequest(string Value);
public record GenRequestV2(string Value);

public readonly record struct GenResponse(string Response);

public class GenericsValidator : IGenericsValidator<GenRequest, GenResponse> 
{
	public async IAsyncEnumerable<GenResponse> GetAsync(GenRequest request, string? filter)
	{
		await Task.CompletedTask;
		yield return new GenResponse("Result 1");
		yield return new GenResponse("Result 2");
		yield return new GenResponse("Result 3");
	}

	public IEnumerable<GenResponse> Get()
	{
		yield return new GenResponse("2e23");
	}
} 
public class GenericsValidatorV3 : IGenericsValidator<GenRequest, GenResponse> 
{
	public async IAsyncEnumerable<GenResponse> GetAsync(GenRequest request, string? filter)
	{
		await Task.Yield();
		yield return new GenResponse("Result 1");
		await Task.Yield();
		yield return new GenResponse("Result 2");
		await Task.Yield();
		yield return new GenResponse("Result 3");
	}

	public IEnumerable<GenResponse> Get()
	{
		yield return new GenResponse("2e23");
	}
} 
public class GenericsValidatorV2 : IGenericsValidator<GenRequestV2, GenResponse> 
{
	public async IAsyncEnumerable<GenResponse> GetAsync(GenRequestV2 request, string? filter)
	{
		await Task.CompletedTask;
		yield return new GenResponse("Result 1");
		yield return new GenResponse("Result 2");
		yield return new GenResponse("Result 3");
	}

	public IEnumerable<GenResponse> Get()
	{
		yield return new GenResponse("2e23");
	}
} 