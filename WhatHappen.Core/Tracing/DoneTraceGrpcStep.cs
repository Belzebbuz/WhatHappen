using System.Text.Json;

namespace WhatHappen.Core.Tracing;

public class DoneTraceGrpcStep : TraceStep
{
	public override string Type => "Конец отслеживания gRPC";
	public string Method { get; set; }
	public object Response { get; set; }
	
	public override TraceStepInfo ToTraceStepInfo()
	{
		return new TraceStepInfo()
		{
			MethodInfo = Method,
			Input = string.Empty,
			Output =JsonSerializer.Serialize(Response, Options),
			Type = Type
		};
	}

	public override string ToJson()
	{
		return JsonSerializer.Serialize(this, Options);
	}
}