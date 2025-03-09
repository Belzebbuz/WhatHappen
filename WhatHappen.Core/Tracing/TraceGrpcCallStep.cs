using System.Text.Json;

namespace WhatHappen.Core.Tracing;

public class TraceGrpcCallStep : TraceStep
{
	public override string Type => "gRPC";
	public override bool IsExternal { get; } = true;
	public string Service { get; set; }
	public string Method { get; set; }
	public object Request { get; set; }
	public object Response { get; set; }
	
	public override string ToJson()
	{
		return JsonSerializer.Serialize(this, Options);
	}
}