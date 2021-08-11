using RediPal.Messages;
using RedipalCore.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RediPalTester
{
    public class Task_Store_Message : RediMessageBase
    {
        public Task_Store_Message() : base("task-store") { }

        [RediMessageIndex(0)]
        public string CradleID { get; set; }
        [RediMessageIndex(1)]
        public string OperatorID { get; set; }
    }




    public class Task_Rotate_Message : RediMessageBase
    {
        public Task_Rotate_Message() : base("task-rotate") { }

        [RediMessageIndex(0)]
        public string OperatorID { get; set; }
    }
}
