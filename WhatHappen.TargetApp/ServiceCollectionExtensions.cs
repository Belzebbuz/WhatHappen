using System.Reflection;

namespace WhatHappen.TargetApp;

public static class ServiceCollectionExtensions
{
	public static void DecorateTrackingServices(this IServiceCollection services, Assembly assembly)
	{
		var decorators = assembly.DefinedTypes
			.Where(x => x.Name.Contains("_TraceDecorator") && x.IsClass)
			.ToArray();
		foreach (var type in decorators)
		{
			var decoratorType = type.GetInterfaces().FirstOrDefault() ??
			                    throw new ArgumentException("Для интерфейса не сгенерирован декоратор");
			var registeredServices = services.Where(x => x.ServiceType == decoratorType).ToArray();
			foreach (var registeredService in registeredServices)
			{

			}
		}
	}
}