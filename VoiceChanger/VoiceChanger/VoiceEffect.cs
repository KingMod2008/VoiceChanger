using System;
using NAudio.Wave;

namespace KingVoiceChanger
{
    public class VoiceEffect
    {
        public string Name { get; }
        public string Description { get; }
        public string IconPath { get; }
        public Func<ISampleProvider, ISampleProvider> CreateEffect { get; }

        public VoiceEffect(string name, string description, string iconPath, Func<ISampleProvider, ISampleProvider> createEffect)
        {
            Name = name;
            Description = description;
            IconPath = iconPath;
            CreateEffect = createEffect;
        }
    }
}