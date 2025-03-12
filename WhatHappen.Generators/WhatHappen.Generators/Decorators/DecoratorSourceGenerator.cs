using Microsoft.CodeAnalysis;

namespace WhatHappen.Generators.Decorators;

[Generator]
public class DecoratorSourceGenerator : IIncrementalGenerator
{
	public void Initialize(IncrementalGeneratorInitializationContext context)
	{
		MarkerAttributeGenerator.Generate(context);
		DecoratorCodeGenerator.Generate(context);
		ServiceExtensionsGenerator.Generate(context);
	}
}