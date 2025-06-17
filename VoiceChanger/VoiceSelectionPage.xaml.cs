using System;
using System.Collections.Generic;
using System.Windows.Controls;

namespace KingVoiceChanger
{
    public partial class VoiceSelectionPage : Page
    {
        public event Action<VoiceEffect>? EffectSelected;

        public VoiceSelectionPage(List<VoiceEffect> effects)
        {
            InitializeComponent();
            VoiceEffectsListBox.ItemsSource = effects;
        }

        private void VoiceEffectsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (VoiceEffectsListBox.SelectedItem is VoiceEffect selectedEffect)
            {
                EffectSelected?.Invoke(selectedEffect);
            }
        }
    }
}