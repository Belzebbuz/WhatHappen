using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using WhatHappen.Core.Tracing;

namespace WhatHappen.Core.GrpcServices;

internal static class TraceVisualizer
{
	private static readonly Dictionary<Type,string> NodeStyles = new()
	{
		{ typeof(TraceMethodInvocationStep), ("fillcolor=lightcyan") },// Зеленый
		{ typeof(TraceDbStep), ("fillcolor=lightyellow") },// Желтый
		{ typeof(TraceGrpcCallStep), ("fillcolor=lightyellow") },// Красный
		{ typeof(InitTraceGrpcStep), ("fillcolor=lightgreen") }, // Серый
	};

	public static string GenerateGraph(Trace trace)
	{
		var sb = new StringBuilder();
		sb.AppendLine("digraph TraceGraph {");
		sb.AppendLine("rankdir=LR;");
		sb.AppendLine("node [shape=record, style=filled, fillcolor=lightcyan];");
		sb.AppendLine("edge [arrowhead=vee];");
		BuildNodes(sb, trace.RootStep);
		BuildConnections(sb, trace.RootStep);
		sb.AppendLine("}");
		return sb.ToString();
	}

	private static void BuildConnections(StringBuilder sb, TraceStep step)
	{
		foreach (var child in step.Children)
		{
			var style = child.IsExternal ? "[style=dashed]" : "";
			sb.AppendLine($"    {step.StepId} -> {child.StepId} {style};");
			BuildConnections(sb, child);
		}
	}

	private static void BuildNodes(StringBuilder sb, TraceStep step)
	{

		var tooltip = $"tooltip=\"{GetTooltip(step)}\"";
		sb.AppendLine($"{step.StepId} [label=\"{GetStepLabel(step)}\"{tooltip} {GetNodeStyle(step)}];");
		foreach (var child in step.Children)
		{
			BuildNodes(sb, child);
		}
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
				$"{step.StepId}|Вызов метода |{methodStep.Class.Split('.')[^1]}.{methodStep.Method}",
            
			TraceDbStep dbStep => 
				$"{step.StepId}|DB Query|{dbStep.Sql.Truncate(30).Replace("\n", " ").Replace("\"","\\\"")}",
            
			TraceGrpcCallStep grpcStep => 
				$"{step.StepId}|Исходящий gRPC запрос|{grpcStep.Service}/{grpcStep.Method}",
            
			InitTraceGrpcStep initStep => 
				$"{step.StepId}|Начало обработки gRPC запроса|{initStep.Method}",
            
			_ => step.Type
		};
	}

	private static string GetNodeStyle(TraceStep step)
	{
		return NodeStyles.GetValueOrDefault(step.GetType(), "#ffffff");
	}
}

public static class StringExtensions
{
	public static string Truncate(this string value, int maxLength)
	{
		return value.Length <= maxLength 
			? value 
			: value[..(maxLength-3)] + "...";
	}
}