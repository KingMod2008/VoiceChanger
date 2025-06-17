using System;
using System.Collections.Generic;
using System.Linq;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using NAudio.Dsp;

namespace KingVoiceChanger
{
    // --- NEW EFFECTS ONLY ---

    // 1. Pitch Shift (uses NAudio's SmbPitchShiftingSampleProvider)
    // (No custom class needed, will use SmbPitchShiftingSampleProvider directly)



    // 2. Echo
    public class EchoEffect : ISampleProvider
    {
        private readonly ISampleProvider source;
        private readonly float[] delayBuffer;
        private int writePos;
        private readonly int delaySamples;
        private readonly float feedback;
        public WaveFormat WaveFormat => source.WaveFormat;
        public EchoEffect(ISampleProvider source, int delayMs = 300, float feedback = 0.4f)
        {
            this.source = source;
            this.feedback = feedback;
            this.delaySamples = (int)(WaveFormat.SampleRate * (delayMs / 1000.0));
            delayBuffer = new float[delaySamples * WaveFormat.Channels];
            writePos = 0;
        }
        public int Read(float[] buffer, int offset, int count)
        {
            int read = source.Read(buffer, offset, count);
            for (int n = 0; n < read; n++)
            {
                int idx = (writePos + n) % delayBuffer.Length;
                float delayed = delayBuffer[idx];
                buffer[offset + n] += delayed * feedback;
                delayBuffer[idx] = buffer[offset + n];
            }
            writePos = (writePos + read) % delayBuffer.Length;
            return read;
        }
    }

    // 3. Reverb (simple Schroeder reverb)
    public class ReverbEffect : ISampleProvider
    {
        private readonly ISampleProvider source;
        private readonly float[] combBuffer;
        private readonly float[] allPassBuffer;
        private int combPos;
        private int allPassPos;
        public WaveFormat WaveFormat => source.WaveFormat;
        public ReverbEffect(ISampleProvider source)
        {
            this.source = source;
            int combSamples = (int)(0.1f * WaveFormat.SampleRate); // 100 ms
            combBuffer = new float[combSamples];
            int allPassSamples = (int)(0.03f * WaveFormat.SampleRate); // 30 ms
            allPassBuffer = new float[allPassSamples];
        }
        public int Read(float[] buffer, int offset, int count)
        {
            int read = source.Read(buffer, offset, count);
            for (int i = 0; i < read; i++)
            {
                // Comb section
                float combOut = combBuffer[combPos];
                combBuffer[combPos] = buffer[offset + i] + combOut * 0.5f;
                combPos = (combPos + 1) % combBuffer.Length;
                // All-pass section
                float buf = allPassBuffer[allPassPos];
                float apOut = -combOut + buf;
                allPassBuffer[allPassPos] = combOut + buf * 0.5f;
                allPassPos = (allPassPos + 1) % allPassBuffer.Length;
                buffer[offset + i] = apOut;
            }
            return read;
        }
    }

    // 4. Underwater (low-pass filter + optional ambience)
    public class UnderwaterEffect : ISampleProvider
    {
        private readonly ISampleProvider voiceSource;
        private readonly BiQuadFilter lowPassFilter;
        private readonly ISampleProvider ambienceSource;
        private readonly MixingSampleProvider mixer;
        private readonly int sampleRate;
        public WaveFormat WaveFormat => voiceSource.WaveFormat;
        public UnderwaterEffect(ISampleProvider source)
        {
            this.voiceSource = source;
            this.sampleRate = source.WaveFormat.SampleRate;
            lowPassFilter = BiQuadFilter.LowPassFilter(sampleRate, 400, 2);
            ambienceSource = TryLoadLoopedAmbience(sampleRate) ?? new SilenceProvider(source.WaveFormat).ToSampleProvider();
            mixer = new MixingSampleProvider(new[] { new LowPassSampleProvider(voiceSource, lowPassFilter), ambienceSource });
            mixer.ReadFully = true;
        }
        public int Read(float[] buffer, int offset, int count)
        {
            return mixer.Read(buffer, offset, count);
        }
        private ISampleProvider? TryLoadLoopedAmbience(int sampleRate)
        {
            string path = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "underwater_ambience.wav");
            if (!System.IO.File.Exists(path))
                return null;
            try
            {
                var reader = new NAudio.Wave.AudioFileReader(path);
                var loop = new LoopStream(reader);
                loop.EnableLooping = true;
                return loop.ToSampleProvider();
            }
            catch { return null; }
        }
        private class LowPassSampleProvider : ISampleProvider
        {
            private readonly ISampleProvider src;
            private readonly BiQuadFilter filter;
            public WaveFormat WaveFormat => src.WaveFormat;
            public LowPassSampleProvider(ISampleProvider src, BiQuadFilter filter)
            {
                this.src = src;
                this.filter = filter;
            }
            public int Read(float[] buffer, int offset, int count)
            {
                int read = src.Read(buffer, offset, count);
                for (int i = 0; i < read; i++)
                    buffer[offset + i] = filter.Transform(buffer[offset + i]);
                return read;
            }
        }
    }

// Only one LoopStream
    // Only one LoopStream
public class LoopStream : WaveStream
    {
        private readonly WaveStream sourceStream;
        public bool EnableLooping { get; set; } = true;
        public override WaveFormat WaveFormat => sourceStream.WaveFormat;
        public override long Length => sourceStream.Length;
        public override long Position
        {
            get => sourceStream.Position;
            set => sourceStream.Position = value;
        }
        public LoopStream(WaveStream sourceStream)
        {
            this.sourceStream = sourceStream;
        }
        public override int Read(byte[] buffer, int offset, int count)
        {
            int totalBytesRead = 0;
            while (totalBytesRead < count)
            {
                int bytesRead = sourceStream.Read(buffer, offset + totalBytesRead, count - totalBytesRead);
                if (bytesRead == 0)
                {
                    if (sourceStream.Position == 0 || !EnableLooping)
                        break;
                    // Loop
                    sourceStream.Position = 0;
                }
                totalBytesRead += bytesRead;
            }
            return totalBytesRead;
        }
    }


    // 5. Robot (simple sign modulation)
    public class RobotEffect : ISampleProvider
    {
        private readonly ISampleProvider source;
        private float phase;
        private const float CarrierFreq = 30f; // Hz
        public WaveFormat WaveFormat => source.WaveFormat;
        public RobotEffect(ISampleProvider source)
        {
            this.source = source;
        }
        public int Read(float[] buffer, int offset, int count)
        {
            int read = source.Read(buffer, offset, count);
            for (int n = 0; n < read; n++)
            {
                float mod = (float)Math.Sign(Math.Sin(2 * Math.PI * CarrierFreq * phase / WaveFormat.SampleRate));
                buffer[offset + n] *= mod;
                phase++;
            }
            return read;
        }
    }


    // --- Additional Robust Effects ---

    // Distortion
    public class DistortionEffect : ISampleProvider
    {
        private readonly ISampleProvider source;
        public float Gain { get; set; } = 10f;
        public WaveFormat WaveFormat => source.WaveFormat;
        public DistortionEffect(ISampleProvider source) { this.source = source; }
        public int Read(float[] buffer, int offset, int count)
        {
            int read = source.Read(buffer, offset, count);
            for (int i = 0; i < read; i++)
            {
                float s = buffer[offset + i] * Gain;
                // Simple hard clipping
                if (s > 1.0f) s = 1.0f;
                if (s < -1.0f) s = -1.0f;
                buffer[offset + i] = s;
            }
            return read;
        }
    }





    // Vibrato
    public class VibratoEffect : ISampleProvider
    {
        private readonly ISampleProvider source;
        private readonly float[] delayBuffer;
        private int delayPosition;
        private readonly int sampleRate;
        private float lfoPhase;
        private const float LfoFrequency = 5.0f;
        private const float Depth = 0.0015f;
        public WaveFormat WaveFormat => source.WaveFormat;
        public VibratoEffect(ISampleProvider source)
        {
            this.source = source;
            this.sampleRate = source.WaveFormat.SampleRate;
            int maxDelaySamples = (int)(sampleRate * Depth * 2);
            delayBuffer = new float[maxDelaySamples];
        }
        public int Read(float[] buffer, int offset, int count)
        {
            int read = source.Read(buffer, offset, count);
            for (int i = 0; i < read; i++)
            {
                lfoPhase += 2 * (float)Math.PI * LfoFrequency / sampleRate;
                if (lfoPhase > 2 * (float)Math.PI) lfoPhase -= 2 * (float)Math.PI;
                float lfo = (float)Math.Sin(lfoPhase);
                float delay = (Depth * (1 + lfo)) * sampleRate;
                int idx = (delayPosition - (int)delay + delayBuffer.Length) % delayBuffer.Length;
                float delayed = delayBuffer[idx];
                delayBuffer[delayPosition] = buffer[offset + i];
                buffer[offset + i] = delayed;
                delayPosition = (delayPosition + 1) % delayBuffer.Length;
            }
            return read;
        }
    }

    // Monster (pitch shift + distortion)
    public class MonsterEffect : ISampleProvider
    {
        private readonly ISampleProvider effectChain;
        public WaveFormat WaveFormat => effectChain.WaveFormat;
        public MonsterEffect(ISampleProvider source)
        {
            var pitch = new SmbPitchShiftingSampleProvider(source) { PitchFactor = 0.6f };
            var distortion = new DistortionEffect(pitch) { Gain = 15 };
            effectChain = distortion;
        }
        public int Read(float[] buffer, int offset, int count)
        {
            return effectChain.Read(buffer, offset, count);
        }
    }

    // Whisper
    public class WhisperEffect : ISampleProvider
    {
        private readonly ISampleProvider source;
        private readonly Random rnd = new Random();
        public WaveFormat WaveFormat => source.WaveFormat;
        public WhisperEffect(ISampleProvider source) { this.source = source; }
        public int Read(float[] buffer, int offset, int count)
        {
            int read = source.Read(buffer, offset, count);
            for (int i = 0; i < read; i++)
            {
                float noise = (float)(rnd.NextDouble() * 2 - 1) * 0.02f;
                buffer[offset + i] = buffer[offset + i] * 0.3f + noise;
            }
            return read;
        }
    }

    // Telephone
    public class TelephoneEffect : ISampleProvider
    {
        private readonly ISampleProvider source;
        private readonly BiQuadFilter bandPass;
        public WaveFormat WaveFormat => source.WaveFormat;
        public TelephoneEffect(ISampleProvider source)
        {
            this.source = source;
            bandPass = BiQuadFilter.BandPassFilterConstantSkirtGain(source.WaveFormat.SampleRate, 1200f, 0.707f);
        }
        public int Read(float[] buffer, int offset, int count)
        {
            int read = source.Read(buffer, offset, count);
            for (int i = 0; i < read; i++)
                buffer[offset + i] = bandPass.Transform(buffer[offset + i]);
            return read;
        }
    }

    // Bass Boost
    public class BassBoostEffect : ISampleProvider
    {
        private readonly ISampleProvider source;
        private readonly BiQuadFilter filter;
        public WaveFormat WaveFormat => source.WaveFormat;
        public BassBoostEffect(ISampleProvider source)
        {
            this.source = source;
            filter = BiQuadFilter.LowShelf(source.WaveFormat.SampleRate, 250, 1, 9);
        }
        public int Read(float[] buffer, int offset, int count)
        {
            int read = source.Read(buffer, offset, count);
            for (int i = 0; i < read; i++)
                buffer[offset + i] = filter.Transform(buffer[offset + i]);
            return read;
        }
    }

    // --- 10 More Unique Effects ---

    // Alien (high pitch + vibrato)
    public class AlienEffect : ISampleProvider
    {
        private readonly ISampleProvider effectChain;
        public WaveFormat WaveFormat => effectChain.WaveFormat;
        public AlienEffect(ISampleProvider source)
        {
            var pitch = new SmbPitchShiftingSampleProvider(source) { PitchFactor = 2.2f };
            effectChain = new VibratoEffect(pitch);
        }
        public int Read(float[] buffer, int offset, int count) => effectChain.Read(buffer, offset, count);
    }

    // Bender (robotic + pitch drop)
    public class BenderEffect : ISampleProvider
    {
        private readonly ISampleProvider effectChain;
        public WaveFormat WaveFormat => effectChain.WaveFormat;
        public BenderEffect(ISampleProvider source)
        {
            var pitch = new SmbPitchShiftingSampleProvider(source) { PitchFactor = 0.7f };
            effectChain = new RobotEffect(pitch);
        }
        public int Read(float[] buffer, int offset, int count) => effectChain.Read(buffer, offset, count);
    }

    // Tremolo (amplitude modulation)
    public class TremoloEffect : ISampleProvider
    {
        private readonly ISampleProvider source;
        private float phase;
        private const float TremoloFreq = 8f;
        public WaveFormat WaveFormat => source.WaveFormat;
        public TremoloEffect(ISampleProvider source) { this.source = source; }
        public int Read(float[] buffer, int offset, int count)
        {
            int read = source.Read(buffer, offset, count);
            for (int i = 0; i < read; i++)
            {
                float mod = (float)((Math.Sin(2 * Math.PI * TremoloFreq * phase / WaveFormat.SampleRate) + 1) / 2);
                buffer[offset + i] *= mod;
                phase++;
            }
            return read;
        }
    }

    // Bitcrusher (reduces bit depth)
    public class BitcrusherEffect : ISampleProvider
    {
        private readonly ISampleProvider source;
        public WaveFormat WaveFormat => source.WaveFormat;
        public BitcrusherEffect(ISampleProvider source) { this.source = source; }
        public int Read(float[] buffer, int offset, int count)
        {
            int read = source.Read(buffer, offset, count);
            for (int i = 0; i < read; i++)
            {
                buffer[offset + i] = (float)Math.Round(buffer[offset + i] * 8f) / 8f;
            }
            return read;
        }
    }

    // Telephone2 (bandpass + distortion)
    public class Telephone2Effect : ISampleProvider
    {
        private readonly ISampleProvider effectChain;
        public WaveFormat WaveFormat => effectChain.WaveFormat;
        public Telephone2Effect(ISampleProvider source)
        {
            var band = new TelephoneEffect(source);
            effectChain = new DistortionEffect(band);
        }
        public int Read(float[] buffer, int offset, int count) => effectChain.Read(buffer, offset, count);
    }

    // Phaser (simple phase effect)
    public class PhaserEffect : ISampleProvider
    {
        private readonly ISampleProvider source;
        private float phase;
        private const float PhaserFreq = 0.7f;
        public WaveFormat WaveFormat => source.WaveFormat;
        public PhaserEffect(ISampleProvider source) { this.source = source; }
        public int Read(float[] buffer, int offset, int count)
        {
            int read = source.Read(buffer, offset, count);
            for (int i = 0; i < read; i++)
            {
                float mod = (float)Math.Sin(2 * Math.PI * PhaserFreq * phase / WaveFormat.SampleRate);
                buffer[offset + i] *= (0.7f + 0.3f * mod);
                phase++;
            }
            return read;
        }
    }

    // Octaver (down one octave)
    public class OctaverEffect : ISampleProvider
    {
        private readonly ISampleProvider source;
        private int toggle = 0;
        public WaveFormat WaveFormat => source.WaveFormat;
        public OctaverEffect(ISampleProvider source) { this.source = source; }
        public int Read(float[] buffer, int offset, int count)
        {
            int read = source.Read(buffer, offset, count);
            for (int i = 0; i < read; i++)
            {
                if ((toggle++ & 1) == 0) buffer[offset + i] = 0;
            }
            return read;
        }
    }

    // Stutter (chops audio in blocks)
    public class StutterEffect : ISampleProvider
    {
        private readonly ISampleProvider source;
        private int sampleCount = 0;
        private bool mute = false;
        public WaveFormat WaveFormat => source.WaveFormat;
        public StutterEffect(ISampleProvider source) { this.source = source; }
        public int Read(float[] buffer, int offset, int count)
        {
            int read = source.Read(buffer, offset, count);
            for (int i = 0; i < read; i++)
            {
                if ((sampleCount++ / 2000) % 2 == 0) mute = !mute;
                if (mute) buffer[offset + i] = 0;
            }
            return read;
        }
    }

    // Muffled (lowpass only)
    public class MuffledEffect : ISampleProvider
    {
        private readonly ISampleProvider source;
        private readonly BiQuadFilter filter;
        public WaveFormat WaveFormat => source.WaveFormat;
        public MuffledEffect(ISampleProvider source)
        {
            this.source = source;
            filter = BiQuadFilter.LowPassFilter(source.WaveFormat.SampleRate, 600, 1f);
        }
        public int Read(float[] buffer, int offset, int count)
        {
            int read = source.Read(buffer, offset, count);
            for (int i = 0; i < read; i++)
                buffer[offset + i] = filter.Transform(buffer[offset + i]);
            return read;
        }
    }

    // Highpass (removes bass)
    public class HighpassEffect : ISampleProvider
    {
        private readonly ISampleProvider source;
        private readonly BiQuadFilter filter;
        public WaveFormat WaveFormat => source.WaveFormat;
        public HighpassEffect(ISampleProvider source)
        {
            this.source = source;
            filter = BiQuadFilter.HighPassFilter(source.WaveFormat.SampleRate, 2000, 1f);
        }
        public int Read(float[] buffer, int offset, int count)
        {
            int read = source.Read(buffer, offset, count);
            for (int i = 0; i < read; i++)
                buffer[offset + i] = filter.Transform(buffer[offset + i]);
            return read;
        }
    }

    // Radio (bandpass + static)
    public class RadioEffect : ISampleProvider
    {
        private readonly ISampleProvider source;
        private readonly BiQuadFilter bandPass;
        private readonly Random random = new Random();
        private const float StaticAmount = 0.05f;
        public WaveFormat WaveFormat => source.WaveFormat;

        public RadioEffect(ISampleProvider source)
        {
            this.source = source;
            bandPass = BiQuadFilter.BandPassFilterConstantPeakGain(source.WaveFormat.SampleRate, 1800, 1.2f);
        }

        public int Read(float[] buffer, int offset, int count)
        {
            int read = source.Read(buffer, offset, count);
            for (int i = 0; i < read; i++)
            {
                float filtered = bandPass.Transform(buffer[offset + i]);
                float staticNoise = (float)(random.NextDouble() * 2 - 1) * StaticAmount;
                buffer[offset + i] = filtered + staticNoise;
            }
            return read;
        }
    }

}
