using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using WhatHappen.Core.Interceptors;
using WhatHappen.Core.Tracing;

namespace WhatHappen.Core.Patching;

internal static class HarmonyWhatHappenPatcher
{
	public static void Patch(Assembly assembly)
	{
		var methods = GetTrackingMethods(assembly);
		var harmony = new Harmony(nameof(HarmonyWhatHappenPatcher));
		foreach (var methodInfo in methods)
		{
			harmony.Patch(methodInfo, new HarmonyMethod(HarmonyMethodInterceptor.Prefix), new HarmonyMethod(HarmonyMethodInterceptor.Postfix));
		}
	}

	private static IEnumerable<MethodInfo> GetTrackingMethods(Assembly assembly)
	{
		var methods = assembly.GetTypes()
			.Where(t => t.GetCustomAttribute<TrackAttribute>() != null)
			.SelectMany(t => t.GetMethods(
				BindingFlags.Instance | 
				BindingFlags.Static | 
				BindingFlags.Public | 
				BindingFlags.NonPublic | 
				BindingFlags.DeclaredOnly));
		return methods;
	}

	public static void Unpatch()
	{
		var harmony = new Harmony(nameof(HarmonyWhatHappenPatcher));
		harmony.UnpatchAll();
	}

}