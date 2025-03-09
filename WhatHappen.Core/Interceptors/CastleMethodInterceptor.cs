using System.Threading.Tasks;
using Castle.DynamicProxy;
using WhatHappen.Core.Tracing;

namespace WhatHappen.Core.Interceptors;

public class CastleMethodInterceptor : IInterceptor
{
	public void Intercept(IInvocation invocation)
	{
		var step = new TraceMethodInvocationStep()
		{
			Class = invocation.TargetType.FullName ?? "NO NAME METHOD",
			Method = invocation.Method.Name,
			Input = invocation.Arguments,
			IsCompleted = false
		};
		TracingContext.AddStep(step);
		
		invocation.Proceed();

		if (invocation.ReturnValue is Task task)
		{
			if(task.IsCompleted)
			{
				var result = task.GetType().GetProperty("Result")?.GetValue(task);
				TracingContext.CompleteStep(result, step.StepId);
			}
			else
			{
				task.ContinueWith(t =>
				{
					var result = t.GetType().GetProperty("Result")?.GetValue(t);
					TracingContext.CompleteStep(result, step.StepId);
				});
			}
			return;
		}
		TracingContext.CompleteStep(invocation.ReturnValue, step.StepId);
	}
}