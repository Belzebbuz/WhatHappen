using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using WhatHappen.Core.Tracing;

namespace WhatHappen.Core.Interceptors;

internal static class HarmonyMethodInterceptor
{
	public static void Prefix(
		MethodBase __originalMethod,
		object __instance, 
		object[] __args,
		ref Dictionary<string, object> __state)
	{
		var step = new TraceMethodInvocationStep
		{
			Class = __originalMethod.DeclaringType?.FullName ?? "No name",
			Method = __originalMethod.Name,
			Input = __args,
			IsCompleted = false,
		};
		__state = new Dictionary<string, object> { ["step"] = step };
		TracingContext.AddStep(step);
	}

	public static void Postfix(
		MethodBase __originalMethod, 
		object __instance, 
		object __result, 
		ref Dictionary<string, object> __state)
	{

		if (__state?.TryGetValue("step", out var stepObj) != true ||
		    stepObj is not TraceMethodInvocationStep step) return;
		
		if (__result is Task task)
		{
			if (task.IsCompleted)
			{
				var result = task.GetType().GetProperty("Result")?.GetValue(task);
				TracingContext.CompleteStep(result, step.StepId);
				return;
			}
			task.ContinueWith(t => 
			{
				var result = t.GetType().GetProperty("Result")?.GetValue(t);
				TracingContext.CompleteStep(result, step.StepId);
			});
		}
		else
		{
			TracingContext.CompleteStep(__result, step.StepId);
		}
	}
}