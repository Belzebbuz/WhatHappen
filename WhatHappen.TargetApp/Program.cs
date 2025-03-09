using Microsoft.EntityFrameworkCore;
using WhatHappen.Core;
using WhatHappen.OtherService;
using WhatHappen.TargetApp.Context;
using WhatHappen.TargetApp.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddGrpcReflection();
builder.Services.AddScoped<IValidator, Validator>();
builder.Services.AddScoped<IValidatorCalculator, ValidatorCalculator>();
builder.Services.AddDbContext<AppDbContext>(opt =>
{
	opt.UseNpgsql(builder.Configuration.GetConnectionString("postgres"));
	opt.AddWhatHappenTracing();
});
builder.Services.AddGrpc();
builder.Services.AddGrpcClient<OtherGreeter.OtherGreeterClient>(opt =>
{
	opt.Address = new Uri(builder.Configuration.GetConnectionString("OtherService") ?? throw new ArgumentException());
	opt.AddWhatHappenTracing();
});
builder.Services.AddKeyedSingleton<IKeyedValidation, KeyedValidation>("a-value");
builder.Services.AddWhatHappen(typeof(Program).Assembly, builder.Configuration);

var app = builder.Build();
await AppDbContext.MigrateAsync(app);

app.MapGrpcService<GreeterService>();
app.MapGrpcWhatHappen();
app.MapGrpcReflectionService();
app.Run();