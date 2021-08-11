using RedipalCore;
using RedipalCore.Attributes;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace RediPal.TestObjects
{
    [RediExpire(seconds: 5)]
    [RediKeySpace("statusmessage")]
    public class StatusMessage : RediBase
    {
        [RediIgnore]
        public string ImagePath { get; set; } = "";

        public string Status { get; set; } = string.Empty;
        public StatusMessageType Type { get; set; }

        [RediWriteAsImage(Redi_ImageFormat.Jpeg, 25)]
        public Bitmap? Image { get; set; }
    }

    public enum StatusMessageType
    {
        Info,
        Warning,
        Error
    }
}
