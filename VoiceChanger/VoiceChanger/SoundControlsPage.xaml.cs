using System.Windows;
using System.Windows.Controls;

namespace KingVoiceChanger
{
    public partial class SoundControlsPage : Page
    {
        private readonly MainWindow mainWindow;

        public SoundControlsPage(MainWindow mainWindow)
        {
            InitializeComponent();
            this.mainWindow = mainWindow;
            SetupEventHandlers();
            UpdateAllLabels();
        }

        private void SetupEventHandlers()
        {
            GainSlider.ValueChanged += (s, e) => { 
                mainWindow.Gain = (float)e.NewValue;
                GainValueLabel.Content = $"{e.NewValue:P0}";
                mainWindow.UpdateEffect();
            };
            MicBoostSlider.ValueChanged += (s, e) => {
                mainWindow.MicBoost = (float)e.NewValue;
                MicBoostValueLabel.Content = $"{e.NewValue:P0}";
                mainWindow.UpdateEffect();
            };
            EchoDelaySlider.ValueChanged += (s, e) => {
                mainWindow.EchoDelay = (int)e.NewValue;
                EchoDelayValueLabel.Content = $"{e.NewValue:F0}ms";
                mainWindow.UpdateEffect();
            };
            EchoFeedbackSlider.ValueChanged += (s, e) => {
                mainWindow.EchoFeedback = (float)e.NewValue;
                EchoFeedbackValueLabel.Content = $"{e.NewValue:P0}";
                mainWindow.UpdateEffect();
            };

            // SelectVoiceEffectButton.Click handler removed because ShowVoiceSelection no longer exists.
    // TODO: Add new handler if needed.
        }

        public void SetSelectedVoiceEffectName(string name)
        {
            SelectVoiceEffectButton.Content = name;
        }

        public void SetEchoControlsVisibility(bool isVisible)
        {
            EchoControlsPanel.Visibility = isVisible ? Visibility.Visible : Visibility.Collapsed;
        }

        private void UpdateAllLabels()
        {
            GainValueLabel.Content = $"{GainSlider.Value:P0}";
            MicBoostValueLabel.Content = $"{MicBoostSlider.Value:P0}";
            EchoDelayValueLabel.Content = $"{EchoDelaySlider.Value:F0}ms";
            EchoFeedbackValueLabel.Content = $"{EchoFeedbackSlider.Value:P0}";

        }
    }
}
