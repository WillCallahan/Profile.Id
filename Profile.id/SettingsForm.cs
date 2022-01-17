using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Profile.id
{
    public partial class SettingsForm : Form
    {
        private List<ProfileSettings> _profileSettingsList;

        private readonly Action<List<ProfileSettings>> _updatedSettingsAction;

        private const string ColorSampleComboNamePrefix = "colorSample";

        public SettingsForm(Action<List<ProfileSettings>> updatedSettingsAction)
        {
            InitializeComponent();
            _updatedSettingsAction = updatedSettingsAction;
            LoadSettings();
            InitializeInputFields();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
        }

        private void LoadSettings()
        {
            var persistedProfileSettings = ProfileSettingsRepository
                .GetProfileSettings()
                .ToDictionary(p => p.ProfileName);
            var awsProfiles = ProfileReader.GetAllProfiles();

            _profileSettingsList = awsProfiles
                .Where(a => !persistedProfileSettings.ContainsKey(a))
                .Select(a => new ProfileSettings(a, Color.White))
                .Concat(persistedProfileSettings.Values.ToList())
                .ToList();
        }

        private void InitializeInputFields()
        {
            SuspendLayout();
            panel1.SuspendLayout();
            for (var i = 0; i < _profileSettingsList.Count; i ++)
            {
                var (textField, comboBox, colorSample) = GenerateFieldsForProfileSettings(_profileSettingsList[i], i);
                panel1.Controls.Add(comboBox);
                panel1.Controls.Add(textField);
                panel1.Controls.Add(colorSample);
            }
            panel1.ResumeLayout(false);
            panel1.PerformLayout();
            ResumeLayout(false);
        }

        private static List<string> GetColorOptions()
        {
            return Enum.GetNames(typeof(KnownColor))
                .Where(c => c != IconStylizer.TransparentColor.ToString())
                .ToList();
        }

        private (TextBox, ComboBox, Label) GenerateFieldsForProfileSettings(ProfileSettings profileSettings, int index)
        {
            var yCoordinate = index * 30;
            var tabIndexOffset = index + 5;

            var textBox = new TextBox
            {
                Name = $"profileName{index}",
                Text = profileSettings.ProfileName,
                Location = new Point(0, yCoordinate),
                Visible = true,
                TabIndex = tabIndexOffset * 1,
                Size = new Size(200, 15)
            };

            var options = GetColorOptions();
            var comboBox = new ComboBox
            {
                Name = $"colorPicker{index}",
                Location = new Point(215, yCoordinate),
                Visible = true,
                TabIndex = tabIndexOffset * 2,
                Size = new Size(100, 15)
            };

            foreach (var option in options)
            {
                comboBox.Items.Add(option);
            }

            comboBox.SelectedIndex = options.IndexOf(profileSettings.Color.ToKnownColor().ToString());
            comboBox.SelectedIndexChanged += OnColorPickerChange;

            var colorSample = new Label
            {
                Name = $"{ColorSampleComboNamePrefix}{index}",
                Text = "",
                Location = new Point(330, yCoordinate + 2),
                Visible = true,
                TabIndex = 0,
                Size = new Size(15, 15),
                BackColor = profileSettings.Color
            };

            return (textBox, comboBox, colorSample);
        }

        private void OnColorPickerChange(object sender, EventArgs e)
        {
            if (sender is ComboBox comboBox)
            {
                var options = GetColorOptions();
                var selectedColor = Color.FromName(options[comboBox.SelectedIndex]);

                var profileSettingsIndex = int.Parse(comboBox.Name.Substring(ColorSampleComboNamePrefix.Length, comboBox.Name.Length - ColorSampleComboNamePrefix.Length));

                var colorSampleLabel = panel1.Controls
                    .Find($"{ColorSampleComboNamePrefix}{profileSettingsIndex}", false)
                    .Single();

                _profileSettingsList[profileSettingsIndex].Color = selectedColor;
                colorSampleLabel.BackColor = selectedColor;
            }
        }

        private void CancelDialogButton_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void SaveButton_Click(object sender, EventArgs e)
        {
            ProfileSettingsRepository.SaveProfileSettings(_profileSettingsList);
            _updatedSettingsAction?.Invoke(_profileSettingsList);
            Close();
        }
    }
}
