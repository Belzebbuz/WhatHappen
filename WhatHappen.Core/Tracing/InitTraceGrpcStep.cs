﻿using System.Text.Json;

namespace WhatHappen.Core.Tracing;

internal class InitTraceGrpcStep : TraceStep, IAfterCallOutputSetter
{
	public override string Type => "Инициализация отслеживания gRPC";
	public required string  Method { get; set; }
	public required object Request { get; set; }
	public object? Response { get; set; }

	public override string ToJson()
	{
		return JsonSerializer.Serialize(this, Options);
	}

	public void SetOutput(object? output)
	{
		Response = output;
	}
}