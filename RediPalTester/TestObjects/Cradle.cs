using RedipalCore.Attributes;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Timers;

namespace RedipalCore.TestObjects
{
    [RediDefaultSet("cradles")]
    [RediKeySpace("cradle")]
    public class Cradle
    {
        public Cradle()
        {
            CradleClass = CradleClass.Unknown;
            PrioityLevel = CradlePrioityLevel.Undefined;
            DateAdded = DateTime.Now;
            LastUpdated = DateTime.Now;
            ID = "";
        }

        public string ID { get; set; }
        public double Priority { get; set; }
        public string? EstRetrievalLocation { get; set; }
        public DateTime EstRetrievalDate { get; set; }
        public double Weight { get; set; }
        public CradleClass CradleClass { get; set; }
        public CradlePrioityLevel PrioityLevel { get; set; }
        public CradleState State { get; set; }
        public DateTime DateAdded { get; }
        public DateTime LastAccessed { get; set; }
        public DateTime LastUpdated { get; set; }
        public bool IsEmpty => State == CradleState.Empty;
        public bool LastPlaced180 { get; set; }
        public bool NewInSystem { get; set; }

        public List<string> Tags { get; set; } = new List<string>();
        public List<string> Jobs { get; set; } = new List<string>();

        private bool locked;

        private bool lastPlaced180;
    }

    public enum CradleState
    {
        UnSet,
        Empty,
        Scrap,
        PendingScrap,
        Scheduled,
        ProductionScheduled
    }

    public enum CradleClass
    {
        Unknown,
        Empty,
        Light,
        Medium,
        Heavy
    }

    public enum CradlePrioityLevel
    {
        Level_1 = 1,
        Level_2,
        Level_3,
        Level_4,
        Undefined
    }
}
