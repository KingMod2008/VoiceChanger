using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using NAudio.Dsp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using KingVoiceChanger;

namespace KingVoiceChanger
{
    public partial class MainWindow : Window
    {
        private WaveInEvent? waveIn;
        private WaveOutEvent? waveOut;
        private BufferedWaveProvider? bufferedWaveProvider;
        private ISampleProvider? finalProvider;
        private readonly List<VoiceEffect> voiceEffects;
        private VoiceEffect? selectedVoiceEffect;
        private bool isHearingMyself = false;
        private bool isEffectApplied = false;

        public float Gain { get; set; } = 1.0f;
        public float MicBoost { get; set; } = 1.0f;
        public float Pitch { get; set; } = 1.5f;
        public float AiBoost { get; set; } = 1.0f;
        public int EchoDelay { get; set; } = 200;
        public float EchoFeedback { get; set; } = 0.5f;

        public MainWindow()
        {
            InitializeComponent();
            voiceEffects = InitializeVoiceEffects();
            selectedVoiceEffect = voiceEffects.First();
            this.Loaded += MainWindow_Loaded;
        }

        private List<VoiceEffect> InitializeVoiceEffects()
        {
            return new List<VoiceEffect>
            {
                new VoiceEffect("Normal", "No effect", "/icons/normal.png", p => p),
                new VoiceEffect("Echo", "Repeating echo", "/icons/echo.png", p => new EchoEffect(p, delayMs: this.EchoDelay, feedback: this.EchoFeedback)),
                new VoiceEffect("Reverb", "Simulates a large room", "/icons/reverb.png", p => new ReverbEffect(p)),
                new VoiceEffect("Underwater", "Muffled sound like you're submerged", "/icons/underwater.png", p => new UnderwaterEffect(p)),
                new VoiceEffect("Robot", "Robotic, synthesized voice", "/icons/robot.png", p => new RobotEffect(p)),
                new VoiceEffect("Distortion", "Distorted, harsh sound", "/icons/distortion.png", p => new DistortionEffect(p)),
                new VoiceEffect("Vibrato", "Wavering pitch effect", "/icons/vibrato.png", p => new VibratoEffect(p)),
                new VoiceEffect("Monster", "Deep and menacing", "/icons/monster.png", p => new MonsterEffect(p)),
                new VoiceEffect("Whisper", "Breathy whisper", "/icons/whisper.png", p => new WhisperEffect(p)),
                new VoiceEffect("Telephone", "Narrow band phone sound", "/icons/telephone.png", p => new TelephoneEffect(p)),
                new VoiceEffect("Bass Boost", "Boosts low frequencies", "/icons/bass.png", p => new BassBoostEffect(p)),
                new VoiceEffect("Radio", "Old radio sound", "/icons/radio.png", p => new RadioEffect(p))
            };
        }

        private void Start(int inputDeviceIndex, int outputDeviceIndex)
        {
            Stop();
            try
            {
                waveIn = new WaveInEvent { DeviceNumber = inputDeviceIndex, WaveFormat = new WaveFormat(44100, 1) };
                waveIn.DataAvailable += OnDataAvailable;

                bufferedWaveProvider = new BufferedWaveProvider(waveIn.WaveFormat)
                {
                    BufferLength = waveIn.WaveFormat.AverageBytesPerSecond * 5, // 5 seconds buffer
                    DiscardOnBufferOverflow = true
                };
                UpdateEffect();

                waveOut = new WaveOutEvent { DeviceNumber = outputDeviceIndex };
                waveOut.Init(finalProvider);

                waveIn.StartRecording();
                waveOut.Play();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error starting audio: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Stop();
            }
        }

        public void Stop()
        {
            waveIn?.StopRecording();
            waveIn?.Dispose();
            waveIn = null;

            waveOut?.Stop();
            waveOut?.Dispose();
            waveOut = null;

            bufferedWaveProvider = null;
        }

        public void UpdateEffect()
        {
            if (bufferedWaveProvider == null) return;

            var sampleProvider = bufferedWaveProvider.ToSampleProvider();

            var boostProvider = new VolumeSampleProvider(sampleProvider) { Volume = MicBoost };
            var aiBoostProvider = new VolumeSampleProvider(boostProvider) { Volume = AiBoost };

            var effectProvider = selectedVoiceEffect?.CreateEffect(aiBoostProvider) ?? aiBoostProvider;

            var volumeProvider = new VolumeSampleProvider(effectProvider) { Volume = Gain };

            finalProvider = volumeProvider;

            if (waveOut?.PlaybackState == PlaybackState.Playing)
            {
                waveOut.Stop();
                waveOut.Init(finalProvider);
                waveOut.Play();
            }
        }

        private void OnDataAvailable(object? sender, WaveInEventArgs e)
        {
            if (bufferedWaveProvider != null)
            {
                bufferedWaveProvider.AddSamples(e.Buffer, 0, e.BytesRecorded);
            }
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            VoiceEffectComboBox.ItemsSource = voiceEffects.Select(v => v.Name).ToList();
            VoiceEffectComboBox.SelectedIndex = 0;
            VoiceEffectComboBox.SelectionChanged += (s, ev) => {
                var name = VoiceEffectComboBox.SelectedItem as string;
                selectedVoiceEffect = voiceEffects.FirstOrDefault(v => v.Name == name) ?? voiceEffects.First();
                
                bool wasHearingMyself = isHearingMyself;
                bool wasEffectApplied = isEffectApplied;
                
                if (wasHearingMyself || wasEffectApplied)
                {
                    Stop();
                    // we don't need to call UpdateEffect here because Start will call it
                    if (wasHearingMyself)
                    {
                        Start(0, 0); 
                    }
                    else // wasEffectApplied
                    {
                        int vbCableIndex = GetVBCableOutputDeviceIndex();
                        if (vbCableIndex != -1)
                        {
                            Start(0, vbCableIndex);
                        }
                    }
                }
            };

            GainSlider.ValueChanged += (s, ev) => { Gain = (float)GainSlider.Value; UpdateEffect(); };
            MicBoostSlider.ValueChanged += (s, ev) => { MicBoost = (float)MicBoostSlider.Value; UpdateEffect(); };
            PitchSlider.ValueChanged += (s, ev) => { Pitch = (float)PitchSlider.Value; UpdateEffect(); };
            AiBoostSlider.ValueChanged += (s, ev) => { AiBoost = (float)AiBoostSlider.Value; UpdateEffect(); };
            EchoDelaySlider.ValueChanged += (s, ev) => { EchoDelay = (int)EchoDelaySlider.Value; UpdateEffect(); };
            EchoFeedbackSlider.ValueChanged += (s, ev) => { EchoFeedback = (float)EchoFeedbackSlider.Value; UpdateEffect(); };
        }

        private void HearMyselfButton_Click(object sender, RoutedEventArgs e)
        {
            if (isHearingMyself)
            {
                Stop();
                isHearingMyself = false;
                StatusLabel.Content = "Inactive";
                StatusLabel.Foreground = Brushes.Red;
                return;
            }

            Stop();
            isHearingMyself = true;
            isEffectApplied = false;

            int inputDevice = 0; // default input device
            int outputDevice = 0; // default output device
            Start(inputDevice, outputDevice);
            StatusLabel.Content = "Hearing Myself";
            StatusLabel.Foreground = Brushes.Green;
        }

        private void ApplyEffectButton_Click(object sender, RoutedEventArgs e)
        {
            int vbCableIndex = GetVBCableOutputDeviceIndex();
            if (vbCableIndex == -1)
            {
                MessageBox.Show("VB-Cable is not installed. Please install it to apply effects to other apps.", "VB-Cable Not Found", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (isEffectApplied)
            {
                Stop();
                isEffectApplied = false;
                StatusLabel.Content = "Inactive";
                StatusLabel.Foreground = Brushes.Red;
                return;
            }

            Stop();
            isHearingMyself = false;
            isEffectApplied = true;

            int inputDevice = 0; // default input device
            Start(inputDevice, vbCableIndex);
            StatusLabel.Content = "Applied to Apps";
            StatusLabel.Foreground = Brushes.Green;
        }

        private int GetVBCableOutputDeviceIndex()
        {
            for (int i = 0; i < WaveOut.DeviceCount; i++)
            {
                if (WaveOut.GetCapabilities(i).ProductName.Contains("CABLE Input"))
                {
                    return i;
                }
            }
            return -1;
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            Stop();
            waveIn?.Dispose();
            waveIn = null;

            waveOut?.Stop();
            waveOut?.Dispose();
            waveOut = null;

            bufferedWaveProvider = null;
        }
        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // Add any cleanup logic if needed
        }

        private void TitleBar_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.ChangedButton == System.Windows.Input.MouseButton.Left)
                this.DragMove();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void ThemeToggle_Click(object sender, RoutedEventArgs e)
        {
            // Theme toggle logic placeholder
        }
    }
}
