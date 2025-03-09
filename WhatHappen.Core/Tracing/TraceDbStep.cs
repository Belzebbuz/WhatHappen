using System.Text.Json;

namespace WhatHappen.Core.Tracing;

public class TraceDbStep : TraceStep
{
	public override string Type => "DB";
	public string Sql { get; set; }
	public object Parameters { get; set; }
	
	public override TraceStepInfo ToTraceStepInfo()
	{
		return new TraceStepInfo()
		{
			MethodInfo = Sql,
			Input = JsonSerializer.Serialize(Parameters, Options),
			Output = string.Empty,
			Type = Type
		};
	}
	public override string ToJson()
	{
		return JsonSerializer.Serialize(this, Options);
	}
}