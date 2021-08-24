using RedipalCore;
using RedipalCore.Attributes;
using System;

namespace AG.ROC.Core
{
    [RediKeySpace("status-messages:disarm-state")]
    [RediDefaultSet("status-messages:disarm-states")]
    public class DisarmState : RediBase
    {  
        public bool Allow { get; set; }
        public string Message { get; set; } = "";
        public DateTime LastUpdate { get; set; }
    }
}
