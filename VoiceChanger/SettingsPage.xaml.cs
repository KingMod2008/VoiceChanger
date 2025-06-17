using NAudio.Wave;
using System.Windows.Controls;

namespace KingVoiceChanger
{
    public partial class SettingsPage : Page
    {
        private MainWindow mainWindow;

        public SettingsPage(MainWindow mainWindow)
        {
            InitializeComponent();
            this.mainWindow = mainWindow;
        }

        public void LoadDeviceLists()
        {
            InputDeviceComboBox.Items.Clear();
            for (int i = 0; i < WaveInEvent.DeviceCount; i++)
            {
                InputDeviceComboBox.Items.Add(WaveInEvent.GetCapabilities(i).ProductName);
            }
            if (InputDeviceComboBox.Items.Count > 0)
            {
                InputDeviceComboBox.SelectedIndex = 0;
            }

            OutputDeviceComboBox.Items.Clear();
            for (int i = 0; i < WaveOut.DeviceCount; i++)
            {
                OutputDeviceComboBox.Items.Add(WaveOut.GetCapabilities(i).ProductName);
            }
            if (OutputDeviceComboBox.Items.Count > 0)
            {
                OutputDeviceComboBox.SelectedIndex = 0;
            }
        }

        public int GetSelectedInputDevice()
        {
            return InputDeviceComboBox.SelectedIndex;
        }

        public int GetSelectedOutputDevice()
        {
            return OutputDeviceComboBox.SelectedIndex;
        }
    }
}
