﻿// Copyright 2017 Xamarin Inc.

using System;
using System.Collections.Generic;

using Mono.Cecil;
using Mono.Linker;
using Mono.Tuner;

using Xamarin.Bundler;

#if NET
using Mono.Linker.Steps;
#else
using MonoTouch.Tuner;
#endif

namespace Xamarin.Linker.Steps
{
	public class PreserveSmartEnumConversionsSubStep : ExceptionalSubStep
	{
		Dictionary<TypeDefinition, Tuple<MethodDefinition, MethodDefinition>> cache;
		protected override string Name { get; } = "Smart Enum Conversion Preserver";
		protected override int ErrorCode { get; } = 2200;

		public override SubStepTargets Targets {
			get {
				return SubStepTargets.Method | SubStepTargets.Property;
			}
		}

#if NET
		LinkContext context {
			get { return Context; }
		}
#endif

		public override bool IsActiveFor (AssemblyDefinition assembly)
		{
			if (Profile.IsProductAssembly (assembly))
				return true;

			// We don't need to process assemblies that don't reference ObjCRuntime.BindAsAttribute.
			foreach (var tr in assembly.MainModule.GetTypeReferences ()) {
				if (tr.IsPlatformType ("ObjCRuntime", "BindAsAttribute"))
					return true;
			}

			return false;
		}

		void Preserve (Tuple<MethodDefinition, MethodDefinition> pair, MethodDefinition conditionA, MethodDefinition conditionB = null)
		{
			if (conditionA != null) {
				context.Annotations.AddPreservedMethod (conditionA.DeclaringType, pair.Item1);
				context.Annotations.AddPreservedMethod (conditionA.DeclaringType, pair.Item2);
			}
			if (conditionB != null) {
				context.Annotations.AddPreservedMethod (conditionB.DeclaringType, pair.Item1);
				context.Annotations.AddPreservedMethod (conditionB.DeclaringType, pair.Item2);
			}
		}

		void ProcessAttributeProvider (ICustomAttributeProvider provider, MethodDefinition conditionA, MethodDefinition conditionB = null)
		{
			if (provider?.HasCustomAttributes != true)
				return;

			foreach (var ca in provider.CustomAttributes) {
				var tr = ca.Constructor.DeclaringType;

				if (!tr.IsPlatformType ("ObjCRuntime", "BindAsAttribute"))
					continue;

				if (ca.ConstructorArguments.Count != 1) {
#if NET
					var s = "warning MT4124: " + String.Format (Errors.MT4124_E, provider.AsString (), ca.ConstructorArguments.Count);
					Context.LogMessage (MessageContainer.CreateInfoMessage (s));
#else
					ErrorHelper.Show (ErrorHelper.CreateWarning (LinkContext.Target.App, 4124, provider, Errors.MT4124_E, provider.AsString (), ca.ConstructorArguments.Count));
#endif
					continue;
				}

				var managedType = ca.ConstructorArguments [0].Value as TypeReference;
				var managedEnumType = managedType?.GetElementType ().Resolve ();
				if (managedEnumType == null) {
#if NET
					var s = "warning MT4124: " + String.Format (Errors.MT4124_H, provider.AsString (), managedType?.FullName);
					Context.LogMessage (MessageContainer.CreateInfoMessage (s));
#else
					ErrorHelper.Show (ErrorHelper.CreateWarning (LinkContext.Target.App, 4124, provider, Errors.MT4124_H, provider.AsString (), managedType?.FullName));
#endif
					continue;
				}

				// We only care about enums, BindAs attributes can be used for other types too.
				if (!managedEnumType.IsEnum)
					continue;

				Tuple<MethodDefinition, MethodDefinition> pair;
				if (cache != null && cache.TryGetValue (managedEnumType, out pair)) {
					Preserve (pair, conditionA, conditionB);
					continue;
				}

				// Find the Extension type
				TypeDefinition extensionType = null;
				var extensionName = managedEnumType.Name + "Extensions";
				foreach (var type in managedEnumType.Module.Types) {
					if (type.Namespace != managedEnumType.Namespace)
						continue;
					if (type.Name != extensionName)
						continue;
					extensionType = type;
					break;
				}
				if (extensionType == null) {
					Log (1, $"Could not find a smart extension type for the enum {managedEnumType.FullName} (due to BindAs attribute on {provider.AsString ()}): most likely this is because the enum isn't a smart enum.");
					continue;
				}

				// Find the GetConstant/GetValue methods
				MethodDefinition getConstant = null;
				MethodDefinition getValue = null;

				foreach (var method in extensionType.Methods) {
					if (!method.IsStatic)
						continue;
					if (!method.HasParameters || method.Parameters.Count != 1)
						continue;
					if (method.Name == "GetConstant") {
						if (!method.ReturnType.IsPlatformType ("Foundation", "NSString"))
							continue;
						if (method.Parameters [0].ParameterType != managedEnumType)
							continue;
						getConstant = method;
					} else if (method.Name == "GetValue") {
						if (!method.Parameters [0].ParameterType.IsPlatformType ("Foundation", "NSString"))
							continue;
						if (method.ReturnType != managedEnumType)
							continue;
						getValue = method;
					}
				}

				if (getConstant == null) {
					Log (1, $"Could not find the GetConstant method on the supposedly smart extension type {extensionType.FullName} for the enum {managedEnumType.FullName} (due to BindAs attribute on {provider.AsString ()}): most likely this is because the enum isn't a smart enum.");
					continue;
				}

				if (getValue == null) {
					Log (1, $"Could not find the GetValue method on the supposedly smart extension type {extensionType.FullName} for the enum {managedEnumType.FullName} (due to BindAs attribute on {provider.AsString ()}): most likely this is because the enum isn't a smart enum.");
					continue;
				}

				pair = new Tuple<MethodDefinition, MethodDefinition> (getConstant, getValue);
				if (cache == null)
					cache = new Dictionary<TypeDefinition, Tuple<MethodDefinition, MethodDefinition>> ();
				cache.Add (managedEnumType, pair);
				Preserve (pair, conditionA, conditionB);
			}
		}

		void Log (int logLevel, string message)
		{
#if NET
			Context.LogMessage (MessageContainer.CreateInfoMessage (message));
#else
			Driver.Log (logLevel, message);
#endif
		}

		protected override void Process (MethodDefinition method)
		{
			ProcessAttributeProvider (method, method);
			ProcessAttributeProvider (method.MethodReturnType, method);
			if (method.HasParameters) {
				foreach (var p in method.Parameters)
					ProcessAttributeProvider (p, method);
			}
		}

		protected override void Process (PropertyDefinition property)
		{
			ProcessAttributeProvider (property, property.GetMethod, property.SetMethod);
			if (property.GetMethod != null)
				Process (property.GetMethod);
			if (property.SetMethod != null)
				Process (property.SetMethod);
		}
	}
}
