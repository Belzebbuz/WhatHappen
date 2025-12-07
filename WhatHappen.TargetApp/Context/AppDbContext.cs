using Microsoft.EntityFrameworkCore;

namespace WhatHappen.TargetApp.Context;

public class AppDbContext : DbContext
{
	public AppDbContext(DbContextOptions<AppDbContext> options)
	:base(options)
	{
	}

	public DbSet<Data> Datas => Set<Data>();

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		modelBuilder.Entity<Data>()
			.Property(d => d.InfoValues)
			.HasColumnType("jsonb");
		base.OnModelCreating(modelBuilder);
	}
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
	public List<InfoValue>? InfoValues { get; set; }
}

public class InfoValue
{
	public string Key { get; set; }
	public string Value { get; set; }
}