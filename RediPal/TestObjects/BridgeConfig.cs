using RedipalCore.Attributes;
using System;
using System.Collections.Generic;

namespace RedipalCore.TestObjects
{
    [RediKeySpace("config")]
    public class BridgeData
    {
        public Dictionary<string, BridgeSettings>? Settings { get; set; }

        public Dictionary<string, BridgeZone>? Zones { get; set; }
    }

    public class BridgeSettings
    {
        public BridgeSettings()
        {
            BridgeZone = new BridgeZone();
            DisabledBridgeZone = new BridgeZone();
            LocationDataID = "";
            AllowedZones = new List<string>();
        }

        public string? ID { get; set; }
        public bool EnableBidge { get; set; }
        public bool LimitToZones { get; set; }
        public bool StopAllMotion_OnDisable { get; set; }
        public string LocationDataID { get; set; }
        public int BridgeHomeY { get; set; }
        public int BridgeHomeX { get; set; }
        public BridgeZone BridgeZone { get; set; }
        public BridgeZone DisabledBridgeZone { get; set; }

        [RediReName("WorkZones")]
        public Dictionary<string, WorkZone>? WorkZone { get; set; }

        public List<string> AllowedZones { get; set; }
    }

    [RediWriteAsJson]
    public class WorkZone
    {
        public string? ID { get; set; }
        public int Priority { get; set; }
    }

    [RediWriteAsJson]
    public class BridgeZone
    {
        public int MinZonePox { get; set; }
        public int MaxZonePox { get; set; }
    }

    public class AllowableZone
    {
        public AllowableZone(string ID, BridgeZone bridgeZone)
        {
            ZoneCoordinates = bridgeZone;
            ZoneID = ID;
        }

        public string ZoneID { get; set; }
        public BridgeZone ZoneCoordinates { get; }
        public bool ZoneReseved { get; set; }
        public string? ReserverName { get; set; }
        public DateTime ReserveDataTime { get; }
    }
}
