using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KingVoiceChanger
{
    public class AppSettings
    {
        public string? InputDeviceId { get; set; }
        public string? OutputDeviceId { get; set; }
        public string? VoiceEffectName { get; set; }
        public string? Theme { get; set; }
        public float Gain { get; set; } = 1.0f;
    }
}
