namespace UnityEngineAnalyzer
{
    /// unique IDs for all diagnostics.
    public static class DiagnosticIDs
    {
        public const string Common = "UEA0000";
        public const string DoNotUseOnGUI = "UEA0001";
        public const string DoNotUseStringMethods = "UEA0002";
        public const string EmptyMonoBehaviourMethod = "UEA0003";
        public const string UseCompareTag = "UEA0004";
        public const string DoNotUseFindMethodsInUpdate = "UEA0005";
        public const string DoNotUseCoroutines = "UEA0006";
        public const string DoNotUseForEachInUpdate = "UEA0007";
        public const string UnsealedDerivedClass = "UEA0008";
        public const string InvokeFunctionMissing = "UEA0009";
        public const string DoNotUseCameraMain = "UEA0010";
        public const string UseHashInsteadOfString = "UEA0011";
        public const string DoNotUseMultiDimensionalArray = "UEA0012";
        public const string DoNotUseVectorDistance = "UEA0013";
        public const string CalculationOrder = "UEA0014";
        public const string DoNotUseCameraRect = "UEA0015";
        public const string ReplaceOldGCMethods = "UEA0016";
        public const string DoNotInitContainerWithoutSize = "UEA0017";
        public const string DoNotUseEnumOrStructAsKey = "UEA0018";
        public const string DoNotYieldReturnZero = "UEA0019";
        public const string DoNotUseAddOpOnString = "UEA0020";
        public const string DoNotUseEqualOpOnString = "UEA0021";
        public const string DoNotUseRegex = "UEA0022";
        public const string DoNotUseMaterial = "UEA0023";
        public const string DoNotNewContainerInUpdate = "UEA0024";
        public const string DoNotUseLambda = "UEA0025";
        public const string DoNotReturnContainer = "UEA0026";
        public const string DoNotUseStringCompareMethods = "UEA0027";
        public const string DoNotUseLambdaInGenericMethod = "UEA0028";
        public const string Boxing = "UEA0029";
        public const string MethodToDelegate = "UEA0030";
        public const string DoNotUseDefaultDebug = "UEA0031";
        public const string CacheContainerAPI = "UEA0032";
        public const string TransformUpdate = "UEA0033";
        public const string ArrayList = "UEA0034";
        public const string ParticleApiWithChildren = "UEA0035";
        public const string RaycastMaxDistance = "UEA0036";
        public const string DoNotMoveRigidbodyByTransform = "UEA0037";
        public const string TextureNonReadable = "UEA0038";
        public const string Unmanaged = "UEA0039";
        public const string UnityUnusedFlag = "UEA0040";
        public const string DoNotUseStringFormat = "UEA0041";
        public const string UseStringCatenatingInsteadOfFormat = "UEA0042";
        public const string DoNotUseParams = "UEA0043";
        public const string GameObjectInDestructor = "UEA0044";
        public const string PrivateFieldNeverUsed = "UEA0045";
        public const string PreprocessorDirectiveUnityEditorAndDebug = "UEA0046";
        public const string UsingUnityEditor = "UEA0047";
        public const string LocalFieldNeverUsed = "UEA0048";
        public const string BeyondRowLimitMonoBehaviourMethod = "UEA0049";
        public const string DoNotUseConstantStringMethodsFromConfig = "UEA0050";
        public const string ForbiddenMethods = "UEA0051";
        public const string ContainLoopMonoBehaviourMethods = "UEA0052";
        


        //NOTES: These should probably be on their own analyzer - as they are not specific to Unity
        public const string DoNotUseRemoting = "AOT0001";
        public const string DoNotUseReflectionEmit = "AOT0002";
        public const string DoNotUseLinq = "AOT0003";
        public const string TypeGetType = "AOT0004";
    }
}