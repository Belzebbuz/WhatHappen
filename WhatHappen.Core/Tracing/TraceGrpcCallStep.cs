using System.Text.Json;

namespace WhatHappen.Core.Tracing;

public class TraceGrpcCallStep : TraceStep
{
	public override string Type => "gRPC";

	public string Service { get; set; }
	public string Method { get; set; }
	public object Request { get; set; }
	public object Response { get; set; }
	
	public override TraceStepInfo ToTraceStepInfo()
	{
		return new TraceStepInfo()
		{
			MethodInfo = $"Сервис: {Service} -- Метод: {Method}",
			Input = JsonSerializer.Serialize(Request, Options),
			Output = JsonSerializer.Serialize(Response, Options),
			Type = Type
		};
	}
	public override string ToJson()
	{
		return JsonSerializer.Serialize(this, Options);
	}
}