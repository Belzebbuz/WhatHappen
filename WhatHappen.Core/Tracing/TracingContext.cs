using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace WhatHappen.Core.Tracing;

public static class TracingContext
{
	private static readonly AsyncLocal<Trace> CurrentTrace = new();
	private static readonly ConcurrentDictionary<Guid, Trace> Traces = new();
	public static void StartNewTrace()
	{
		var trace = new Trace();
		CurrentTrace.Value = trace;
		Traces[trace.OperationId] = trace;
	}

	public static void AddStep(TraceStep step)
	{
		if(CurrentTrace.Value is null)
			return;
		var stepId = CurrentTrace.Value.Steps.Count + 1;

		step.StepId = $"step_{stepId}";

		
		CurrentTrace.Value.Steps.Add(step);
	}

	public static Trace? GetCurrentTrace() => CurrentTrace.Value;
	public static Trace? GetTrace(Guid operationId) => Traces.GetValueOrDefault(operationId);

	public static void SetOutput(object? output, Guid operationId, string stepId)
	{
		var trace = Traces.GetValueOrDefault(operationId);
		var step = trace?.Steps.FirstOrDefault(x => x.StepId == stepId);
		if(step is IAfterCallOutputSetter setter)
			setter?.SetOutput(output);
	}
}