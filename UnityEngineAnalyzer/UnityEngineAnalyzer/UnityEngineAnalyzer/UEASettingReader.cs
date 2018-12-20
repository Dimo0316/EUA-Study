using System;
using System.Collections.Generic;
using System.IO;

namespace UnityEngineAnalyzer
{
    public class UEASettings
    {
        public List<string> ignoreFoldeRegexStrings { get; set; }
        public List<string> prohibitedFunctionRegexStrings { get; set; }
        public List<string> noConstansFunctionRegexStrings { get; set; }
        public int startAndAwakeFunctinMAXRows { get; set; }
    }

    public class UEASettingReader
    {
        static string sPath = Environment.GetEnvironmentVariable("LOCALAPPDATA");
        private static string configFilePath = sPath + @"\uea_config.txt";
        
        private static UEASettings uEASettings = new UEASettings();
        
        /// Read config file. If there is no file, generate default one.
        public UEASettings ReadConfigFile()
        {

            if (!File.Exists(configFilePath))
            {
                // generate default uea_config
                uEASettings.ignoreFoldeRegexStrings = new List<string>();
                uEASettings.prohibitedFunctionRegexStrings = new List<string>();
                uEASettings.noConstansFunctionRegexStrings = new List<string>();
                uEASettings.startAndAwakeFunctinMAXRows = int.MaxValue;

                return uEASettings;
            }

            string[] configs =  File.ReadAllLines(configFilePath);
            uEASettings.ignoreFoldeRegexStrings = new List<string>(configs[0].Split('|'));
            uEASettings.prohibitedFunctionRegexStrings = new List<string>(configs[1].Split('|'));
            uEASettings.noConstansFunctionRegexStrings = new List<string>(configs[2].Split('|'));
            uEASettings.startAndAwakeFunctinMAXRows = int.Parse(configs[3]);

            return uEASettings;
        }
    }
}
