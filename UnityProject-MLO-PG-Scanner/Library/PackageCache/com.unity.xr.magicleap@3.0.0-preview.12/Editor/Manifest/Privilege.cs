using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

using UnityEngine;

namespace UnityEditor.XR.MagicLeap
{
    [Serializable]
    public class Privilege
    {
        public enum Category
        {
            Invalid,
            Sensitive,
            Reality,
            Autogranted,
            Trusted
        }
        public string name;
        public bool enabled;

        public override bool Equals(object obj)
        {
            var p2 = obj as Privilege;
            if (p2 == null)
                return false;
            return name == p2.name
                && enabled == p2.enabled;
        }

        public override int GetHashCode()
        {
            unchecked // Overflow is fine, just wrap
            {
                int hash = 17;
                // Suitable nullity checks etc, of course :)
                if (!string.IsNullOrEmpty(name))
                    hash = hash * 486187739 + name.GetHashCode();
                hash = hash * 486187739 + enabled.GetHashCode();
                return hash;
            }
        }
    }

    internal class PrivilegeDescriptor
    {
        public int? apiLevel { get; set; }
        public Privilege.Category category { get; set; }
        public string description { get; set; }
        public string name { get; set; }
    }

    internal static class PrivilegeParser
    {
        const string kPlatformLevelRegex = @"\#define ML_PLATFORM_API_LEVEL (\d+)";
        const string kPrivilegeIDRegex = @"\s+\/\*!\s\n\s+\<b\>Description:\</b\>(.+)\<br/\>\s+\n.+Type:.+(autogranted|reality|sensitive|trusted).+\n.+\n(?:\s+\\apilevel\s(\d+)\s+\n)?\s+\*/\s+\n\s+MLPrivilegeID_(\w+).+";

        public const string kPlatformHeaderPath = "include/ml_platform.h";
        public const string kPrivilegeHeaderPath = "include/ml_privilege_ids.h";


        //[MenuItem("Magic Leap/PrivilegeParserTest")]
        static void RunPrivilegeParser()
        {
            var path = Path.Combine(SDKUtility.sdkPath, kPrivilegeHeaderPath);
            Debug.LogFormat("path to header: {0}", path);
            foreach (var d in ParsePrivilegesFromHeader(path))
            {
                Debug.LogFormat("Privilege: {0}, Type: {1}", d.name, d.category);
            }
        }

        //[MenuItem("Magic Leap/PlatformParserTest")]
        static void RunPlatformParser()
        {
            var path = Path.Combine(SDKUtility.sdkPath, kPlatformHeaderPath);
            Debug.LogFormat("path to header: {0}", path);
            Debug.LogFormat("platform level: {0}", ParsePlatformLevelFromHeader(path));
        }

        public static int ParsePlatformLevelFromHeader(string header_path)
        {
            if (header_path == null && !File.Exists(header_path))
                throw new ArgumentException(string.Format("File '{0}' not found", header_path));
            using (var reader = new StreamReader(header_path))
            {
                var buffer = reader.ReadToEnd().Replace("\n", "\r\n");
                var regex = new Regex(kPlatformLevelRegex);
                var match = regex.Match(buffer);
                return int.Parse(match.Groups[1].Value);
            }
        }

        public static IEnumerable<PrivilegeDescriptor> ParsePrivilegesFromHeader(string header_path)
        {
            if (header_path == null && !File.Exists(header_path))
                throw new ArgumentException(string.Format("File '{0}' not found", header_path));
            using (var reader = new StreamReader(header_path))
            {
                var buffer = reader.ReadToEnd().Replace("\n", "\r\n");
                var regex = new Regex(kPrivilegeIDRegex);
                var matches = regex.Matches(buffer);
                foreach (Match match in matches)
                {
                    yield return new PrivilegeDescriptor {
                        apiLevel = ParseAPILevel(match.Groups[3].Value),
                        category = ParseCategory(match.Groups[2].Value),
                        description = match.Groups[1].Value,
                        name = match.Groups[4].Value
                    };
                }
            }
        }

        private static int? ParseAPILevel(string input)
        {
            if (string.IsNullOrEmpty(input))
                return null;
            int value = 0;
            return (int.TryParse(input, out value)) ? new Nullable<int>(value) : null;
        }

        private static Privilege.Category ParseCategory(string input)
        {
            if (input == "autogranted") return Privilege.Category.Autogranted;
            if (input == "reality") return Privilege.Category.Reality;
            if (input == "sensitive") return Privilege.Category.Sensitive;
            if (input == "trusted") return Privilege.Category.Trusted;
            return Privilege.Category.Invalid;
        }
    }
}