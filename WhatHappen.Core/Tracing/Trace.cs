using System;
using System.Collections.Generic;

namespace WhatHappen.Core.Tracing;

public class Trace
{
	public Guid OperationId { get; } = Guid.NewGuid();
	public TraceStep? RootStep { get; set; }
	public Dictionary<string, TraceStep> StepMap { get; } = new();
	public int StepCounter = 0;
}