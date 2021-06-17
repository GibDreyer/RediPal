﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedipalCore.Objects
{
    public class RediConfig
    {
        /// <summary>
        /// If true it allows RediPal to use up to 100% of the available CPU. false is default
        /// </summary>
        public bool UnThrottleCPU { get; set; } = false;
        public bool IgnoreReadOnlyProperties { get; set; } = false;
        public bool Default_ToLowerCase { get; set; } = true;
    }
}
