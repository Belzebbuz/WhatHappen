using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace WhatHappen.Generators.Decorators;

public static class ServiceExtensionsGenerator
{
	public static void Generate(IncrementalGeneratorInitializationContext context)
	{
		var serviceExtensionsClassBody = $@"
using System;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace DecoratorsGenerators;

public static class TracingDecoratorsServiceCollectionExtensions
{{
	public static void DecorateTrackingServices(this IServiceCollection services, Assembly assembly)
	{{
		var decorators = assembly.DefinedTypes
			.Where(x => x.Name.Contains(""_TraceDecorator"") && x.IsClass)
			.ToArray(); 
		foreach (var type in decorators)
		{{
			var decoratorType = type.GetInterfaces().FirstOrDefault() ?? throw new ArgumentException(""Для интерфейса не сгенерирован декоратор"");
			var registeredServices = services.Where(x => x.ServiceType == decoratorType).ToArray();
			foreach (var registeredService in registeredServices)
			{{
				if (registeredService.IsKeyedService)
				{{
					var decoratedKeyedServiceDescriptor = new ServiceDescriptor(
						registeredService.ServiceType,
						registeredService.ServiceKey,
						(provider, _) =>
						{{
							var originalService = ActivatorUtilities.CreateInstance(provider, registeredService.KeyedImplementationType!);
							var decoratorInstance = Activator.CreateInstance(type, originalService);
							return decoratorInstance;
						}},
						registeredService.Lifetime);
					services.Add(decoratedKeyedServiceDescriptor);
					continue;
				}}

				
				var decoratedServiceDescriptor = new ServiceDescriptor(registeredService.ServiceType, (provider) =>
				{{
					var originalService =
						ActivatorUtilities.CreateInstance(provider, registeredService.ImplementationType!);
					var decoratorInstance = Activator.CreateInstance(type, originalService);
					return decoratorInstance;
				}}, registeredService.Lifetime);
				services.Add(decoratedServiceDescriptor);
			}}
		}}
	}}
}}
";
		context.RegisterPostInitializationOutput(ctx => ctx.AddSource(
			"TracingDecoratorsServiceCollectionExtensions.g.cs",
			SourceText.From(serviceExtensionsClassBody, Encoding.UTF8)));
	}
}