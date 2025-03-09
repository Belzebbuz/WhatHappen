using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using WhatHappen.Core.Tracing;

namespace WhatHappen.Core.GrpcServices;

public static class TraceVisualizer
{
	private static readonly Dictionary<Type, (string shape, string color)> NodeStyles = new()
	{
		{ typeof(TraceMethodInvocationStep), ("box", "#d0f0c0") },// Зеленый
		{ typeof(TraceDbStep), ("cylinder", "#fff3b0") },// Желтый
		{ typeof(TraceGrpcCallStep), ("component", "#ffcccb") },// Красный
		{ typeof(InitTraceGrpcStep), ("ellipse", "#e0e0e0") }, // Серый
		{ typeof(DoneTraceGrpcStep), ("ellipse", "#e0e0e0") }  // Серый
	};

	public static string GenerateGraph(Trace trace)
	{
		var sb = new StringBuilder();
		sb.AppendLine("digraph TraceGraph {");
		sb.AppendLine("rankdir=TB;");
		sb.AppendLine("node [style=filled, fontname=\"Segoe UI\"];");
		sb.AppendLine("edge [arrowsize=0.8];");

		var steps = trace.Steps;
		for (var i = 0; i < steps.Count; i++)
		{
			var step = steps[i];
			var nodeId = step.StepId;
			var style = GetNodeStyle(step);
            
			sb.AppendLine($"{nodeId} [");
			sb.AppendLine($"label=\"{GetStepLabel(step)}\"");
			sb.AppendLine($"tooltip=\"{GetTooltip(step)}\"");
			sb.AppendLine($"shape={style.shape}");
			sb.AppendLine($"fillcolor=\"{style.color}\"");
			sb.AppendLine("];");

			if (i <= 0) continue;
            
			var prevStepId = steps[i - 1].StepId;
			sb.AppendLine($"{prevStepId} -> {nodeId};");
		}

		sb.AppendLine("}");
		return sb.ToString();
	}

	private static string GetTooltip(TraceStep step)
	{
		return step.ToJson().Replace("\"","\\\"").Replace(@"\\""","\\\"");
	}

	private static string GetStepLabel(TraceStep step)
	{
		return step switch
		{
			TraceMethodInvocationStep methodStep => 
				$"{methodStep.Class.Split('.')[^1]}\\n{methodStep.Method}",
            
			TraceDbStep dbStep => 
				$"DB Query\n{dbStep.Sql.Truncate(30).Replace("\n", " ").Replace("\"","\\\"")}",
            
			TraceGrpcCallStep grpcStep => 
				$"Исходящий gRPC запрос\n{grpcStep.Service}/{grpcStep.Method}",
            
			InitTraceGrpcStep initStep => 
				$"Начало обработки gRPC запроса\n{initStep.Method}",
            
			DoneTraceGrpcStep doneStep => 
				$"Конец обработки gRPC запроса\n{doneStep.Method}",
            
			_ => step.Type
		};
	}

	private static (string shape, string color) GetNodeStyle(TraceStep step)
	{
		return NodeStyles.TryGetValue(step.GetType(), out var style) 
			? style 
			: ("box", "#ffffff");
	}
}