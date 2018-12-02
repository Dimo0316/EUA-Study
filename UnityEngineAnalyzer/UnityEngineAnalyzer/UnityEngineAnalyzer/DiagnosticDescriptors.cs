using Microsoft.CodeAnalysis;
using UnityEngineAnalyzer.AOT;
using UnityEngineAnalyzer.Camera;
using UnityEngineAnalyzer.Closure;
using UnityEngineAnalyzer.CompareTag;
using UnityEngineAnalyzer.Containers;
using UnityEngineAnalyzer.Coroutines;
using UnityEngineAnalyzer.MonoBehaviourMethods;
using UnityEngineAnalyzer.FindMethodsInUpdate;
using UnityEngineAnalyzer.ForbiddenMethods;
using UnityEngineAnalyzer.ForEachInUpdate;
using UnityEngineAnalyzer.IL2CPP;
using UnityEngineAnalyzer.Material;
using UnityEngineAnalyzer.Memory;
using UnityEngineAnalyzer.OnGUI;
using UnityEngineAnalyzer.Particles;
using UnityEngineAnalyzer.Physics;
using UnityEngineAnalyzer.Preprocessor;
using UnityEngineAnalyzer.Regex;
using UnityEngineAnalyzer.ReplaceOldGCMethods;
using UnityEngineAnalyzer.String;
using UnityEngineAnalyzer.StringMethods;
using UnityEngineAnalyzer.Textures;
using UnityEngineAnalyzer.Transform;
using UnityEngineAnalyzer.UnityDebug;
using UnityEngineAnalyzer.UnityThread;
using UnityEngineAnalyzer.ValueType;
using UnityEngineAnalyzer.Variable;
using UnityEngineAnalyzer.Vectors;


namespace UnityEngineAnalyzer
{
    static class DiagnosticDescriptors
    {
        //NOTES: Naming of Descriptors are a bit inconsistant
        //NOTES: The Resource Reading code seems  repetative


        /// Common DiagnosticDescriptor
        public static readonly DiagnosticDescriptor Common = new DiagnosticDescriptor(
            // an unique identifier for the diagnostic
            id: DiagnosticIDs.Common,
            // a short localizable title describing the diagnostic
            title: "Common Diagnostic",
            // a localizable format message string, which can be passed as the first argument to System.String.Format(System.String, System.Object[]) when creating the diagnostic message with this descriptor
            messageFormat: "{0}",
            // the category of the diagnostic (like Design, Naming etc.)
            category: DiagnosticCategories.Miscellaneous,
            // the default severity of the diagnostic
            defaultSeverity: DiagnosticSeverity.Info,
            // the diagnostic is enabled by default
            isEnabledByDefault: true,
            // an optional longer localizable description for the diagnostic
            description: "This is a common Diagnostic");


        /// Common DiagnosticDescriptor
        public static readonly DiagnosticDescriptor Test = new DiagnosticDescriptor(
            // an unique identifier for the diagnostic
            id: "UEATEST",
            // a short localizable title describing the diagnostic
            title: "UEA Test",
            // a localizable format message string, which can be passed as the first argument to System.String.Format(System.String, System.Object[]) when creating the diagnostic message with this descriptor
            messageFormat: "UnityEngine Analyzer is running.\n{0}",
            // the category of the diagnostic (like Design, Naming etc.)
            category: DiagnosticCategories.Miscellaneous,
            // the default severity of the diagnostic
            defaultSeverity: DiagnosticSeverity.Warning,
            // the diagnostic is enabled by default
            isEnabledByDefault: true,
            // an optional longer localizable description for the diagnostic
            description: "This is a UEA Test");


        /// A DiagnosticDescriptor describing the "OnGUI" diagnostic.
        public static readonly DiagnosticDescriptor DoNotUseOnGUI = new DiagnosticDescriptor(
            // an unique identifier for the diagnostic
            id: DiagnosticIDs.DoNotUseOnGUI,
            // a short localizable title describing the diagnostic
            title: new LocalizableResourceString(nameof(DoNotUseOnGUIResources.Title),
                DoNotUseOnGUIResources.ResourceManager,
                typeof(DoNotUseOnGUIResources)),
            // a localizable format message string, which can be passed as the first argument to System.String.Format(System.String, System.Object[]) when creating the diagnostic message with this descriptor
            messageFormat: new LocalizableResourceString(nameof(DoNotUseOnGUIResources.MessageFormat),
                DoNotUseOnGUIResources.ResourceManager, typeof(DoNotUseOnGUIResources)),
            // the category of the diagnostic (like Design, Naming etc.)
            category: DiagnosticCategories.GC,
            // the default severity of the diagnostic
            defaultSeverity: DiagnosticSeverity.Warning,
            // the diagnostic is enabled by default
            isEnabledByDefault: true,
            // an optional longer localizable description for the diagnostic
            description: new LocalizableResourceString(nameof(DoNotUseOnGUIResources.Description),
                DoNotUseOnGUIResources.ResourceManager, typeof(DoNotUseOnGUIResources)));


        /// A DiagnosticDescriptor describing the "SendMessage, SendMessageUpwards, BroadcastMessage" diagnostic.
        public static readonly DiagnosticDescriptor DoNotUseStringMethods = new DiagnosticDescriptor(
            // an unique identifier for the diagnostic
            id: DiagnosticIDs.DoNotUseStringMethods,
            // a short localizable title describing the diagnostic
            title: new LocalizableResourceString(nameof(DoNotUseStringMethodsResources.Title),
                DoNotUseStringMethodsResources.ResourceManager, typeof(DoNotUseStringMethodsResources)),
            // a localizable format message string, which can be passed as the first argument to System.String.Format(System.String, System.Object[]) when creating the diagnostic message with this descriptor
            messageFormat: new LocalizableResourceString(nameof(DoNotUseStringMethodsResources.MessageFormat),
                DoNotUseStringMethodsResources.ResourceManager, typeof(DoNotUseStringMethodsResources)),
            // the category of the diagnostic (like Design, Naming etc.)
            category: DiagnosticCategories.StringMethods,
            // the default severity of the diagnostic
            defaultSeverity: DiagnosticSeverity.Warning,
            // the diagnostic is enabled by default
            isEnabledByDefault: true,
            // an optional longer localizable description for the diagnostic
            description: new LocalizableResourceString(nameof(DoNotUseStringMethodsResources.Description),
                DoNotUseStringMethodsResources.ResourceManager, typeof(DoNotUseStringMethodsResources)));


        /// A DiagnosticDescriptor describing the "SendMessage, SendMessageUpwards, BroadcastMessage" diagnostic.
        public static readonly DiagnosticDescriptor DoNotUseLinq = new DiagnosticDescriptor(
            // an unique identifier for the diagnostic
            id: DiagnosticIDs.DoNotUseLinq,
            // a short localizable title describing the diagnostic
            title: new LocalizableResourceString(nameof(DoNotUseLinqResources.Title),
                DoNotUseLinqResources.ResourceManager,
                typeof(DoNotUseLinqResources)),
            // a localizable format message string, which can be passed as the first argument to System.String.Format(System.String, System.Object[]) when creating the diagnostic message with this descriptor
            messageFormat: new LocalizableResourceString(nameof(DoNotUseLinqResources.MessageFormat),
                DoNotUseLinqResources.ResourceManager, typeof(DoNotUseLinqResources)),
            // the category of the diagnostic (like Design, Naming etc.)
            category: DiagnosticCategories.StringMethods,
            // the default severity of the diagnostic
            defaultSeverity: DiagnosticSeverity.Warning,
            // the diagnostic is enabled by default
            isEnabledByDefault: true,
            // an optional longer localizable description for the diagnostic
            description: new LocalizableResourceString(nameof(DoNotUseLinqResources.Description),
                DoNotUseLinqResources.ResourceManager, typeof(DoNotUseLinqResources)));


        /// A DiagnosticDescriptor describing the "Coroutines" diagnostic.
        public static readonly DiagnosticDescriptor DoNotUseCoroutines = new DiagnosticDescriptor(
            // an unique identifier for the diagnostic
            id: DiagnosticIDs.DoNotUseCoroutines,
            // a short localizable title describing the diagnostic
            title: new LocalizableResourceString(nameof(DoNotUseCoroutinesResources.Title),
                DoNotUseCoroutinesResources.ResourceManager, typeof(DoNotUseCoroutinesResources)),
            // a localizable format message string, which can be passed as the first argument to System.String.Format(System.String, System.Object[]) when creating the diagnostic message with this descriptor
            messageFormat: new LocalizableResourceString(nameof(DoNotUseCoroutinesResources.MessageFormat),
                DoNotUseCoroutinesResources.ResourceManager, typeof(DoNotUseCoroutinesResources)),
            // the category of the diagnostic (like Design, Naming etc.)
            category: DiagnosticCategories.GC,
            // the default severity of the diagnostic
            defaultSeverity: DiagnosticSeverity.Warning,
            // the diagnostic is enabled by default
            isEnabledByDefault: true,
            // an optional longer localizable description for the diagnostic
            description: new LocalizableResourceString(nameof(DoNotUseCoroutinesResources.Description),
                DoNotUseCoroutinesResources.ResourceManager, typeof(DoNotUseCoroutinesResources)));


        /// A DiagnosticDescriptor describing the "Empty MonoBehaviour method" diagnostic.
        public static readonly DiagnosticDescriptor EmptyMonoBehaviourMethod = new DiagnosticDescriptor(
            // an unique identifier for the diagnostic
            id: DiagnosticIDs.EmptyMonoBehaviourMethod,
            // a short localizable title describing the diagnostic
            title: new LocalizableResourceString(nameof(EmptyMonoBehaviourMethodsResources.Title),
                EmptyMonoBehaviourMethodsResources.ResourceManager, typeof(EmptyMonoBehaviourMethodsResources)),
            // a localizable format message string, which can be passed as the first argument to System.String.Format(System.String, System.Object[]) when creating the diagnostic message with this descriptor
            messageFormat: new LocalizableResourceString(nameof(EmptyMonoBehaviourMethodsResources.MessageFormat),
                EmptyMonoBehaviourMethodsResources.ResourceManager, typeof(EmptyMonoBehaviourMethodsResources)),
            // the category of the diagnostic (like Design, Naming etc.)
            category: DiagnosticCategories.Miscellaneous,
            // the default severity of the diagnostic
            defaultSeverity: DiagnosticSeverity.Warning,
            // the diagnostic is enabled by default
            isEnabledByDefault: true,
            // an optional longer localizable description for the diagnostic
            description: new LocalizableResourceString(nameof(EmptyMonoBehaviourMethodsResources.Description),
                EmptyMonoBehaviourMethodsResources.ResourceManager, typeof(EmptyMonoBehaviourMethodsResources)));


        /// A DiagnosticDescriptor describing the "tag comparison" diagnostic.
        public static readonly DiagnosticDescriptor UseCompareTag = new DiagnosticDescriptor(
            // an unique identifier for the diagnostic
            id: DiagnosticIDs.UseCompareTag,
            // a short localizable title describing the diagnostic
            title: new LocalizableResourceString(nameof(UseCompareTagResources.Title),
                UseCompareTagResources.ResourceManager,
                typeof(UseCompareTagResources)),
            // a localizable format message string, which can be passed as the first argument to System.String.Format(System.String, System.Object[]) when creating the diagnostic message with this descriptor
            messageFormat: new LocalizableResourceString(nameof(UseCompareTagResources.MessageFormat),
                UseCompareTagResources.ResourceManager, typeof(UseCompareTagResources)),
            // the category of the diagnostic (like Design, Naming etc.)
            category: DiagnosticCategories.GC,
            // the default severity of the diagnostic
            defaultSeverity: DiagnosticSeverity.Warning,
            // the diagnostic is enabled by default
            isEnabledByDefault: true,
            // an optional longer localizable description for the diagnostic
            description: new LocalizableResourceString(nameof(UseCompareTagResources.Description),
                UseCompareTagResources.ResourceManager, typeof(UseCompareTagResources)));


        /// A DiagnosticDescriptor describing the "Find methods in Update" diagnostic.
        public static readonly DiagnosticDescriptor DoNotUseFindMethodsInUpdate = new DiagnosticDescriptor(
            // an unique identifier for the diagnostic
            id: DiagnosticIDs.DoNotUseFindMethodsInUpdate,
            // a short localizable title describing the diagnostic
            title: new LocalizableResourceString(nameof(DoNotUseFindMethodsInUpdateResources.Title),
                DoNotUseFindMethodsInUpdateResources.ResourceManager, typeof(DoNotUseFindMethodsInUpdateResources)),
            // a localizable format message string, which can be passed as the first argument to System.String.Format(System.String, System.Object[]) when creating the diagnostic message with this descriptor
            messageFormat: new LocalizableResourceString(nameof(DoNotUseFindMethodsInUpdateResources.MessageFormat),
                DoNotUseFindMethodsInUpdateResources.ResourceManager, typeof(DoNotUseFindMethodsInUpdateResources)),
            // the category of the diagnostic (like Design, Naming etc.)
            category: DiagnosticCategories.Performance,
            // the default severity of the diagnostic
            defaultSeverity: DiagnosticSeverity.Warning,
            // the diagnostic is enabled by default
            isEnabledByDefault: true,
            // an optional longer localizable description for the diagnostic
            description: new LocalizableResourceString(nameof(DoNotUseFindMethodsInUpdateResources.Description),
                DoNotUseFindMethodsInUpdateResources.ResourceManager, typeof(DoNotUseFindMethodsInUpdateResources)));


        /// A DiagnosticDescriptor describing the "Find methods in child methods in Update" diagnostic.
        public static readonly DiagnosticDescriptor DoNotUseFindMethodsInUpdateRecursive = new DiagnosticDescriptor(
            // an unique identifier for the diagnostic
            id: DiagnosticIDs.DoNotUseFindMethodsInUpdate,
            // a short localizable title describing the diagnostic
            title: new LocalizableResourceString(nameof(DoNotUseFindMethodsInUpdateResources.Title),
                DoNotUseFindMethodsInUpdateResources.ResourceManager, typeof(DoNotUseFindMethodsInUpdateResources)),
            // a localizable format message string, which can be passed as the first argument to System.String.Format(System.String, System.Object[]) when creating the diagnostic message with this descriptor
            messageFormat: new LocalizableResourceString(
                nameof(DoNotUseFindMethodsInUpdateResources.MessageFormatRecursive),
                DoNotUseFindMethodsInUpdateResources.ResourceManager, typeof(DoNotUseFindMethodsInUpdateResources)),
            // the category of the diagnostic (like Design, Naming etc.)
            category: DiagnosticCategories.Performance,
            // the default severity of the diagnostic
            defaultSeverity: DiagnosticSeverity.Warning,
            // the diagnostic is enabled by default
            isEnabledByDefault: true,
            // an optional longer localizable description for the diagnostic
            description: new LocalizableResourceString(nameof(DoNotUseFindMethodsInUpdateResources.Description),
                DoNotUseFindMethodsInUpdateResources.ResourceManager, typeof(DoNotUseFindMethodsInUpdateResources)));


        /// A DiagnosticDescriptor describing the "using Remoting directive" diagnostic.
        public static readonly DiagnosticDescriptor DoNotUseRemoting = new DiagnosticDescriptor(
            // an unique identifier for the diagnostic
            id: DiagnosticIDs.DoNotUseRemoting,
            // a short localizable title describing the diagnostic
            title: new LocalizableResourceString(nameof(DoNotUseRemotingResources.Title),
                DoNotUseRemotingResources.ResourceManager, typeof(DoNotUseRemotingResources)),
            // a localizable format message string, which can be passed as the first argument to System.String.Format(System.String, System.Object[]) when creating the diagnostic message with this descriptor
            messageFormat: new LocalizableResourceString(nameof(DoNotUseRemotingResources.MessageFormat),
                DoNotUseRemotingResources.ResourceManager, typeof(DoNotUseRemotingResources)),
            // the category of the diagnostic (like Design, Naming etc.)
            category: DiagnosticCategories.AOT,
            // the default severity of the diagnostic
            defaultSeverity: DiagnosticSeverity.Warning,
            // the diagnostic is enabled by default
            isEnabledByDefault: true,
            // an optional longer localizable description for the diagnostic
            description: new LocalizableResourceString(nameof(DoNotUseRemotingResources.Description),
                DoNotUseRemotingResources.ResourceManager, typeof(DoNotUseRemotingResources)));


        /// A DiagnosticDescriptor describing the "using System.Reflection.Emit" diagnostic.
        public static readonly DiagnosticDescriptor DoNotUseReflectionEmit = new DiagnosticDescriptor(
            // an unique identifier for the diagnostic
            id: DiagnosticIDs.DoNotUseReflectionEmit,
            // a short localizable title describing the diagnostic
            title: new LocalizableResourceString(nameof(DoNotUseReflectionEmitResources.Title),
                DoNotUseReflectionEmitResources.ResourceManager, typeof(DoNotUseReflectionEmitResources)),
            // a localizable format message string, which can be passed as the first argument to System.String.Format(System.String, System.Object[]) when creating the diagnostic message with this descriptor
            messageFormat: new LocalizableResourceString(nameof(DoNotUseReflectionEmitResources.MessageFormat),
                DoNotUseReflectionEmitResources.ResourceManager, typeof(DoNotUseReflectionEmitResources)),
            // the category of the diagnostic (like Design, Naming etc.)
            category: DiagnosticCategories.AOT,
            // the default severity of the diagnostic
            defaultSeverity: DiagnosticSeverity.Warning,
            // the diagnostic is enabled by default
            isEnabledByDefault: true,
            // an optional longer localizable description for the diagnostic
            description: new LocalizableResourceString(nameof(DoNotUseReflectionEmitResources.Description),
                DoNotUseReflectionEmitResources.ResourceManager, typeof(DoNotUseReflectionEmitResources)));


        /// A DiagnosticDescriptor describing the "Type.GetType" diagnostic.
        public static readonly DiagnosticDescriptor TypeGetType = new DiagnosticDescriptor(
            // an unique identifier for the diagnostic
            id: DiagnosticIDs.TypeGetType,
            // a short localizable title describing the diagnostic
            title: new LocalizableResourceString(nameof(TypeGetTypeResources.Title),
                TypeGetTypeResources.ResourceManager,
                typeof(TypeGetTypeResources)),
            // a localizable format message string, which can be passed as the first argument to System.String.Format(System.String, System.Object[]) when creating the diagnostic message with this descriptor
            messageFormat: new LocalizableResourceString(nameof(TypeGetTypeResources.MessageFormat),
                TypeGetTypeResources.ResourceManager, typeof(TypeGetTypeResources)),
            // the category of the diagnostic (like Design, Naming etc.)
            category: DiagnosticCategories.AOT,
            // the default severity of the diagnostic
            defaultSeverity: DiagnosticSeverity.Warning,
            // the diagnostic is enabled by default
            isEnabledByDefault: true,
            // an optional longer localizable description for the diagnostic
            description: new LocalizableResourceString(nameof(TypeGetTypeResources.Description),
                TypeGetTypeResources.ResourceManager, typeof(TypeGetTypeResources)));


        /// A DiagnosticDescriptor describing the "ForEach in Update" diagnostic.
        public static readonly DiagnosticDescriptor DoNotUseForEachInUpdate = new DiagnosticDescriptor(
            // an unique identifier for the diagnostic
            id: DiagnosticIDs.DoNotUseForEachInUpdate,
            // a short localizable title describing the diagnostic
            title: new LocalizableResourceString(nameof(DoNotUseForEachInUpdateResources.Title),
                DoNotUseForEachInUpdateResources.ResourceManager, typeof(DoNotUseForEachInUpdateResources)),
            // a localizable format message string, which can be passed as the first argument to System.String.Format(System.String, System.Object[]) when creating the diagnostic message with this descriptor
            messageFormat: new LocalizableResourceString(nameof(DoNotUseForEachInUpdateResources.MessageFormat),
                DoNotUseForEachInUpdateResources.ResourceManager, typeof(DoNotUseForEachInUpdateResources)),
            // the category of the diagnostic (like Design, Naming etc.)
            category: DiagnosticCategories.Performance,
            // the default severity of the diagnostic
            defaultSeverity: DiagnosticSeverity.Warning,
            // the diagnostic is enabled by default
            isEnabledByDefault: true,
            // an optional longer localizable description for the diagnostic
            description: new LocalizableResourceString(nameof(DoNotUseForEachInUpdateResources.Description),
                DoNotUseForEachInUpdateResources.ResourceManager, typeof(DoNotUseForEachInUpdateResources))
        );


        /// A DiagnosticDescriptor describing the "Unsealed derived class" diagnostic.
        public static readonly DiagnosticDescriptor UnsealedDerivedClass = new DiagnosticDescriptor(
            // an unique identifier for the diagnostic
            id: DiagnosticIDs.UnsealedDerivedClass,
            // a short localizable title describing the diagnostic
            title: new LocalizableResourceString(nameof(UnsealedDerivedClassResources.Title),
                UnsealedDerivedClassResources.ResourceManager, typeof(UnsealedDerivedClassResources)),
            // a localizable format message string, which can be passed as the first argument to System.String.Format(System.String, System.Object[]) when creating the diagnostic message with this descriptor
            messageFormat: new LocalizableResourceString(nameof(UnsealedDerivedClassResources.MessageFormat),
                UnsealedDerivedClassResources.ResourceManager, typeof(UnsealedDerivedClassResources)),
            // the category of the diagnostic (like Design, Naming etc.)
            category: DiagnosticCategories.Performance,
            // the default severity of the diagnostic
            defaultSeverity: DiagnosticSeverity.Warning,
            // the diagnostic is enabled by default
            isEnabledByDefault: true,
            // an optional longer localizable description for the diagnostic
            description: new LocalizableResourceString(nameof(UnsealedDerivedClassResources.Description),
                UnsealedDerivedClassResources.ResourceManager, typeof(UnsealedDerivedClassResources))
        );


        /// A DiagnosticDescriptor describing the "" diagnostic.
        public static readonly DiagnosticDescriptor InvokeFunctionMissing = new DiagnosticDescriptor(
            // an unique identifier for the diagnostic
            id: DiagnosticIDs.InvokeFunctionMissing,
            // a short localizable title describing the diagnostic
            title: new LocalizableResourceString(nameof(InvokeFunctionMissingResources.Title),
                InvokeFunctionMissingResources.ResourceManager, typeof(InvokeFunctionMissingResources)),
            // a localizable format message string, which can be passed as the first argument to System.String.Format(System.String, System.Object[]) when creating the diagnostic message with this descriptor
            messageFormat: new LocalizableResourceString(nameof(InvokeFunctionMissingResources.MessageFormat),
                InvokeFunctionMissingResources.ResourceManager, typeof(InvokeFunctionMissingResources)),
            // the category of the diagnostic (like Design, Naming etc.)
            category: DiagnosticCategories.Performance,
            // the default severity of the diagnostic
            defaultSeverity: DiagnosticSeverity.Warning,
            // the diagnostic is enabled by default
            isEnabledByDefault: true,
            // an optional longer localizable description for the diagnostic
            description: new LocalizableResourceString(nameof(InvokeFunctionMissingResources.Description),
                InvokeFunctionMissingResources.ResourceManager, typeof(InvokeFunctionMissingResources))
        );


        /// A DiagnosticDescriptor describing the "Camera.main" diagnostic.
        public static readonly DiagnosticDescriptor DoNotUseCameraMain = new DiagnosticDescriptor(
            // an unique identifier for the diagnostic
            id: DiagnosticIDs.DoNotUseCameraMain,
            // a short localizable title describing the diagnostic
            title: new LocalizableResourceString(nameof(DoNotUseCameraMainResources.Title),
                DoNotUseCameraMainResources.ResourceManager, typeof(DoNotUseCameraMainResources)),
            // a localizable format message string, which can be passed as the first argument to System.String.Format(System.String, System.Object[]) when creating the diagnostic message with this descriptor
            messageFormat: new LocalizableResourceString(nameof(DoNotUseCameraMainResources.MessageFormat),
                DoNotUseCameraMainResources.ResourceManager, typeof(DoNotUseCameraMainResources)),
            // the category of the diagnostic (like Design, Naming etc.)
            category: DiagnosticCategories.Performance,
            // the default severity of the diagnostic
            defaultSeverity: DiagnosticSeverity.Warning,
            // the diagnostic is enabled by default
            isEnabledByDefault: true,
            // an optional longer localizable description for the diagnostic
            description: new LocalizableResourceString(nameof(DoNotUseCameraMainResources.Description),
                DoNotUseCameraMainResources.ResourceManager, typeof(DoNotUseCameraMainResources)));


        /// A DiagnosticDescriptor describing the "use hashcodes instead of strings" diagnostic.
        public static readonly DiagnosticDescriptor UseHashInsteadOfString = new DiagnosticDescriptor(
            // an unique identifier for the diagnostic
            id: DiagnosticIDs.UseHashInsteadOfString,
            // a short localizable title describing the diagnostic
            title: new LocalizableResourceString(nameof(UseHashInsteadOfStringResources.Title),
                UseHashInsteadOfStringResources.ResourceManager, typeof(UseHashInsteadOfStringResources)),
            // a localizable format message string, which can be passed as the first argument to System.String.Format(System.String, System.Object[]) when creating the diagnostic message with this descriptor
            messageFormat: new LocalizableResourceString(nameof(UseHashInsteadOfStringResources.MessageFormat),
                UseHashInsteadOfStringResources.ResourceManager, typeof(UseHashInsteadOfStringResources)),
            // the category of the diagnostic (like Design, Naming etc.)
            category: DiagnosticCategories.Performance,
            // the default severity of the diagnostic
            defaultSeverity: DiagnosticSeverity.Warning,
            // the diagnostic is enabled by default
            isEnabledByDefault: true,
            // an optional longer localizable description for the diagnostic
            description: new LocalizableResourceString(nameof(UseHashInsteadOfStringResources.Description),
                UseHashInsteadOfStringResources.ResourceManager, typeof(UseHashInsteadOfStringResources)));


        /// A DiagnosticDescriptor describing the "use multi-dimension array" diagnostic.
        public static readonly DiagnosticDescriptor DoNotUseMultiDimensionalArray = new DiagnosticDescriptor(
            // an unique identifier for the diagnostic
            id: DiagnosticIDs.DoNotUseMultiDimensionalArray,
            // a short localizable title describing the diagnostic
            title: new LocalizableResourceString(nameof(DoNotUseMultiDimensionalArrayResources.Title),
                DoNotUseMultiDimensionalArrayResources.ResourceManager, typeof(DoNotUseMultiDimensionalArrayResources)),
            // a localizable format message string, which can be passed as the first argument to System.String.Format(System.String, System.Object[]) when creating the diagnostic message with this descriptor
            messageFormat: new LocalizableResourceString(nameof(DoNotUseMultiDimensionalArrayResources.MessageFormat),
                DoNotUseMultiDimensionalArrayResources.ResourceManager, typeof(DoNotUseMultiDimensionalArrayResources)),
            // the category of the diagnostic (like Design, Naming etc.)
            category: DiagnosticCategories.Performance,
            // the default severity of the diagnostic
            defaultSeverity: DiagnosticSeverity.Warning,
            // the diagnostic is enabled by default
            isEnabledByDefault: true,
            // an optional longer localizable description for the diagnostic
            description: new LocalizableResourceString(nameof(DoNotUseMultiDimensionalArrayResources.Description),
                DoNotUseMultiDimensionalArrayResources.ResourceManager,
                typeof(DoNotUseMultiDimensionalArrayResources)));


        /// A DiagnosticDescriptor describing the "VectorX.Distance" diagnostic.
        public static readonly DiagnosticDescriptor DoNotUseVectorDistance = new DiagnosticDescriptor(
            // an unique identifier for the diagnostic
            id: DiagnosticIDs.DoNotUseVectorDistance,
            // a short localizable title describing the diagnostic
            title: new LocalizableResourceString(nameof(DoNotUseVectorDistanceResources.Title),
                DoNotUseVectorDistanceResources.ResourceManager, typeof(DoNotUseVectorDistanceResources)),
            // a localizable format message string, which can be passed as the first argument to System.String.Format(System.String, System.Object[]) when creating the diagnostic message with this descriptor
            messageFormat: new LocalizableResourceString(nameof(DoNotUseVectorDistanceResources.MessageFormat),
                DoNotUseVectorDistanceResources.ResourceManager, typeof(DoNotUseVectorDistanceResources)),
            // the category of the diagnostic (like Design, Naming etc.)
            category: DiagnosticCategories.Performance,
            // the default severity of the diagnostic
            defaultSeverity: DiagnosticSeverity.Warning,
            // the diagnostic is enabled by default
            isEnabledByDefault: true,
            // an optional longer localizable description for the diagnostic
            description: new LocalizableResourceString(nameof(DoNotUseVectorDistanceResources.Description),
                DoNotUseVectorDistanceResources.ResourceManager, typeof(DoNotUseVectorDistanceResources)));


        /// A DiagnosticDescriptor describing the "calculation order" diagnostic.
        public static readonly DiagnosticDescriptor CalculationOrder = new DiagnosticDescriptor(
            // an unique identifier for the diagnostic
            id: DiagnosticIDs.CalculationOrder,
            // a short localizable title describing the diagnostic
            title: new LocalizableResourceString(nameof(CalculationOrderResources.Title),
                CalculationOrderResources.ResourceManager, typeof(CalculationOrderResources)),
            // a localizable format message string, which can be passed as the first argument to System.String.Format(System.String, System.Object[]) when creating the diagnostic message with this descriptor
            messageFormat: new LocalizableResourceString(nameof(CalculationOrderResources.MessageFormat),
                CalculationOrderResources.ResourceManager, typeof(CalculationOrderResources)),
            // the category of the diagnostic (like Design, Naming etc.)
            category: DiagnosticCategories.Performance,
            // the default severity of the diagnostic
            defaultSeverity: DiagnosticSeverity.Warning,
            // the diagnostic is enabled by default
            isEnabledByDefault: true,
            // an optional longer localizable description for the diagnostic
            description: new LocalizableResourceString(nameof(CalculationOrderResources.Description),
                CalculationOrderResources.ResourceManager, typeof(CalculationOrderResources)));


        /// A DiagnosticDescriptor describing the "Camera.rect" diagnostic.
        public static readonly DiagnosticDescriptor DoNotUseCameraRect = new DiagnosticDescriptor(
            // an unique identifier for the diagnostic
            id: DiagnosticIDs.DoNotUseCameraRect,
            // a short localizable title describing the diagnostic
            title: new LocalizableResourceString(nameof(DoNotUseCameraRectResources.Title),
                DoNotUseCameraRectResources.ResourceManager, typeof(DoNotUseCameraRectResources)),
            // a localizable format message string, which can be passed as the first argument to System.String.Format(System.String, System.Object[]) when creating the diagnostic message with this descriptor
            messageFormat: new LocalizableResourceString(nameof(DoNotUseCameraRectResources.MessageFormat),
                DoNotUseCameraRectResources.ResourceManager, typeof(DoNotUseCameraRectResources)),
            // the category of the diagnostic (like Design, Naming etc.)
            category: DiagnosticCategories.Performance,
            // the default severity of the diagnostic
            defaultSeverity: DiagnosticSeverity.Warning,
            // the diagnostic is enabled by default
            isEnabledByDefault: true,
            // an optional longer localizable description for the diagnostic
            description: new LocalizableResourceString(nameof(DoNotUseCameraRectResources.Description),
                DoNotUseCameraRectResources.ResourceManager, typeof(DoNotUseCameraRectResources)));


        /// A DiagnosticDescriptor describing the "old API methods cause GC" diagnostic.
        public static readonly DiagnosticDescriptor ReplaceOldGCMethods = new DiagnosticDescriptor(
            // an unique identifier for the diagnostic
            id: DiagnosticIDs.ReplaceOldGCMethods,
            // a short localizable title describing the diagnostic
            title: new LocalizableResourceString(nameof(ReplaceOldGCMethodsResources.Title),
                ReplaceOldGCMethodsResources.ResourceManager, typeof(ReplaceOldGCMethodsResources)),
            // a localizable format message string, which can be passed as the first argument to System.String.Format(System.String, System.Object[]) when creating the diagnostic message with this descriptor
            messageFormat: new LocalizableResourceString(nameof(ReplaceOldGCMethodsResources.MessageFormat),
                ReplaceOldGCMethodsResources.ResourceManager, typeof(ReplaceOldGCMethodsResources)),
            // the category of the diagnostic (like Design, Naming etc.)
            category: DiagnosticCategories.Performance,
            // the default severity of the diagnostic
            defaultSeverity: DiagnosticSeverity.Warning,
            // the diagnostic is enabled by default
            isEnabledByDefault: true,
            // an optional longer localizable description for the diagnostic
            description: new LocalizableResourceString(nameof(ReplaceOldGCMethodsResources.Description),
                ReplaceOldGCMethodsResources.ResourceManager, typeof(ReplaceOldGCMethodsResources)));


        /// A DiagnosticDescriptor describing the "create a container with default size" diagnostic.
        public static readonly DiagnosticDescriptor DoNotInitContainerWithoutSize = new DiagnosticDescriptor(
            // an unique identifier for the diagnostic
            id: DiagnosticIDs.DoNotInitContainerWithoutSize,
            // a short localizable title describing the diagnostic
            title: new LocalizableResourceString(nameof(DoNotInitContainerWithoutSizeResources.Title),
                DoNotInitContainerWithoutSizeResources.ResourceManager, typeof(DoNotInitContainerWithoutSizeResources)),
            // a localizable format message string, which can be passed as the first argument to System.String.Format(System.String, System.Object[]) when creating the diagnostic message with this descriptor
            messageFormat: new LocalizableResourceString(nameof(DoNotInitContainerWithoutSizeResources.MessageFormat),
                DoNotInitContainerWithoutSizeResources.ResourceManager, typeof(DoNotInitContainerWithoutSizeResources)),
            // the category of the diagnostic (like Design, Naming etc.)
            category: DiagnosticCategories.Performance,
            // the default severity of the diagnostic
            defaultSeverity: DiagnosticSeverity.Warning,
            // the diagnostic is enabled by default
            isEnabledByDefault: true,
            // an optional longer localizable description for the diagnostic
            description: new LocalizableResourceString(nameof(DoNotInitContainerWithoutSizeResources.Description),
                DoNotInitContainerWithoutSizeResources.ResourceManager,
                typeof(DoNotInitContainerWithoutSizeResources)));


        /// A DiagnosticDescriptor describing the "use enum or struct as key to dictionary" diagnostic.
        public static readonly DiagnosticDescriptor DoNotUseEnumOrStructAsKey = new DiagnosticDescriptor(
            // an unique identifier for the diagnostic
            id: DiagnosticIDs.DoNotUseEnumOrStructAsKey,
            // a short localizable title describing the diagnostic
            title: new LocalizableResourceString(nameof(DoNotUseEnumOrStructAsKeyResources.Title),
                DoNotUseEnumOrStructAsKeyResources.ResourceManager, typeof(DoNotUseEnumOrStructAsKeyResources)),
            // a localizable format message string, which can be passed as the first argument to System.String.Format(System.String, System.Object[]) when creating the diagnostic message with this descriptor
            messageFormat: new LocalizableResourceString(nameof(DoNotUseEnumOrStructAsKeyResources.MessageFormat),
                DoNotUseEnumOrStructAsKeyResources.ResourceManager, typeof(DoNotUseEnumOrStructAsKeyResources)),
            // the category of the diagnostic (like Design, Naming etc.)
            category: DiagnosticCategories.Performance,
            // the default severity of the diagnostic
            defaultSeverity: DiagnosticSeverity.Warning,
            // the diagnostic is enabled by default
            isEnabledByDefault: true,
            // an optional longer localizable description for the diagnostic
            description: new LocalizableResourceString(nameof(DoNotUseEnumOrStructAsKeyResources.Description),
                DoNotUseEnumOrStructAsKeyResources.ResourceManager, typeof(DoNotUseEnumOrStructAsKeyResources)));


        /// A DiagnosticDescriptor describing the "yield return 0" diagnostic.
        public static readonly DiagnosticDescriptor DoNotYieldReturnZero = new DiagnosticDescriptor(
            // an unique identifier for the diagnostic
            id: DiagnosticIDs.DoNotYieldReturnZero,
            // a short localizable title describing the diagnostic
            title: new LocalizableResourceString(nameof(DoNotYieldReturnZeroResources.Title),
                DoNotYieldReturnZeroResources.ResourceManager, typeof(DoNotYieldReturnZeroResources)),
            // a localizable format message string, which can be passed as the first argument to System.String.Format(System.String, System.Object[]) when creating the diagnostic message with this descriptor
            messageFormat: new LocalizableResourceString(nameof(DoNotYieldReturnZeroResources.MessageFormat),
                DoNotYieldReturnZeroResources.ResourceManager, typeof(DoNotYieldReturnZeroResources)),
            // the category of the diagnostic (like Design, Naming etc.)
            category: DiagnosticCategories.Performance,
            // the default severity of the diagnostic
            defaultSeverity: DiagnosticSeverity.Warning,
            // the diagnostic is enabled by default
            isEnabledByDefault: true,
            // an optional longer localizable description for the diagnostic
            description: new LocalizableResourceString(nameof(DoNotYieldReturnZeroResources.Description),
                DoNotYieldReturnZeroResources.ResourceManager, typeof(DoNotYieldReturnZeroResources)));


        /// A DiagnosticDescriptor describing the "catenating strings by '+' operator" diagnostic.
        public static readonly DiagnosticDescriptor DoNotUseAddOpOnString = new DiagnosticDescriptor(
            // an unique identifier for the diagnostic
            id: DiagnosticIDs.DoNotUseAddOpOnString,
            // a short localizable title describing the diagnostic
            title: new LocalizableResourceString(nameof(DoNotUseAddOpOnStringResources.Title),
                DoNotUseAddOpOnStringResources.ResourceManager, typeof(DoNotUseAddOpOnStringResources)),
            // a localizable format message string, which can be passed as the first argument to System.String.Format(System.String, System.Object[]) when creating the diagnostic message with this descriptor
            messageFormat: new LocalizableResourceString(nameof(DoNotUseAddOpOnStringResources.MessageFormat),
                DoNotUseAddOpOnStringResources.ResourceManager, typeof(DoNotUseAddOpOnStringResources)),
            // the category of the diagnostic (like Design, Naming etc.)
            category: DiagnosticCategories.Performance,
            // the default severity of the diagnostic
            defaultSeverity: DiagnosticSeverity.Warning,
            // the diagnostic is enabled by default
            isEnabledByDefault: true,
            // an optional longer localizable description for the diagnostic
            description: new LocalizableResourceString(nameof(DoNotUseAddOpOnStringResources.Description),
                DoNotUseAddOpOnStringResources.ResourceManager, typeof(DoNotUseAddOpOnStringResources)));


        /// A DiagnosticDescriptor describing the "catenating strings by '==' operator" diagnostic.
        public static readonly DiagnosticDescriptor DoNotUseEqualOpOnString = new DiagnosticDescriptor(
            // an unique identifier for the diagnostic
            id: DiagnosticIDs.DoNotUseEqualOpOnString,
            // a short localizable title describing the diagnostic
            title: new LocalizableResourceString(nameof(DoNotUseEqualOpOnStringResources.Title),
                DoNotUseEqualOpOnStringResources.ResourceManager, typeof(DoNotUseEqualOpOnStringResources)),
            // a localizable format message string, which can be passed as the first argument to System.String.Format(System.String, System.Object[]) when creating the diagnostic message with this descriptor
            messageFormat: new LocalizableResourceString(nameof(DoNotUseEqualOpOnStringResources.MessageFormat),
                DoNotUseEqualOpOnStringResources.ResourceManager, typeof(DoNotUseEqualOpOnStringResources)),
            // the category of the diagnostic (like Design, Naming etc.)
            category: DiagnosticCategories.Performance,
            // the default severity of the diagnostic
            defaultSeverity: DiagnosticSeverity.Warning,
            // the diagnostic is enabled by default
            isEnabledByDefault: true,
            // an optional longer localizable description for the diagnostic
            description: new LocalizableResourceString(nameof(DoNotUseEqualOpOnStringResources.Description),
                DoNotUseEqualOpOnStringResources.ResourceManager, typeof(DoNotUseEqualOpOnStringResources)));


        /// A DiagnosticDescriptor describing the "regular expression" diagnostic.
        public static readonly DiagnosticDescriptor DoNotUseRegex = new DiagnosticDescriptor(
            // an unique identifier for the diagnostic
            id: DiagnosticIDs.DoNotUseRegex,
            // a short localizable title describing the diagnostic
            title: new LocalizableResourceString(nameof(DoNotUseRegexResources.Title),
                DoNotUseRegexResources.ResourceManager,
                typeof(DoNotUseRegexResources)),
            // a localizable format message string, which can be passed as the first argument to System.String.Format(System.String, System.Object[]) when creating the diagnostic message with this descriptor
            messageFormat: new LocalizableResourceString(nameof(DoNotUseRegexResources.MessageFormat),
                DoNotUseRegexResources.ResourceManager, typeof(DoNotUseRegexResources)),
            // the category of the diagnostic (like Design, Naming etc.)
            category: DiagnosticCategories.Performance,
            // the default severity of the diagnostic
            defaultSeverity: DiagnosticSeverity.Warning,
            // the diagnostic is enabled by default
            isEnabledByDefault: true,
            // an optional longer localizable description for the diagnostic
            description: new LocalizableResourceString(nameof(DoNotUseRegexResources.Description),
                DoNotUseRegexResources.ResourceManager, typeof(DoNotUseRegexResources)));


        /// A DiagnosticDescriptor describing the "use sharedMaterial instead of material" diagnostic.
        public static readonly DiagnosticDescriptor DoNotUseMaterial = new DiagnosticDescriptor(
            // an unique identifier for the diagnostic
            id: DiagnosticIDs.DoNotUseMaterial,
            // a short localizable title describing the diagnostic
            title: new LocalizableResourceString(nameof(DoNotUseMaterialResources.Title),
                DoNotUseMaterialResources.ResourceManager, typeof(DoNotUseMaterialResources)),
            // a localizable format message string, which can be passed as the first argument to System.String.Format(System.String, System.Object[]) when creating the diagnostic message with this descriptor
            messageFormat: new LocalizableResourceString(nameof(DoNotUseMaterialResources.MessageFormat),
                DoNotUseMaterialResources.ResourceManager, typeof(DoNotUseMaterialResources)),
            // the category of the diagnostic (like Design, Naming etc.)
            category: DiagnosticCategories.Performance,
            // the default severity of the diagnostic
            defaultSeverity: DiagnosticSeverity.Warning,
            // the diagnostic is enabled by default
            isEnabledByDefault: true,
            // an optional longer localizable description for the diagnostic
            description: new LocalizableResourceString(nameof(DoNotUseMaterialResources.Description),
                DoNotUseMaterialResources.ResourceManager, typeof(DoNotUseMaterialResources)));


        /// A DiagnosticDescriptor describing the "new container in update" diagnostic.
        public static readonly DiagnosticDescriptor DoNotNewContainerInUpdate = new DiagnosticDescriptor(
            // an unique identifier for the diagnostic
            id: DiagnosticIDs.DoNotNewContainerInUpdate,
            // a short localizable title describing the diagnostic
            title: new LocalizableResourceString(nameof(DoNotNewContainerInUpdateResources.Title),
                DoNotNewContainerInUpdateResources.ResourceManager, typeof(DoNotNewContainerInUpdateResources)),
            // a localizable format message string, which can be passed as the first argument to System.String.Format(System.String, System.Object[]) when creating the diagnostic message with this descriptor
            messageFormat: new LocalizableResourceString(nameof(DoNotNewContainerInUpdateResources.MessageFormat),
                DoNotNewContainerInUpdateResources.ResourceManager, typeof(DoNotNewContainerInUpdateResources)),
            // the category of the diagnostic (like Design, Naming etc.)
            category: DiagnosticCategories.Performance,
            // the default severity of the diagnostic
            defaultSeverity: DiagnosticSeverity.Warning,
            // the diagnostic is enabled by default
            isEnabledByDefault: true,
            // an optional longer localizable description for the diagnostic
            description: new LocalizableResourceString(nameof(DoNotNewContainerInUpdateResources.Description),
                DoNotNewContainerInUpdateResources.ResourceManager, typeof(DoNotNewContainerInUpdateResources)));


        /// A DiagnosticDescriptor describing the "lambda" diagnostic.
        public static readonly DiagnosticDescriptor DoNotUseLambda = new DiagnosticDescriptor(
            // an unique identifier for the diagnostic
            id: DiagnosticIDs.DoNotUseLambda,
            // a short localizable title describing the diagnostic
            title: new LocalizableResourceString(nameof(DoNotUseLambdaResources.Title),
                DoNotUseLambdaResources.ResourceManager, typeof(DoNotUseLambdaResources)),
            // a localizable format message string, which can be passed as the first argument to System.String.Format(System.String, System.Object[]) when creating the diagnostic message with this descriptor
            messageFormat: new LocalizableResourceString(nameof(DoNotUseLambdaResources.MessageFormat),
                DoNotUseLambdaResources.ResourceManager, typeof(DoNotUseLambdaResources)),
            // the category of the diagnostic (like Design, Naming etc.)
            category: DiagnosticCategories.Performance,
            // the default severity of the diagnostic
            defaultSeverity: DiagnosticSeverity.Warning,
            // the diagnostic is enabled by default
            isEnabledByDefault: true,
            // an optional longer localizable description for the diagnostic
            description: new LocalizableResourceString(nameof(DoNotUseLambdaResources.Description),
                DoNotUseLambdaResources.ResourceManager, typeof(DoNotUseLambdaResources)));


        /// A DiagnosticDescriptor describing the "return container" diagnostic.
        public static readonly DiagnosticDescriptor DoNotReturnContainer = new DiagnosticDescriptor(
            // an unique identifier for the diagnostic
            id: DiagnosticIDs.DoNotReturnContainer,
            // a short localizable title describing the diagnostic
            title: new LocalizableResourceString(nameof(DoNotReturnContainerResources.Title),
                DoNotReturnContainerResources.ResourceManager, typeof(DoNotReturnContainerResources)),
            // a localizable format message string, which can be passed as the first argument to System.String.Format(System.String, System.Object[]) when creating the diagnostic message with this descriptor
            messageFormat: new LocalizableResourceString(nameof(DoNotReturnContainerResources.MessageFormat),
                DoNotReturnContainerResources.ResourceManager, typeof(DoNotReturnContainerResources)),
            // the category of the diagnostic (like Design, Naming etc.)
            category: DiagnosticCategories.Performance,
            // the default severity of the diagnostic
            defaultSeverity: DiagnosticSeverity.Warning,
            // the diagnostic is enabled by default
            isEnabledByDefault: true,
            // an optional longer localizable description for the diagnostic
            description: new LocalizableResourceString(nameof(DoNotReturnContainerResources.Description),
                DoNotReturnContainerResources.ResourceManager, typeof(DoNotReturnContainerResources)));


        /// A DiagnosticDescriptor describing the "string.StartsWith() and string.EndsWith()" diagnostic.
        public static readonly DiagnosticDescriptor DoNotUseStringCompareMethods = new DiagnosticDescriptor(
            // an unique identifier for the diagnostic
            id: DiagnosticIDs.DoNotUseStringCompareMethods,
            // a short localizable title describing the diagnostic
            title: new LocalizableResourceString(nameof(DoNotUseStringCompareMethodsResources.Title),
                DoNotUseStringCompareMethodsResources.ResourceManager, typeof(DoNotUseStringCompareMethodsResources)),
            // a localizable format message string, which can be passed as the first argument to System.String.Format(System.String, System.Object[]) when creating the diagnostic message with this descriptor
            messageFormat: new LocalizableResourceString(nameof(DoNotUseStringCompareMethodsResources.MessageFormat),
                DoNotUseStringCompareMethodsResources.ResourceManager, typeof(DoNotUseStringCompareMethodsResources)),
            // the category of the diagnostic (like Design, Naming etc.)
            category: DiagnosticCategories.Performance,
            // the default severity of the diagnostic
            defaultSeverity: DiagnosticSeverity.Warning,
            // the diagnostic is enabled by default
            isEnabledByDefault: true,
            // an optional longer localizable description for the diagnostic
            description: new LocalizableResourceString(nameof(DoNotUseStringCompareMethodsResources.Description),
                DoNotUseStringCompareMethodsResources.ResourceManager, typeof(DoNotUseStringCompareMethodsResources)));


        /// A DiagnosticDescriptor describing the "lambda in generic method" diagnostic.
        public static readonly DiagnosticDescriptor DoNotUseLambdaInGenericMethod = new DiagnosticDescriptor(
            // an unique identifier for the diagnostic
            id: DiagnosticIDs.DoNotUseLambdaInGenericMethod,
            // a short localizable title describing the diagnostic
            title: new LocalizableResourceString(nameof(DoNotUseLambdaInGenericMethodResources.Title),
                DoNotUseLambdaInGenericMethodResources.ResourceManager, typeof(DoNotUseLambdaInGenericMethodResources)),
            // a localizable format message string, which can be passed as the first argument to System.String.Format(System.String, System.Object[]) when creating the diagnostic message with this descriptor
            messageFormat: new LocalizableResourceString(nameof(DoNotUseLambdaInGenericMethodResources.MessageFormat),
                DoNotUseLambdaInGenericMethodResources.ResourceManager, typeof(DoNotUseLambdaInGenericMethodResources)),
            // the category of the diagnostic (like Design, Naming etc.)
            category: DiagnosticCategories.Performance,
            // the default severity of the diagnostic
            defaultSeverity: DiagnosticSeverity.Warning,
            // the diagnostic is enabled by default
            isEnabledByDefault: true,
            // an optional longer localizable description for the diagnostic
            description: new LocalizableResourceString(nameof(DoNotUseLambdaInGenericMethodResources.Description),
                DoNotUseLambdaInGenericMethodResources.ResourceManager,
                typeof(DoNotUseLambdaInGenericMethodResources)));


        /// A DiagnosticDescriptor describing the "boxing" diagnostic.
        public static readonly DiagnosticDescriptor Boxing = new DiagnosticDescriptor(
            // an unique identifier for the diagnostic
            id: DiagnosticIDs.Boxing,
            // a short localizable title describing the diagnostic
            title: new LocalizableResourceString(nameof(BoxingResources.Title), BoxingResources.ResourceManager,
                typeof(BoxingResources)),
            // a localizable format message string, which can be passed as the first argument to System.String.Format(System.String, System.Object[]) when creating the diagnostic message with this descriptor
            messageFormat: new LocalizableResourceString(nameof(BoxingResources.MessageFormat),
                BoxingResources.ResourceManager, typeof(BoxingResources)),
            // the category of the diagnostic (like Design, Naming etc.)
            category: DiagnosticCategories.Performance,
            // the default severity of the diagnostic
            defaultSeverity: DiagnosticSeverity.Warning,
            // the diagnostic is enabled by default
            isEnabledByDefault: true,
            // an optional longer localizable description for the diagnostic
            description: new LocalizableResourceString(nameof(BoxingResources.Description),
                BoxingResources.ResourceManager,
                typeof(BoxingResources)));


        /// A DiagnosticDescriptor describing the "method to delegate" diagnostic.
        public static readonly DiagnosticDescriptor MethodToDelegate = new DiagnosticDescriptor(
            // an unique identifier for the diagnostic
            id: DiagnosticIDs.MethodToDelegate,
            // a short localizable title describing the diagnostic
            title: new LocalizableResourceString(nameof(MethodToDelegateResources.Title),
                MethodToDelegateResources.ResourceManager, typeof(MethodToDelegateResources)),
            // a localizable format message string, which can be passed as the first argument to System.String.Format(System.String, System.Object[]) when creating the diagnostic message with this descriptor
            messageFormat: new LocalizableResourceString(nameof(MethodToDelegateResources.MessageFormat),
                MethodToDelegateResources.ResourceManager, typeof(MethodToDelegateResources)),
            // the category of the diagnostic (like Design, Naming etc.)
            category: DiagnosticCategories.Performance,
            // the default severity of the diagnostic
            defaultSeverity: DiagnosticSeverity.Warning,
            // the diagnostic is enabled by default
            isEnabledByDefault: true,
            // an optional longer localizable description for the diagnostic
            description: new LocalizableResourceString(nameof(MethodToDelegateResources.Description),
                MethodToDelegateResources.ResourceManager, typeof(MethodToDelegateResources)));


        /// A DiagnosticDescriptor describing the "UnityEngine.Debug" diagnostic.
        public static readonly DiagnosticDescriptor DoNotUseDefaultDebug = new DiagnosticDescriptor(
            // an unique identifier for the diagnostic
            id: DiagnosticIDs.DoNotUseDefaultDebug,
            // a short localizable title describing the diagnostic
            title: new LocalizableResourceString(nameof(DoNotUseDefaultDebugResources.Title),
                DoNotUseDefaultDebugResources.ResourceManager, typeof(DoNotUseDefaultDebugResources)),
            // a localizable format message string, which can be passed as the first argument to System.String.Format(System.String, System.Object[]) when creating the diagnostic message with this descriptor
            messageFormat: new LocalizableResourceString(nameof(DoNotUseDefaultDebugResources.MessageFormat),
                DoNotUseDefaultDebugResources.ResourceManager, typeof(DoNotUseDefaultDebugResources)),
            // the category of the diagnostic (like Design, Naming etc.)
            category: DiagnosticCategories.Performance,
            // the default severity of the diagnostic
            defaultSeverity: DiagnosticSeverity.Warning,
            // the diagnostic is enabled by default
            isEnabledByDefault: true,
            // an optional longer localizable description for the diagnostic
            description: new LocalizableResourceString(nameof(DoNotUseDefaultDebugResources.Description),
                DoNotUseDefaultDebugResources.ResourceManager, typeof(DoNotUseDefaultDebugResources)));


        /// A DiagnosticDescriptor describing the "cache container api" diagnostic.
        public static readonly DiagnosticDescriptor CacheContainerAPI = new DiagnosticDescriptor(
            // an unique identifier for the diagnostic
            id: DiagnosticIDs.CacheContainerAPI,
            // a short localizable title describing the diagnostic
            title: new LocalizableResourceString(nameof(CacheContainerApiResources.Title),
                CacheContainerApiResources.ResourceManager, typeof(CacheContainerApiResources)),
            // a localizable format message string, which can be passed as the first argument to System.String.Format(System.String, System.Object[]) when creating the diagnostic message with this descriptor
            messageFormat: new LocalizableResourceString(nameof(CacheContainerApiResources.MessageFormat),
                CacheContainerApiResources.ResourceManager, typeof(CacheContainerApiResources)),
            // the category of the diagnostic (like Design, Naming etc.)
            category: DiagnosticCategories.Performance,
            // the default severity of the diagnostic
            defaultSeverity: DiagnosticSeverity.Warning,
            // the diagnostic is enabled by default
            isEnabledByDefault: true,
            // an optional longer localizable description for the diagnostic
            description: new LocalizableResourceString(nameof(CacheContainerApiResources.Description),
                CacheContainerApiResources.ResourceManager, typeof(CacheContainerApiResources)));


        /// A DiagnosticDescriptor describing the "transform update" diagnostic.
        public static readonly DiagnosticDescriptor TransformUpdate = new DiagnosticDescriptor(
            // an unique identifier for the diagnostic
            id: DiagnosticIDs.TransformUpdate,
            // a short localizable title describing the diagnostic
            title: new LocalizableResourceString(nameof(TransformUpdateResources.Title),
                TransformUpdateResources.ResourceManager, typeof(TransformUpdateResources)),
            // a localizable format message string, which can be passed as the first argument to System.String.Format(System.String, System.Object[]) when creating the diagnostic message with this descriptor
            messageFormat: new LocalizableResourceString(nameof(TransformUpdateResources.MessageFormat),
                TransformUpdateResources.ResourceManager, typeof(TransformUpdateResources)),
            // the category of the diagnostic (like Design, Naming etc.)
            category: DiagnosticCategories.Performance,
            // the default severity of the diagnostic
            defaultSeverity: DiagnosticSeverity.Warning,
            // the diagnostic is enabled by default
            isEnabledByDefault: true,
            // an optional longer localizable description for the diagnostic
            description: new LocalizableResourceString(nameof(TransformUpdateResources.Description),
                TransformUpdateResources.ResourceManager, typeof(TransformUpdateResources)));


        /// A DiagnosticDescriptor describing the "ArrayList" diagnostic.
        public static readonly DiagnosticDescriptor ArrayList = new DiagnosticDescriptor(
            // an unique identifier for the diagnostic
            id: DiagnosticIDs.ArrayList,
            // a short localizable title describing the diagnostic
            title: new LocalizableResourceString(nameof(ArrayListResources.Title), ArrayListResources.ResourceManager,
                typeof(ArrayListResources)),
            // a localizable format message string, which can be passed as the first argument to System.String.Format(System.String, System.Object[]) when creating the diagnostic message with this descriptor
            messageFormat: new LocalizableResourceString(nameof(ArrayListResources.MessageFormat),
                ArrayListResources.ResourceManager, typeof(ArrayListResources)),
            // the category of the diagnostic (like Design, Naming etc.)
            category: DiagnosticCategories.Performance,
            // the default severity of the diagnostic
            defaultSeverity: DiagnosticSeverity.Warning,
            // the diagnostic is enabled by default
            isEnabledByDefault: true,
            // an optional longer localizable description for the diagnostic
            description: new LocalizableResourceString(nameof(ArrayListResources.Description),
                ArrayListResources.ResourceManager, typeof(ArrayListResources)));


        /// A DiagnosticDescriptor describing the "withChildren option of ParticleSystem API" diagnostic.
        public static readonly DiagnosticDescriptor ParticleApiWithChildren = new DiagnosticDescriptor(
            // an unique identifier for the diagnostic
            id: DiagnosticIDs.ParticleApiWithChildren,
            // a short localizable title describing the diagnostic
            title: new LocalizableResourceString(nameof(ParticleApiWithChildrenResources.Title),
                ParticleApiWithChildrenResources.ResourceManager, typeof(ParticleApiWithChildrenResources)),
            // a localizable format message string, which can be passed as the first argument to System.String.Format(System.String, System.Object[]) when creating the diagnostic message with this descriptor
            messageFormat: new LocalizableResourceString(nameof(ParticleApiWithChildrenResources.MessageFormat),
                ParticleApiWithChildrenResources.ResourceManager, typeof(ParticleApiWithChildrenResources)),
            // the category of the diagnostic (like Design, Naming etc.)
            category: DiagnosticCategories.Performance,
            // the default severity of the diagnostic
            defaultSeverity: DiagnosticSeverity.Warning,
            // the diagnostic is enabled by default
            isEnabledByDefault: true,
            // an optional longer localizable description for the diagnostic
            description: new LocalizableResourceString(nameof(ParticleApiWithChildrenResources.Description),
                ParticleApiWithChildrenResources.ResourceManager, typeof(ParticleApiWithChildrenResources)));


        /// A DiagnosticDescriptor describing the "maxDistance option of Physics.Raycast" diagnostic.
        public static readonly DiagnosticDescriptor RaycastMaxDistance = new DiagnosticDescriptor(
            // an unique identifier for the diagnostic
            id: DiagnosticIDs.RaycastMaxDistance,
            // a short localizable title describing the diagnostic
            title: new LocalizableResourceString(nameof(RaycastMaxDistanceResources.Title),
                RaycastMaxDistanceResources.ResourceManager, typeof(RaycastMaxDistanceResources)),
            // a localizable format message string, which can be passed as the first argument to System.String.Format(System.String, System.Object[]) when creating the diagnostic message with this descriptor
            messageFormat: new LocalizableResourceString(nameof(RaycastMaxDistanceResources.MessageFormat),
                RaycastMaxDistanceResources.ResourceManager, typeof(RaycastMaxDistanceResources)),
            // the category of the diagnostic (like Design, Naming etc.)
            category: DiagnosticCategories.Performance,
            // the default severity of the diagnostic
            defaultSeverity: DiagnosticSeverity.Warning,
            // the diagnostic is enabled by default
            isEnabledByDefault: true,
            // an optional longer localizable description for the diagnostic
            description: new LocalizableResourceString(nameof(RaycastMaxDistanceResources.Description),
                RaycastMaxDistanceResources.ResourceManager, typeof(RaycastMaxDistanceResources)));


        /// A DiagnosticDescriptor describing the "move rigidbody by Transform API" diagnostic.
        public static readonly DiagnosticDescriptor DoNotMoveRigidbodyByTransform = new DiagnosticDescriptor(
            // an unique identifier for the diagnostic
            id: DiagnosticIDs.DoNotMoveRigidbodyByTransform,
            // a short localizable title describing the diagnostic
            title: new LocalizableResourceString(nameof(DoNotMoveRigidbodyByTransformResources.Title),
                DoNotMoveRigidbodyByTransformResources.ResourceManager, typeof(DoNotMoveRigidbodyByTransformResources)),
            // a localizable format message string, which can be passed as the first argument to System.String.Format(System.String, System.Object[]) when creating the diagnostic message with this descriptor
            messageFormat: new LocalizableResourceString(nameof(DoNotMoveRigidbodyByTransformResources.MessageFormat),
                DoNotMoveRigidbodyByTransformResources.ResourceManager, typeof(DoNotMoveRigidbodyByTransformResources)),
            // the category of the diagnostic (like Design, Naming etc.)
            category: DiagnosticCategories.Performance,
            // the default severity of the diagnostic
            defaultSeverity: DiagnosticSeverity.Warning,
            // the diagnostic is enabled by default
            isEnabledByDefault: true,
            // an optional longer localizable description for the diagnostic
            description: new LocalizableResourceString(nameof(DoNotMoveRigidbodyByTransformResources.Description),
                DoNotMoveRigidbodyByTransformResources.ResourceManager,
                typeof(DoNotMoveRigidbodyByTransformResources)));


        /// A DiagnosticDescriptor describing the "nonReadable option in Texture API" diagnostic.
        public static readonly DiagnosticDescriptor TextureNonReadable = new DiagnosticDescriptor(
            // an unique identifier for the diagnostic
            id: DiagnosticIDs.TextureNonReadable,
            // a short localizable title describing the diagnostic
            title: new LocalizableResourceString(nameof(TextureNonReadableResources.Title),
                TextureNonReadableResources.ResourceManager, typeof(TextureNonReadableResources)),
            // a localizable format message string, which can be passed as the first argument to System.String.Format(System.String, System.Object[]) when creating the diagnostic message with this descriptor
            messageFormat: new LocalizableResourceString(nameof(TextureNonReadableResources.MessageFormat),
                TextureNonReadableResources.ResourceManager, typeof(TextureNonReadableResources)),
            // the category of the diagnostic (like Design, Naming etc.)
            category: DiagnosticCategories.Performance,
            // the default severity of the diagnostic
            defaultSeverity: DiagnosticSeverity.Warning,
            // the diagnostic is enabled by default
            isEnabledByDefault: true,
            // an optional longer localizable description for the diagnostic
            description: new LocalizableResourceString(nameof(TextureNonReadableResources.Description),
                TextureNonReadableResources.ResourceManager, typeof(TextureNonReadableResources)));


        /// A DiagnosticDescriptor describing the "Unmanaged resources" diagnostic.
        public static readonly DiagnosticDescriptor Unmanaged = new DiagnosticDescriptor(
            // an unique identifier for the diagnostic
            id: DiagnosticIDs.Unmanaged,
            // a short localizable title describing the diagnostic
            title: new LocalizableResourceString(nameof(UnmanagedResources.Title), UnmanagedResources.ResourceManager,
                typeof(UnmanagedResources)),
            // a localizable format message string, which can be passed as the first argument to System.String.Format(System.String, System.Object[]) when creating the diagnostic message with this descriptor
            messageFormat: new LocalizableResourceString(nameof(UnmanagedResources.MessageFormat),
                UnmanagedResources.ResourceManager, typeof(UnmanagedResources)),
            // the category of the diagnostic (like Design, Naming etc.)
            category: DiagnosticCategories.Performance,
            // the default severity of the diagnostic
            defaultSeverity: DiagnosticSeverity.Warning,
            // the diagnostic is enabled by default
            isEnabledByDefault: true,
            // an optional longer localizable description for the diagnostic
            description: new LocalizableResourceString(nameof(UnmanagedResources.Description),
                UnmanagedResources.ResourceManager, typeof(UnmanagedResources)));


        /// A DiagnosticDescriptor describing the "unity 'unused' flag" diagnostic.
        public static readonly DiagnosticDescriptor UnityUnusedFlag = new DiagnosticDescriptor(
            // an unique identifier for the diagnostic
            id: DiagnosticIDs.UnityUnusedFlag,
            // a short localizable title describing the diagnostic
            title: new LocalizableResourceString(nameof(UnityUnusedFlagResources.Title),
                UnityUnusedFlagResources.ResourceManager, typeof(UnityUnusedFlagResources)),
            // a localizable format message string, which can be passed as the first argument to System.String.Format(System.String, System.Object[]) when creating the diagnostic message with this descriptor
            messageFormat: new LocalizableResourceString(nameof(UnityUnusedFlagResources.MessageFormat),
                UnityUnusedFlagResources.ResourceManager, typeof(UnityUnusedFlagResources)),
            // the category of the diagnostic (like Design, Naming etc.)
            category: DiagnosticCategories.Performance,
            // the default severity of the diagnostic
            defaultSeverity: DiagnosticSeverity.Warning,
            // the diagnostic is enabled by default
            isEnabledByDefault: true,
            // an optional longer localizable description for the diagnostic
            description: new LocalizableResourceString(nameof(UnityUnusedFlagResources.Description),
                UnityUnusedFlagResources.ResourceManager, typeof(UnityUnusedFlagResources)));


        /// A DiagnosticDescriptor describing the "string.Format" diagnostic.
        public static readonly DiagnosticDescriptor DoNotUseStringFormat = new DiagnosticDescriptor(
            // an unique identifier for the diagnostic
            id: DiagnosticIDs.DoNotUseStringFormat,
            // a short localizable title describing the diagnostic
            title: new LocalizableResourceString(nameof(DoNotUseStringFormatResources.Title),
                DoNotUseStringFormatResources.ResourceManager, typeof(DoNotUseStringFormatResources)),
            // a localizable format message string, which can be passed as the first argument to System.String.Format(System.String, System.Object[]) when creating the diagnostic message with this descriptor
            messageFormat: new LocalizableResourceString(nameof(DoNotUseStringFormatResources.MessageFormat),
                DoNotUseStringFormatResources.ResourceManager, typeof(DoNotUseStringFormatResources)),
            // the category of the diagnostic (like Design, Naming etc.)
            category: DiagnosticCategories.Performance,
            // the default severity of the diagnostic
            defaultSeverity: DiagnosticSeverity.Warning,
            // the diagnostic is enabled by default
            isEnabledByDefault: true,
            // an optional longer localizable description for the diagnostic
            description: new LocalizableResourceString(nameof(DoNotUseStringFormatResources.Description),
                DoNotUseStringFormatResources.ResourceManager, typeof(DoNotUseStringFormatResources)));


        /// A DiagnosticDescriptor describing the "unnecessory string.Format calls" diagnostic.
        public static readonly DiagnosticDescriptor UseStringCatenatingInsteadOfFormat = new DiagnosticDescriptor(
            // an unique identifier for the diagnostic
            id: DiagnosticIDs.UseStringCatenatingInsteadOfFormat,
            // a short localizable title describing the diagnostic
            title: new LocalizableResourceString(nameof(UseStringCatenatingInsteadOfFormatResources.Title),
                UseStringCatenatingInsteadOfFormatResources.ResourceManager,
                typeof(UseStringCatenatingInsteadOfFormatResources)),
            // a localizable format message string, which can be passed as the first argument to System.String.Format(System.String, System.Object[]) when creating the diagnostic message with this descriptor
            messageFormat: new LocalizableResourceString(
                nameof(UseStringCatenatingInsteadOfFormatResources.MessageFormat),
                UseStringCatenatingInsteadOfFormatResources.ResourceManager,
                typeof(UseStringCatenatingInsteadOfFormatResources)),
            // the category of the diagnostic (like Design, Naming etc.)
            category: DiagnosticCategories.Performance,
            // the default severity of the diagnostic
            defaultSeverity: DiagnosticSeverity.Warning,
            // the diagnostic is enabled by default
            isEnabledByDefault: true,
            // an optional longer localizable description for the diagnostic
            description: new LocalizableResourceString(nameof(UseStringCatenatingInsteadOfFormatResources.Description),
                UseStringCatenatingInsteadOfFormatResources.ResourceManager,
                typeof(UseStringCatenatingInsteadOfFormatResources)));


        /// A DiagnosticDescriptor describing the "params" diagnostic.
        public static readonly DiagnosticDescriptor DoNotUseParams = new DiagnosticDescriptor(
            // an unique identifier for the diagnostic
            id: DiagnosticIDs.DoNotUseParams,
            // a short localizable title describing the diagnostic
            title: new LocalizableResourceString(nameof(DoNotUseParamsResources.Title),
                DoNotUseParamsResources.ResourceManager, typeof(DoNotUseParamsResources)),
            // a localizable format message string, which can be passed as the first argument to System.String.Format(System.String, System.Object[]) when creating the diagnostic message with this descriptor
            messageFormat: new LocalizableResourceString(nameof(DoNotUseParamsResources.MessageFormat),
                DoNotUseParamsResources.ResourceManager, typeof(DoNotUseParamsResources)),
            // the category of the diagnostic (like Design, Naming etc.)
            category: DiagnosticCategories.Performance,
            // the default severity of the diagnostic
            defaultSeverity: DiagnosticSeverity.Warning,
            // the diagnostic is enabled by default
            isEnabledByDefault: true,
            // an optional longer localizable description for the diagnostic
            description: new LocalizableResourceString(nameof(DoNotUseParamsResources.Description),
                DoNotUseParamsResources.ResourceManager, typeof(DoNotUseParamsResources)));


        /// A DiagnosticDescriptor describing the "GameObjects in destructor" diagnostic.
        public static readonly DiagnosticDescriptor GameObjectInDestructor = new DiagnosticDescriptor(
            // an unique identifier for the diagnostic
            id: DiagnosticIDs.GameObjectInDestructor,
            // a short localizable title describing the diagnostic
            title: new LocalizableResourceString(nameof(GameObjectInDestructorResources.Title),
                GameObjectInDestructorResources.ResourceManager, typeof(GameObjectInDestructorResources)),
            // a localizable format message string, which can be passed as the first argument to System.String.Format(System.String, System.Object[]) when creating the diagnostic message with this descriptor
            messageFormat: new LocalizableResourceString(nameof(GameObjectInDestructorResources.MessageFormat),
                GameObjectInDestructorResources.ResourceManager, typeof(GameObjectInDestructorResources)),
            // the category of the diagnostic (like Design, Naming etc.)
            category: DiagnosticCategories.Performance,
            // the default severity of the diagnostic
            defaultSeverity: DiagnosticSeverity.Warning,
            // the diagnostic is enabled by default
            isEnabledByDefault: true,
            // an optional longer localizable description for the diagnostic
            description: new LocalizableResourceString(nameof(GameObjectInDestructorResources.Description),
                GameObjectInDestructorResources.ResourceManager, typeof(GameObjectInDestructorResources)));

        /// A DiagnosticDescriptor describing the "PrivateFieldNeverUsedAnalyzer" diagnostic.
        public static readonly DiagnosticDescriptor PrivateFieldNeverUsed = new DiagnosticDescriptor(
            // an unique identifier for the diagnostic
            id: DiagnosticIDs.PrivateFieldNeverUsed,
            // a short localizable title describing the diagnostic
            title: new LocalizableResourceString(nameof(PrivateFieldNeverUsedResources.Title),
                PrivateFieldNeverUsedResources.ResourceManager, typeof(PrivateFieldNeverUsedResources)),
            // a localizable format message string, which can be passed as the first argument to System.String.Format(System.String, System.Object[]) when creating the diagnostic message with this descriptor
            messageFormat: new LocalizableResourceString(nameof(PrivateFieldNeverUsedResources.MessageFormat),
                PrivateFieldNeverUsedResources.ResourceManager, typeof(PrivateFieldNeverUsedResources)),
            // the category of the diagnostic (like Design, Naming etc.)
            category: DiagnosticCategories.Variable,
            // the default severity of the diagnostic
            defaultSeverity: DiagnosticSeverity.Warning,
            // the diagnostic is enabled by default
            isEnabledByDefault: true,
            // an optional longer localizable description for the diagnostic
            description: new LocalizableResourceString(nameof(PrivateFieldNeverUsedResources.Description),
                PrivateFieldNeverUsedResources.ResourceManager, typeof(PrivateFieldNeverUsedResources)));

        /// A DiagnosticDescriptor describing the "PreprocessorDirectiveUnityEditorAndDebug" diagnostic.
        public static readonly DiagnosticDescriptor PreprocessorDirectiveUnityEditorAndDebug = new DiagnosticDescriptor(
            // an unique identifier for the diagnostic
            id: DiagnosticIDs.PreprocessorDirectiveUnityEditorAndDebug,
            // a short localizable title describing the diagnostic
            title: new LocalizableResourceString(nameof(PreprocessorDirectiveUnityEditorAndDebugResources.Title),
                PreprocessorDirectiveUnityEditorAndDebugResources.ResourceManager, typeof(PreprocessorDirectiveUnityEditorAndDebugResources)),
            // a localizable format message string, which can be passed as the first argument to System.String.Format(System.String, System.Object[]) when creating the diagnostic message with this descriptor
            messageFormat: new LocalizableResourceString(nameof(PreprocessorDirectiveUnityEditorAndDebugResources.MessageFormat),
                PreprocessorDirectiveUnityEditorAndDebugResources.ResourceManager, typeof(PreprocessorDirectiveUnityEditorAndDebugResources)),
            // the category of the diagnostic (like Design, Naming etc.)
            category: DiagnosticCategories.Preprocessor,
            // the default severity of the diagnostic
            defaultSeverity: DiagnosticSeverity.Warning,
            // the diagnostic is enabled by default
            isEnabledByDefault: true,
            // an optional longer localizable description for the diagnostic
            description: new LocalizableResourceString(nameof(PreprocessorDirectiveUnityEditorAndDebugResources.Description),
                PreprocessorDirectiveUnityEditorAndDebugResources.ResourceManager, typeof(PreprocessorDirectiveUnityEditorAndDebugResources)));

        /// A DiagnosticDescriptor describing the "UsingUnityEditor" diagnostic.
        public static readonly DiagnosticDescriptor UsingUnityEditor = new DiagnosticDescriptor(
            // an unique identifier for the diagnostic
            id: DiagnosticIDs.UsingUnityEditor,
            // a short localizable title describing the diagnostic
            title: new LocalizableResourceString(nameof(UsingUnityEditorResources.Title),
                UsingUnityEditorResources.ResourceManager, typeof(UsingUnityEditorResources)),
            // a localizable format message string, which can be passed as the first argument to System.String.Format(System.String, System.Object[]) when creating the diagnostic message with this descriptor
            messageFormat: new LocalizableResourceString(nameof(UsingUnityEditorResources.MessageFormat),
                UsingUnityEditorResources.ResourceManager, typeof(UsingUnityEditorResources)),
            // the category of the diagnostic (like Design, Naming etc.)
            category: DiagnosticCategories.Preprocessor,
            // the default severity of the diagnostic
            defaultSeverity: DiagnosticSeverity.Warning,
            // the diagnostic is enabled by default
            isEnabledByDefault: true,
            // an optional longer localizable description for the diagnostic
            description: new LocalizableResourceString(nameof(UsingUnityEditorResources.Description),
                UsingUnityEditorResources.ResourceManager, typeof(UsingUnityEditorResources)));

        /// A DiagnosticDescriptor describing the "LocalFieldNeverUsed" diagnostic.
        public static readonly DiagnosticDescriptor LocalFieldNeverUsed = new DiagnosticDescriptor(
            // an unique identifier for the diagnostic
            id: DiagnosticIDs.LocalFieldNeverUsed,
            // a short localizable title describing the diagnostic
            title: new LocalizableResourceString(nameof(LocalFieldNeverUsedResources.Title),
                LocalFieldNeverUsedResources.ResourceManager, typeof(LocalFieldNeverUsedResources)),
            // a localizable format message string, which can be passed as the first argument to System.String.Format(System.String, System.Object[]) when creating the diagnostic message with this descriptor
            messageFormat: new LocalizableResourceString(nameof(LocalFieldNeverUsedResources.MessageFormat),
                LocalFieldNeverUsedResources.ResourceManager, typeof(LocalFieldNeverUsedResources)),
            // the category of the diagnostic (like Design, Naming etc.)
            category: DiagnosticCategories.Preprocessor,
            // the default severity of the diagnostic
            defaultSeverity: DiagnosticSeverity.Warning,
            // the diagnostic is enabled by default
            isEnabledByDefault: true,
            // an optional longer localizable description for the diagnostic
            description: new LocalizableResourceString(nameof(LocalFieldNeverUsedResources.Description),
                LocalFieldNeverUsedResources.ResourceManager, typeof(LocalFieldNeverUsedResources)));

        /// A DiagnosticDescriptor describing the "Empty MonoBehaviour method" diagnostic.
        public static readonly DiagnosticDescriptor BeyondRowLimitMonoBehaviourMethod = new DiagnosticDescriptor(
            // an unique identifier for the diagnostic
            id: DiagnosticIDs.BeyondRowLimitMonoBehaviourMethod,
            // a short localizable title describing the diagnostic
            title: new LocalizableResourceString(nameof(BeyondRowLimitMonoBehaviourMethodsResources.Title),
                BeyondRowLimitMonoBehaviourMethodsResources.ResourceManager, typeof(BeyondRowLimitMonoBehaviourMethodsResources)),
            // a localizable format message string, which can be passed as the first argument to System.String.Format(System.String, System.Object[]) when creating the diagnostic message with this descriptor
            messageFormat: new LocalizableResourceString(nameof(BeyondRowLimitMonoBehaviourMethodsResources.MessageFormat),
                BeyondRowLimitMonoBehaviourMethodsResources.ResourceManager, typeof(BeyondRowLimitMonoBehaviourMethodsResources)),
            // the category of the diagnostic (like Design, Naming etc.)
            category: DiagnosticCategories.Performance,
            // the default severity of the diagnostic
            defaultSeverity: DiagnosticSeverity.Error,
            // the diagnostic is enabled by default
            isEnabledByDefault: true,
            // an optional longer localizable description for the diagnostic
            description: new LocalizableResourceString(nameof(BeyondRowLimitMonoBehaviourMethodsResources.Description),
                BeyondRowLimitMonoBehaviourMethodsResources.ResourceManager, typeof(BeyondRowLimitMonoBehaviourMethodsResources)));

        /// A DiagnosticDescriptor describing the "SendMessage, SendMessageUpwards, BroadcastMessage" diagnostic.
        public static readonly DiagnosticDescriptor DoNotUseConstantStringMethodsFromConfig = new DiagnosticDescriptor(
            // an unique identifier for the diagnostic
            id: DiagnosticIDs.DoNotUseConstantStringMethodsFromConfig,
            // a short localizable title describing the diagnostic
            title: new LocalizableResourceString(nameof(DoNotUseConstantStringMethodsFromConfigResources.Title),
                DoNotUseConstantStringMethodsFromConfigResources.ResourceManager, typeof(DoNotUseConstantStringMethodsFromConfigResources)),
            // a localizable format message string, which can be passed as the first argument to System.String.Format(System.String, System.Object[]) when creating the diagnostic message with this descriptor
            messageFormat: new LocalizableResourceString(nameof(DoNotUseConstantStringMethodsFromConfigResources.MessageFormat),
                DoNotUseConstantStringMethodsFromConfigResources.ResourceManager, typeof(DoNotUseConstantStringMethodsFromConfigResources)),
            // the category of the diagnostic (like Design, Naming etc.)
            category: DiagnosticCategories.StringMethods,
            // the default severity of the diagnostic
            defaultSeverity: DiagnosticSeverity.Error,
            // the diagnostic is enabled by default
            isEnabledByDefault: true,
            // an optional longer localizable description for the diagnostic
            description: new LocalizableResourceString(nameof(DoNotUseConstantStringMethodsFromConfigResources.Description),
                DoNotUseConstantStringMethodsFromConfigResources.ResourceManager, typeof(DoNotUseConstantStringMethodsFromConfigResources)));

        /// A DiagnosticDescriptor describing the "SendMessage, SendMessageUpwards, BroadcastMessage" diagnostic.
        public static readonly DiagnosticDescriptor ForbiddenMethods = new DiagnosticDescriptor(
            // an unique identifier for the diagnostic
            id: DiagnosticIDs.ForbiddenMethods,
            // a short localizable title describing the diagnostic
            title: new LocalizableResourceString(nameof(ForbiddenMethodsResources.Title),
                ForbiddenMethodsResources.ResourceManager, typeof(ForbiddenMethodsResources)),
            // a localizable format message string, which can be passed as the first argument to System.String.Format(System.String, System.Object[]) when creating the diagnostic message with this descriptor
            messageFormat: new LocalizableResourceString(nameof(ForbiddenMethodsResources.MessageFormat),
                ForbiddenMethodsResources.ResourceManager, typeof(ForbiddenMethodsResources)),
            // the category of the diagnostic (like Design, Naming etc.)
            category: DiagnosticCategories.StringMethods,
            // the default severity of the diagnostic
            defaultSeverity: DiagnosticSeverity.Error,
            // the diagnostic is enabled by default
            isEnabledByDefault: true,
            // an optional longer localizable description for the diagnostic
            description: new LocalizableResourceString(nameof(ForbiddenMethodsResources.Description),
                ForbiddenMethodsResources.ResourceManager, typeof(ForbiddenMethodsResources)));

        /// A DiagnosticDescriptor describing the "UsingUnityEditor" diagnostic.
        public static readonly DiagnosticDescriptor ContainLoopMonoBehaviourMethods = new DiagnosticDescriptor(
            // an unique identifier for the diagnostic
            id: DiagnosticIDs.ContainLoopMonoBehaviourMethods,
            // a short localizable title describing the diagnostic
            title: new LocalizableResourceString(nameof(ContainLoopMonoBehaviourMethodsResources.Title),
                ContainLoopMonoBehaviourMethodsResources.ResourceManager, typeof(ContainLoopMonoBehaviourMethodsResources)),
            // a localizable format message string, which can be passed as the first argument to System.String.Format(System.String, System.Object[]) when creating the diagnostic message with this descriptor
            messageFormat: new LocalizableResourceString(nameof(ContainLoopMonoBehaviourMethodsResources.MessageFormat),
                ContainLoopMonoBehaviourMethodsResources.ResourceManager, typeof(ContainLoopMonoBehaviourMethodsResources)),
            // the category of the diagnostic (like Design, Naming etc.)
            category: DiagnosticCategories.Performance,
            // the default severity of the diagnostic
            defaultSeverity: DiagnosticSeverity.Warning,
            // the diagnostic is enabled by default
            isEnabledByDefault: true,
            // an optional longer localizable description for the diagnostic
            description: new LocalizableResourceString(nameof(ContainLoopMonoBehaviourMethodsResources.Description),
                ContainLoopMonoBehaviourMethodsResources.ResourceManager, typeof(ContainLoopMonoBehaviourMethodsResources)));

    }
}