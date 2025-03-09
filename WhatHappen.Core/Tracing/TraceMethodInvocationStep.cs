using System.Runtime.CompilerServices;
using System.Text.Json;

namespace WhatHappen.Core.Tracing;

public class TraceMethodInvocationStep : TraceStep, IAfterCallOutputSetter
{
	public override string Type => "Вызов метода";

	public string Class { get; init; }
	public string Method { get; init; }
	public object? Input { get; init; }
	public object? Output { get; set; }
	
	public void SetOutput(object? output) => Output = output;

	public override string ToJson()
	{
		return JsonSerializer.Serialize(this, Options);
	}
}