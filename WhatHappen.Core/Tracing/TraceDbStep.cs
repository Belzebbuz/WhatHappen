using System.Text.Json;

namespace WhatHappen.Core.Tracing;

internal class TraceDbStep : TraceStep
{
	public override string Type => "DB";
	public string Sql { get; set; }
	public object Parameters { get; set; }
	public override bool IsExternal { get; } = true;

	public override string ToJson()
	{
		return JsonSerializer.Serialize(this, Options);
	}
}