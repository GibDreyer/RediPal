using RedipalCore.Attributes;
using System;

namespace RedipalCore.TestObjects
{
    [RediKeySpace("dolly")]
    [RediDefaultSet("dollies")]
    public class Dolly : RediBase
    {
        public string DeviceId { get; set; } = "";

        public int Index { get; set; }

        public bool Disabled { get; set; }

        public double MainBatteryVoltage { get; set; }
        public double LogicBatteryVoltage { get; set; }
        public double Motor1PWM { get; set; }
        public double Motor2PWM { get; set; }
        public double Motor1Current { get; set; }
        public double Motor2Current { get; set; }
        public double PCTemp { get; set; }
        public double MCPTemp { get; set; }
        public int Status { get; set; }
        public bool InMotion { get; set; }
        public int Speed { get; set; }
        public bool Fwd { get; set; }
        public bool Rev { get; set; }
        public int Ultrasonic { get; set; }

        public DateTimeOffset Time { get; internal set; } = DateTime.MinValue;
        public DateTime LastHeartBeat { get; set; } = DateTime.MinValue;
    }
}
