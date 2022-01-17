using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Profile.id.Properties;

namespace Profile.id
{
    public class TrayIconController : ApplicationContext
    {
        private readonly NotifyIcon _trayIcon;

        private List<Task> _tasks = new List<Task>();

        public const string AwsProfileEnvironmentVariable = "AWS_PROFILE";

        private List<ProfileSettings> _profileSettings;

        private SettingsForm _settingsFormForm;

        public TrayIconController()
        {
            _profileSettings = ProfileSettingsRepository.GetProfileSettings(ProfileSettingsRepository.DefaultSettingsLocation);
            _trayIcon = new NotifyIcon();

            UpdateTrayIcon();

            _trayIcon.MouseMove += MouseHover;
            _trayIcon.MouseDown += MouseClick;
        }

        public void Exit(object sender, EventArgs e)
        {
            _trayIcon.Visible = false;
            Task.WaitAll(_tasks.ToArray());
            Application.Exit();
        }

        public void OpenSettings(object sender, EventArgs e)
        {
            _settingsFormForm = new SettingsForm(OnChangedProfileSettings);
            _settingsFormForm.Show();
        }

        public void RefreshAll(object sender, EventArgs e)
        {
            _profileSettings = ProfileSettingsRepository.GetProfileSettings(ProfileSettingsRepository.DefaultSettingsLocation);
            UpdateTrayIcon();
        }

        public void ChangeProfile(object sender, EventArgs e)
        {
            if (sender is MenuItem menuItem)
            {
                var newProfileName = menuItem.Text;
                var task = Task.Run(() =>
                {
                    Environment.SetEnvironmentVariable(AwsProfileEnvironmentVariable, newProfileName, EnvironmentVariableTarget.User);
                    UpdateTrayIcon();
                });
                _tasks.Add(task);
            }
        }

        public void MouseHover(object sender, MouseEventArgs e)
        {
            var (target, profileName) = GetActiveProfile();
            var tooltip = GetActiveProfileTooltip(target, profileName);
            if (_trayIcon.BalloonTipText == tooltip || _trayIcon.Text == tooltip)
            {
                return;
            }
            _trayIcon.BalloonTipText = tooltip;
            _trayIcon.Text = tooltip;
        }

        public void MouseClick(object sender, MouseEventArgs e)
        {
            var (_, profileName) = GetActiveProfile();
            foreach (MenuItem menuItem in _trayIcon.ContextMenu.MenuItems)
            {
                menuItem.Checked = profileName == menuItem.Text;
            }
        }

        private void OnChangedProfileSettings(List<ProfileSettings> profileSettingsList)
        {
            _profileSettings = profileSettingsList;
            var (_, profileName) = GetActiveProfile();
            var icon = GetMenuIcon(_profileSettings, profileName);
            _trayIcon.Icon = icon;
        }

        private void UpdateTrayIcon()
        {
            var (target, activeProfileName) = GetActiveProfile();
            var tooltip = GetActiveProfileTooltip(target, activeProfileName);

            var contextMenu = GetContextMenu(activeProfileName);
            var icon = GetMenuIcon(_profileSettings, activeProfileName);

            _trayIcon.ContextMenu = contextMenu;
            _trayIcon.Icon = icon;
            _trayIcon.BalloonTipText = tooltip;
            _trayIcon.Text = tooltip;
            _trayIcon.Visible = true;
        }

        private Icon GetMenuIcon(List<ProfileSettings> profileSettings, string activeProfileName)
        {
            foreach (var profileSetting in profileSettings)
            {
                if (profileSetting.ProfileName == activeProfileName)
                {
                    return IconStylizer.ChangeColor(Resources.AwsIcon, profileSetting.Color);
                }
            }

            return Resources.AwsIcon;
        }

        private ContextMenu GetContextMenu(string activeProfileName)
        {
            var profiles = ProfileReader.GetAllProfiles();

            var menuItems = new List<MenuItem>();
            var profileItems = profiles
                .Select(p =>
                {
                    var menuItem = new MenuItem(p, ChangeProfile)
                    {
                        Checked = activeProfileName == p
                    };
                    return menuItem;
                })
                .ToList();

            menuItems.AddRange(profileItems);

            if (profileItems.Count > 0)
            {
                menuItems.Add(new MenuItem("-"));
            }
            menuItems.Add(new MenuItem("Refresh Profiles", RefreshAll));
            menuItems.Add(new MenuItem("Settings", OpenSettings));
            menuItems.Add(new MenuItem("Exit", Exit));

            var contextMenu = new ContextMenu(menuItems.ToArray());

            return contextMenu;
        }

        private static string GetActiveProfileTooltip(EnvironmentVariableTarget? target, string profileName)
        {
            switch (target)
            {
                case EnvironmentVariableTarget.User:
                    return $"User: {profileName}";
                case EnvironmentVariableTarget.Machine:
                    return $"Machine: {profileName}";
                case EnvironmentVariableTarget.Process:
                    return $"Process: {profileName}";
                default:
                    return "No Active Profile";
            }
        }

        private static (EnvironmentVariableTarget?, string) GetActiveProfile()
        {
            var processProfileName = Environment.GetEnvironmentVariable(AwsProfileEnvironmentVariable, EnvironmentVariableTarget.Process);
            if (!string.IsNullOrEmpty(processProfileName))
            {
                return (EnvironmentVariableTarget.Process, processProfileName);
            }

            var userProfileName = Environment.GetEnvironmentVariable(AwsProfileEnvironmentVariable, EnvironmentVariableTarget.User);
            if (!string.IsNullOrEmpty(userProfileName))
            {
                return (EnvironmentVariableTarget.User, userProfileName);
            }

            var machineProfileName = Environment.GetEnvironmentVariable(AwsProfileEnvironmentVariable, EnvironmentVariableTarget.Machine);
            if (!string.IsNullOrEmpty(machineProfileName))
            {
                return (EnvironmentVariableTarget.Machine, machineProfileName);
            }

            return (null, null);
        }
    }
}