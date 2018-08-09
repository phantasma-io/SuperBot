using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using MySql.Data.MySqlClient;
using System.Text;
using System.Net;

namespace DataLayer
{   
     public static class FileManager
    {
        public enum Location{
            CommsSettings,
            Triggers,
            Reactions,
            UserVariablesList,  //declaration of the existing user variables
        };

        private const string csvStorePath = "/DataLayer/IO/CSVStorage/";
        private const string jsonStorePath = "/DataLayer/IO/JsonStorage/";
        private const string genericStorePath = "/DataLayer/IO/FileStorage/";

        private static string GetJsonPath(Location filekey) => $"{Environment.CurrentDirectory}{jsonStorePath}{pathSuffix[filekey]}";
        private static string GetCsvStorePath(string filename) => $"{Environment.CurrentDirectory}{csvStorePath}{filename}.csv";

        private static Dictionary<Location, string> pathSuffix = new Dictionary<Location, string>{
            {Location.CommsSettings, "CommsLayerSettings.json"},
            {Location.Triggers, "Triggers.json"},
            {Location.Reactions, "Reactions.json"},
            {Location.UserVariablesList, "UserVariablesList.json"},
        };

        public static dynamic ReadJsonToDynamic(Location fileKey)
        {
            var filename = GetJsonPath(fileKey);
            
            //JsonTextReader reader = new JsonTextReader(new StreamReader(filename));
            dynamic obj = JsonConvert.DeserializeObject(File.ReadAllText(filename));

            return obj;
        }

        public static string ReadJsonToString(Location fileKey)
        {
            var filename = GetJsonPath(fileKey);
            
            var str = File.ReadAllText(filename);
            str = str.Replace("\\\"","");

            return str;
            //return System.Text.RegularExpressions.Regex.Unescape(str);
        }

        public static void SaveToCsv(string filename, string csv)
        {
            File.WriteAllText(GetCsvStorePath(filename), csv);
        }

        public static void SaveFile(string data, string filename, string subfolder = "")
        {
            var path = "";
            
            if(subfolder == "")
                path = $"{Environment.CurrentDirectory}{genericStorePath}";
            else
                path = $"{Environment.CurrentDirectory}{genericStorePath}/{subfolder}";
            
            if(!File.Exists(path))
                System.IO.Directory.CreateDirectory(path);

            File.WriteAllText($"{path}/{filename}", data);
        }

        public static void DownloadAndSaveFile(string url, string filename, string subfolder = "")
        {
            var path = "";
            
            if(subfolder == "")
                path = $"{Environment.CurrentDirectory}{genericStorePath}";
            else
                path = $"{Environment.CurrentDirectory}{genericStorePath}/{subfolder}";
            
            if(!File.Exists(path))
                System.IO.Directory.CreateDirectory(path);

            using (WebClient client = new WebClient())
            {
                client.DownloadFile(new Uri(url), $"{path}/{filename}");
            }
        }

        public static void SaveFileBinary(string data, string filename, string subfolder = "")
        {
            var path = "";
            
            if(subfolder == "")
                path = $"{Environment.CurrentDirectory}{genericStorePath}";
            else
                path = $"{Environment.CurrentDirectory}{genericStorePath}/{subfolder}";
            
            if(!File.Exists(path))
                System.IO.Directory.CreateDirectory(path);

            File.WriteAllBytes($"{path}/{filename}", Encoding.ASCII.GetBytes(data));
        }
    }
}