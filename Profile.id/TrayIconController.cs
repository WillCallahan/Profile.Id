using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
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

        public TrayIconController()
        {
            var (target, activeProfileName) = GetActiveProfile();
            var tooltip = GetActiveProfileTooltip(target, activeProfileName);

            var contextMenu = GetContextMenu(activeProfileName);
            

            _trayIcon = new NotifyIcon
            {
                Icon = Resources.AwsIcon,
                ContextMenu = contextMenu,
                BalloonTipText = tooltip,
                Text = tooltip,
                Visible = true
            };

            _trayIcon.MouseMove += MouseHover;
            _trayIcon.MouseDown += MouseClick;
        }

        public void Exit(object sender, EventArgs e)
        {
            _trayIcon.Visible = false;
            Task.WaitAll(_tasks.ToArray());
            Application.Exit();
        }

        public void ChangeProfile(object sender, EventArgs e)
        {
            if (sender is MenuItem menuItem)
            {
                var newProfileName = menuItem.Text;
                var task = Task.Run(() => Environment.SetEnvironmentVariable(AwsProfileEnvironmentVariable, newProfileName, EnvironmentVariableTarget.User));
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
            var (target, profileName) = GetActiveProfile();
            foreach (MenuItem menuItem in _trayIcon.ContextMenu.MenuItems)
            {
                menuItem.Checked = profileName == menuItem.Text;
            }
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
            menuItems.Add(new MenuItem("-"));
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