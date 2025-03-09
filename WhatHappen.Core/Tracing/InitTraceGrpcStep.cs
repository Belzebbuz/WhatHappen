using System.Text.Json;

namespace WhatHappen.Core.Tracing;

public class InitTraceGrpcStep : TraceStep
{
	public override string Type => "Инициализация отслеживания gRPC";
	public string Method { get; set; }
	public object Request { get; set; }
	
	public override TraceStepInfo ToTraceStepInfo()
	{
		return new TraceStepInfo()
		{
			MethodInfo = Method,
			Input = JsonSerializer.Serialize(Request, Options),
			Output = string.Empty,
			Type = Type
		};
	}
	public override string ToJson()
	{
		return JsonSerializer.Serialize(this, Options);
	}
}