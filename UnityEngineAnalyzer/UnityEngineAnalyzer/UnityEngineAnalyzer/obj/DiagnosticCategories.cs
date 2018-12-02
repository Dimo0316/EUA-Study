namespace UnityEngineAnalyzer
{
    /// Categories of warning messages.
    static class DiagnosticCategories
    {
        public const string GC = "GC";
        public const string StringMethods = "String Methods";
        public const string Miscellaneous = "Miscellaneous";
        public const string Performance = "Performance";
        public const string Variable = "Variable";
        public const string Preprocessor = "Preprocessor";

        public const string AOT = "AOT";
    }
}