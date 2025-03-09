using System.Collections.Generic;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Diagnostics;
using WhatHappen.Core.Tracing;

namespace WhatHappen.Core.Interceptors;

public class DatabaseInterceptor : DbCommandInterceptor
{
	public static readonly DatabaseInterceptor Interceptor = new ();
	private readonly record struct StepDbPapams(string Key, object Value);

	public override async ValueTask<DbDataReader> ReaderExecutedAsync(DbCommand command, CommandExecutedEventData eventData, DbDataReader result,
		CancellationToken cancellationToken = new CancellationToken())
	{
		var readResult = await base.ReaderExecutedAsync(command, eventData, result, cancellationToken);
		if(TracingContext.GetCurrentTrace() is null)
			return readResult;
		CreateStep(command);
		return readResult;
	}

	public override DbDataReader ReaderExecuted(DbCommand command, CommandExecutedEventData eventData, DbDataReader result)
	{
		var readResult = base.ReaderExecuted(command, eventData, result);
		if(TracingContext.GetCurrentTrace() is null)
			return readResult;
		CreateStep(command);
		return readResult;
	}

	private static void CreateStep(DbCommand command)
	{
		var parameters = new List<StepDbPapams>();
		foreach (DbParameter dbParameter in command.Parameters)
		{
			parameters.Add(new StepDbPapams(dbParameter.ParameterName,dbParameter.Value ?? "NULL"));
		}
		var dbTraceStep = new TraceDbStep()
		{
			Parameters = parameters,
			Sql = command.CommandText,
			IsCompleted = true
		};
		TracingContext.AddStep(dbTraceStep);
	}
}