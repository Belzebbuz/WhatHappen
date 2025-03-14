﻿using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace WhatHappen.Generators.Decorators;

public static class DecoratorCodeGenerator
{
	private static readonly SymbolDisplayFormat SignatureFormat = new (
		genericsOptions: SymbolDisplayGenericsOptions.IncludeTypeParameters
		                 | SymbolDisplayGenericsOptions.IncludeTypeConstraints,
		memberOptions: SymbolDisplayMemberOptions.IncludeParameters
		               | SymbolDisplayMemberOptions.IncludeType
		               | SymbolDisplayMemberOptions.IncludeModifiers,
		parameterOptions: SymbolDisplayParameterOptions.IncludeType
		                  | SymbolDisplayParameterOptions.IncludeName
		                  | SymbolDisplayParameterOptions.IncludeDefaultValue
		                  | SymbolDisplayParameterOptions.IncludeParamsRefOut,
		miscellaneousOptions: SymbolDisplayMiscellaneousOptions.UseSpecialTypes
		                      | SymbolDisplayMiscellaneousOptions.IncludeNullableReferenceTypeModifier);

	public static void Generate(IncrementalGeneratorInitializationContext context)
	{
		var provider = context.SyntaxProvider
			.CreateSyntaxProvider(
				(s, _) => s is InterfaceDeclarationSyntax,
				(ctx, _) => GetInterfaceDeclarationForSourceGen(ctx))
			.Where(t => t.genrateAttributeFound)
			.Select((t, _) => t.syntax);

		context.RegisterSourceOutput(context.CompilationProvider.Combine(provider.Collect()),
			((ctx, t) => GenerateCode(ctx, t.Left, t.Right)));
	}

	private static void GenerateCode(SourceProductionContext ctx, Compilation compilation,
		ImmutableArray<InterfaceDeclarationSyntax> interfaceDeclarations)
	{
		foreach (var interfaceDeclarationSyntax in interfaceDeclarations)
		{
			var semanticModel = compilation.GetSemanticModel(interfaceDeclarationSyntax.SyntaxTree);
			// Symbols помогают получить информация времени компиляции
			if (semanticModel.GetDeclaredSymbol(interfaceDeclarationSyntax) is not INamedTypeSymbol interfaceSymbol)
				continue;
			var implementations = FindConcreteInterfaces(compilation, interfaceSymbol)
				.ToArray();
			if(interfaceSymbol.IsGenericType)
				for (var i = 0; i < implementations.Length; i++)
				{
					GenerateSingleImplementation(ctx, implementations[i], interfaceDeclarationSyntax,(i + 1).ToString());
				}

			else
			{
				GenerateSingleImplementation(ctx, interfaceSymbol, interfaceDeclarationSyntax, string.Empty);
			}
		}
	}

	private static HashSet<INamedTypeSymbol> FindConcreteInterfaces(Compilation compilation, INamedTypeSymbol interfaceSymbol)
	{
		var implementations = new HashSet<INamedTypeSymbol>(SymbolEqualityComparer.Default);
		if (!interfaceSymbol.IsGenericType) return implementations;
		foreach (var syntaxTree in compilation.SyntaxTrees)
		{
			var fullSemanticModel = compilation.GetSemanticModel(syntaxTree);
			var classes = syntaxTree.GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>();

			foreach (var classSyntax in classes)
			{
				var classSymbol = fullSemanticModel.GetDeclaredSymbol(classSyntax) as INamedTypeSymbol;
				if (classSymbol == null || classSymbol.IsAbstract) continue;

				// Проверяем, реализует ли класс целевую версию интерфейса
				var concreteInterface = classSymbol.AllInterfaces.FirstOrDefault(i =>
					i.OriginalDefinition.Equals(interfaceSymbol, SymbolEqualityComparer.Default));
				if (concreteInterface is not null)
				{
					implementations.Add(concreteInterface);
				}
			}
		}

		return implementations;
	}

	private static void GenerateSingleImplementation(SourceProductionContext ctx, INamedTypeSymbol interfaceSymbol,
		InterfaceDeclarationSyntax interfaceDeclarationSyntax, string classNameSuffix)
	{
		var namespaceName = interfaceSymbol.ContainingNamespace.ToDisplayString();

		var interfaceShortName = interfaceDeclarationSyntax.Identifier.Text;
		var interfaceName = interfaceSymbol.ToDisplayString(SignatureFormat);
		var declaredMethods = interfaceSymbol.GetMembers().OfType<IMethodSymbol>().ToList();
		if (declaredMethods.Count == 0)
			return;
		var decoratedClassName = $"{interfaceShortName}_TraceDecorator{classNameSuffix}";
		var decoratedClassBody = $@"// <auto-generated/>
using WhatHappen.Core.Tracing;
#nullable enable
namespace {namespaceName};
public sealed class {decoratedClassName} : {interfaceName}
{{
	private readonly {interfaceName} _inner;
	
	public {decoratedClassName}({interfaceName} inner)
	{{
		_inner = inner;
	}}
	
	{GenerateMethods(declaredMethods)}
}}";
		ctx.AddSource($"{decoratedClassName}.g.cs", SourceText.From(decoratedClassBody, Encoding.UTF8));
	}


	private static string GenerateMethods(IEnumerable<IMethodSymbol> declaredMethods)
	{
		var methodDeclarations = new List<string>();
		foreach (var methodSymbol in declaredMethods)
		{
			var methodBody = GenerateMethodBody(methodSymbol);
			methodDeclarations.Add(methodBody);
		}
		return string.Join("\n\n\t", methodDeclarations);
	}

	private static string GenerateMethodBody(IMethodSymbol methodSymbol)
	{
		var sb = new StringBuilder();
		var signature = methodSymbol.ToDisplayString(SignatureFormat);
		var needToAwait = methodSymbol.ReturnType.Name == "Task";
		var asyncSymbols = needToAwait ? "async" : string.Empty;
		sb.Append($"public {asyncSymbols} ");
		sb.Append(signature);
		var isEnumerable = methodSymbol.ReturnType.Name.StartsWith("IAsyncEnumerable")
		                   || methodSymbol.ReturnType.Name.StartsWith("IEnumerable");
		var hasResult = methodSymbol.ReturnType.SpecialType != SpecialType.System_Void &&
		                methodSymbol.ReturnType.ToDisplayString() != "System.Threading.Tasks.Task" && 
		                !isEnumerable;
		var methodBody = $@"
	{{
		var originalClass = _inner.GetType().Name;
		var inputParameters = {GenerateParametersObject(methodSymbol)};
		var step = new TraceMethodInvocationStep()
		{{
			Class = originalClass,
			Method = ""{methodSymbol.Name}"",
			Input = inputParameters,
			IsCompleted = false
		}};
		TracingContext.AddStep(step);
		object? result = null;
		try 
		{{
			{(isEnumerable ? "return":string.Empty)}{(hasResult ? "var originalResult =" : string.Empty)}{(needToAwait ? " await" : string.Empty)} _inner.{GetMethodCallName(methodSymbol)}({GetParameterNames(methodSymbol)});
			{(hasResult ? "result = originalResult;" : string.Empty)}
			{(hasResult ? "return originalResult;" : string.Empty)}
		}}
		finally
		{{
			TracingContext.CompleteStep(result, step.StepId);
		}}
	}}";
		sb.Append(methodBody);
		return sb.ToString();
	}

	private static string GetMethodCallName(IMethodSymbol methodSymbol)
	{
		if (!methodSymbol.IsGenericMethod)
			return methodSymbol.Name;
		var typeParameters = methodSymbol.TypeParameters.Select(t => t.Name);
		return $"{methodSymbol.Name}<{string.Join(", ", typeParameters)}>";
	}

	private static string GenerateParametersObject(IMethodSymbol methodSymbol)
	{
		var parameters = methodSymbol.Parameters
			.Where(x => x.RefKind != RefKind.Out)
			.Select(p => $"{p.Name} = {p.Name},");
		return $@"new 
		{{
			{string.Join("\n\t\t\t", parameters)}
		}}";
	}

	private static string GetParameterNames(IMethodSymbol methodSymbol)
	{
		return string.Join(", ", methodSymbol.Parameters.Select(p =>
		{
			return p.RefKind switch
			{
				RefKind.Ref => $"ref {p.Name}",
				RefKind.RefReadOnly => $"in {p.Name}",
				RefKind.Out => $"out {p.Name}",
				_ => p.Name
			};
		}));
	}

	/// <summary>
	/// Находим все интерфейсы, которые нужно задекорировать
	/// <br/>
	/// Они помечены атрибутом [GenerateTrack]
	/// </summary>
	/// <param name="context"></param>
	/// <returns></returns>
	private static (InterfaceDeclarationSyntax syntax, bool genrateAttributeFound) GetInterfaceDeclarationForSourceGen(
		GeneratorSyntaxContext context)
	{
		var interfaceDeclarationSyntax = (InterfaceDeclarationSyntax)context.Node;

		// Проходимся по всем атрибутам интерфейса
		var attributes = interfaceDeclarationSyntax.AttributeLists.SelectMany(x => x.Attributes);
		foreach (var attributeSyntax in attributes)
		{
			if (context.SemanticModel.GetSymbolInfo(attributeSyntax).Symbol is not IMethodSymbol attributeSymbol)
				continue; // if we can't get the symbol, ignore it

			var attributeName = attributeSymbol.ContainingType.ToDisplayString();

			// Проверяем что полное имя атрибута совпадает с искомым
			if (MarkerAttributeGenerator.IsMarkerAttribute(attributeName))
				return (interfaceDeclarationSyntax, true);
		}
		return (interfaceDeclarationSyntax, false);
	}
}