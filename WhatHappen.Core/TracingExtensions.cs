using System.Linq;
using System.Reflection;
using Castle.DynamicProxy;
using Grpc.AspNetCore.Server;
using Grpc.Net.ClientFactory;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using WhatHappen.Core.GrpcServices;
using WhatHappen.Core.Interceptors;
using WhatHappen.Core.Patching;
using WhatHappen.Core.Tracing;
using InterceptorRegistration = Grpc.Net.ClientFactory.InterceptorRegistration;

namespace WhatHappen.Core;

public static class TracingExtensions
{
	public static IServiceCollection AddWhatHappen(this IServiceCollection services, Assembly assembly, IConfiguration configuration)
	{
		// services.Configure<WhatHappenOptions>(configuration.GetSection(nameof(WhatHappenOptions)));
		services.AddTransient<CastleMethodInterceptor>();
		DecorateTrackingServices(services, assembly); // Если используем Castle декораторы то TrackAttribute должен быть развешан на интерфейсы
		//HarmonyWhatHappenPatcher.Patch(assembly); //Если используем патчинг, то нужно перенести TrackAttribute с интерфейсов на реализации
		services.PostConfigure<GrpcServiceOptions>(options => options.Interceptors.Add<GrpcTraceInitInterceptor>());
		return services;
	}

	private static void DecorateTrackingServices(IServiceCollection services, Assembly assembly)
	{
		var trackedTypes = assembly.DefinedTypes
			.Where(x => x.GetCustomAttribute<TrackAttribute>() != null);
		var generator = new ProxyGenerator();
		foreach (var type in trackedTypes)
		{
			var registeredServices = services.Where(x => x.ServiceType == type).ToArray();
			foreach (var registeredService in registeredServices)
			{
				if (registeredService.IsKeyedService)
				{
					var decoratedKeyedServiceDescriptor = new ServiceDescriptor(
						registeredService.ServiceType,
						registeredService.ServiceKey,
						(provider, _) =>
						{
							var originalService = ActivatorUtilities.CreateInstance(provider, registeredService.KeyedImplementationType!);
							var interceptor = provider.GetRequiredService<CastleMethodInterceptor>();
							var instance = generator.CreateInterfaceProxyWithTarget(type, originalService, interceptor);
							return instance;
						},
						registeredService.Lifetime);
					services.Add(decoratedKeyedServiceDescriptor);
					continue;
				}

				
				var decoratedServiceDescriptor = new ServiceDescriptor(registeredService.ServiceType, (provider) =>
				{
					var originalService =
						ActivatorUtilities.CreateInstance(provider, registeredService.ImplementationType!);
					var interceptor = provider.GetRequiredService<CastleMethodInterceptor>();
					var instance = generator.CreateInterfaceProxyWithTarget(type, originalService, interceptor);
					return instance;
				}, registeredService.Lifetime);
				services.Add(decoratedServiceDescriptor);
			}
		}
	}

	public static GrpcClientFactoryOptions AddWhatHappenTracing(this GrpcClientFactoryOptions options)
	{
		options.InterceptorRegistrations.Add(new InterceptorRegistration(InterceptorScope.Client,_ => new GrpcClientCallInterceptor()));
		return options;
	}

	public static DbContextOptionsBuilder AddWhatHappenTracing(this DbContextOptionsBuilder builder)
	{
		builder.AddInterceptors([DatabaseInterceptor.Interceptor]);
		return builder;
	}

	public static void MapGrpcWhatHappen(this WebApplication app)
	{
		app.MapGrpcService<WhatHappenGrpcService>();
	}
}