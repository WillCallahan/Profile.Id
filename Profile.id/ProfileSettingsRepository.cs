using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Profile.id
{
    public class ProfileSettings
    {
        public string ProfileName { get; set; }

        public Color Color { get; set; }
    }

    public static class ProfileSettingsRepository
    {
        public static string DefaultSettingsLocation => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "ProfileId", "settings.csv");
        
        public static List<ProfileSettings> GetProfileSettings(string settingsLocation)
        {
            var profileSettingsList = new List<ProfileSettings>();

            var fileExists = File.Exists(settingsLocation);
            if (!fileExists)
            {
                return profileSettingsList;
            }

            var lines = File.ReadAllLines(settingsLocation);
            foreach (var line in lines)
            {
                var settings = line.Split(',');
                if (settings.Length < 2)
                {
                    throw new InvalidOperationException("Settings are invalid; each line must be the profile name followed by the color");
                }

                var profileSettings = new ProfileSettings
                {
                    ProfileName = settings[0],
                    Color = Color.FromName(settings[1])
                };

                profileSettingsList.Add(profileSettings);
            }

            return profileSettingsList;
        }

        public static void SaveProfileSettings(List<ProfileSettings> profileSettingsList, string settingsLocation)
        {
            var lines = profileSettingsList.Select(p => $"{p.ProfileName},{p.Color}");
            File.WriteAllLines(settingsLocation, lines);
        }
    }
}
