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
			Class = invocation.TargetType.FullName ?? "NO NAME",
			Method = invocation.Method.Name,
			Input = invocation.Arguments,
			IsCompleted = false
		};
		TracingContext.AddStep(step);
		var operationId = TracingContext.GetCurrentTrace()?.OperationId;
		
		invocation.Proceed();
		
		if (invocation.ReturnValue is Task task)
			task.ContinueWith(x =>
			{
				if(operationId is null)
					return;
				
				var taskType = task.GetType();
				if (!taskType.IsGenericType) 
					return;
				
				var resultProperty = taskType.GetProperty("Result");
				var value = resultProperty?.GetValue(task);
				if(value is null )
					return;
				TracingContext.CompleteStep(value, step.StepId);
			});
		else
		{
			step.Output = invocation.ReturnValue;
			step.IsCompleted = true;
		}
	}
}