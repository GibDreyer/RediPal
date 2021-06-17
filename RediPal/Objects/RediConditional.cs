using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace RedipalCore.Objects
{
    public class RediConditional
    {
        public Delegate? Condition { get; set; }
        public Delegate[]? Actions { get; set; }
    }

    public class RediParameterModifier
    {
        public Delegate[]? Actions { get; set; }
    }
}
