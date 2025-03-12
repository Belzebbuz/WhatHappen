using System.Text.Json;

namespace WhatHappen.Core.Tracing;

internal class TraceGrpcCallStep : TraceStep
{
	public override string Type => "gRPC";
	public override bool IsExternal { get; } = true;
	public required string Service { get; set; }
	public required string Method { get; set; }
	public required object Request { get; set; }
	public required object Response { get; set; }
	
	public override string ToJson()
	{
		return JsonSerializer.Serialize(this, Options);
	}
}