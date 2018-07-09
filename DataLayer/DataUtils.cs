using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace DataLayer
{
    public static class DataUtils
    {
        public enum Files{
            CommsSettings,
            Triggers,
            Reactions,
            UserVariables,
        };

        private const string dataStorePath = "/DataLayer/DataStore/";

        private static string GetFilePath(Files filekey) => $"{Environment.CurrentDirectory}{dataStorePath}{filenames[filekey]}";

        private static Dictionary<Files, string> filenames = new Dictionary<Files, string>{
            {Files.CommsSettings, "CommsLayerSettings.json"},
            {Files.Triggers, "Triggers.json"},
            {Files.Reactions, "Reactions.json"},
            {Files.UserVariables, "UserVariables.json"}
        };

        public static dynamic ReadJsonToDynamic(Files fileKey)
        {
            var filename = GetFilePath(fileKey);
            
            //JsonTextReader reader = new JsonTextReader(new StreamReader(filename));
            dynamic obj = JsonConvert.DeserializeObject(File.ReadAllText(filename));

            return obj;
        }

        public static string ReadJsonToString(Files fileKey)
        {
            var filename = GetFilePath(fileKey);
            
            var str = File.ReadAllText(filename);
            str = str.Replace("\\\"","");

            return str;
            //return System.Text.RegularExpressions.Regex.Unescape(str);
        }

        public static void WriteJsonToFile(Files file, object json)
        {
            
        }

    }
}