using Microsoft.EntityFrameworkCore;

namespace WhatHappen.TargetApp.Context;

public class AppDbContext : DbContext
{
	public AppDbContext(DbContextOptions<AppDbContext> options)
	:base(options)
	{
	}

	public DbSet<Data> Datas => Set<Data>();

	public static async Task MigrateAsync(WebApplication app)
	{
		await using var scope = app.Services.CreateAsyncScope();
		await using var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
		await context.Database.MigrateAsync();
	}
}

public class Data
{
	public Guid Id { get; set; }
	public string Value { get; set; }
}