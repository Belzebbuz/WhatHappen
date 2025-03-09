using System;
using System.Collections.Generic;

namespace WhatHappen.Core.Tracing;

public class Trace
{
	public Guid OperationId { get; } = Guid.NewGuid();
	public List<TraceStep> Steps { get; } = new();
}