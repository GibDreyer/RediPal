using RedipalCore;
using RedipalCore.Attributes;
using System;

namespace RedipalCore.TestObjects
{
    [RediKeySpace("roccore:log")]
    public class ServiceLog : RediBase
    {
        public ServiceLog()
        {
        }

        public ServiceLog(string issuer)
        {
            Issuer = issuer;
            DateTime = DateTime.Now.AddSeconds(new Random().Next(-50000, 0)).ToLocalTime();
            var id = Redi_GetObjectID($"roccore:log:{issuer}id");
            WriteID = Redi_GetObjectID($"roccore:log:id") ?? long.MaxValue;

            if (id.HasValue)
            {
                ID = id.Value;
            }
            else
            {
                ID = DateTime.Now.Ticks;
            }
        }

        public long ID { get; set; }
        [RediIgnore]
        public long WriteID { get; set; }
        public string? Message { get; set; }
        public int RetunCode { get; set; }
        public DateTime DateTime { get; set; }
        public LogType LogType { get; set; }

        public string? Issuer;
    }

    public enum LogType
    {
        Error,
        Information,
        System,
        Motion,
        MotionResult,
        MotionInformation,
        Warning,
        NewTask,
        TaskComplete,
        Taskinformation
    }
}
