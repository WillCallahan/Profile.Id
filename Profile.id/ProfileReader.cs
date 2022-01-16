using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Profile.id
{
    public static class ProfileReader
    {
        public const string DefaultCredentialsFilePath = ".aws\\credentials";

        private static string GetDefaultCredentialsFilePath()
        {
            var path = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            return $"{path}\\{DefaultCredentialsFilePath}";
        }

        public static List<string> GetAllProfiles(string credentialsFilePath = null)
        {
            if (credentialsFilePath == null)
            {
                credentialsFilePath = GetDefaultCredentialsFilePath();
            }
            var profiles = new List<string>();
            var lines = File.ReadAllLines(credentialsFilePath);
            foreach (var line in lines)
            {
                var cleanLine = line.Trim();
                if (cleanLine.StartsWith("["))
                {
                    var profileName = cleanLine.Substring(1, cleanLine.Length - 2);
                    profiles.Add(profileName);
                }
            }

            return profiles;
        }
    }
}
