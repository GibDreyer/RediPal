using RedipalCore.Attributes;
using RedipalCore.Interfaces;

namespace RedipalCore.TestObjects
{
    [RediKeySpace("operator", "config")]
    [RediDefaultSet("operators")]
    public class OperatorConfig : RediBase
    {
        public bool AutoReplaceWithEmpty { get; set; }
        public int EmptyDollyTimer { get; set; }
        public int EmptyPriority { get; set; }
        public bool AutoStore { get; set; }
        public bool AutoStoreIfEmpty { get; set; }
        public bool Disable { get; set; }
    }
}
