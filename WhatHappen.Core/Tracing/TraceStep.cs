using System.Collections.Generic;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace WhatHappen.Core.Tracing;

public abstract class TraceStep
{
	protected JsonSerializerOptions Options =  new () 
	{ 
		ReferenceHandler = ReferenceHandler.IgnoreCycles,
		Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
		WriteIndented = true 
	};
	public string? StepId { get; set; }
	public abstract string Type { get; }
	public required bool IsCompleted { get; set; }
	[JsonIgnore] public virtual bool IsExternal { get; } = false;
	[JsonIgnore] public string? ParentStepId { get; set; }
	[JsonIgnore] public List<TraceStep> Children { get; } = new();
	public abstract string ToJson();
}