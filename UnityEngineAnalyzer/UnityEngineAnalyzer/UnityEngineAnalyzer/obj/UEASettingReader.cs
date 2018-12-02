using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnityEngineAnalyzer
{
    class UEASettingReader
    {
        internal class UEASettings
        {
            public List<string> ignoreFoldeRegexStrings { get; set; }
            public List<string> prohibitedFunctionRegexStrings { get; set; }
            public List<string> noConstansFunctionRegexStrings { get; set; }
            public int startFunctinMAXRows { get; set; }
            public int awakeFunctinMAXRows { get; set; }
        }
        private static string configFilePath = "C:/Users/honghao/Desktop/CodeAnalysis/UnityEngineAnalyzer/UnityEngineAnalyzer";
        public static UEASettings uEASettings = new UEASettings();
        public void ReadConfigFile()
        {
            System.IO.StreamReader
        }
    }
}
