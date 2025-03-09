using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace WhatHappen.Core.Tracing;

internal static class TracingContext
{
	private static readonly AsyncLocal<Trace> CurrentTrace = new();
	private static readonly AsyncLocal<Stack<TraceStep>> CallStack = new();
	private static readonly ConcurrentDictionary<Guid, Trace> Traces = new();
	public static Trace? GetCurrentTrace() => CurrentTrace.Value;
	public static Trace? GetTrace(Guid operationId) => Traces.GetValueOrDefault(operationId);
	
	public static void StartNewTrace()
	{
		var trace = new Trace();
		CurrentTrace.Value = trace;
		CallStack.Value = new Stack<TraceStep>();
		Traces[trace.OperationId] = trace;
	}

	
	public static void AddStep(TraceStep step)
	{
		if(CurrentTrace.Value is null || CallStack.Value is null)
			return;
		
		
		var stepId = ++CurrentTrace.Value.StepCounter;
		step.StepId = $"step_{stepId}";
		CurrentTrace.Value.StepMap[step.StepId] = step;

		if (CallStack.Value.TryPeek(out var parent))
		{
			step.ParentStepId = parent.StepId;
			parent.Children.Add(step);
		}
		else
		{
			CurrentTrace.Value.RootStep = step;
		}

		if (step.IsExternal) return;
		CallStack.Value.Push(step);
	}
	

	public static void CompleteStep(object? output,  string stepId)
	{
		if (CurrentTrace.Value?.StepMap.TryGetValue(stepId, out var step) == true 
		    && step is IAfterCallOutputSetter setter)
		{
			step.IsCompleted = true;
			setter.SetOutput(output);
			CallStack.Value?.TryPop(out _);
		}
	}
}