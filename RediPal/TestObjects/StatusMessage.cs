using RedipalCore.Attributes;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace RediPal.TestObjects
{
    [RediExpire(seconds: 45)]
    [RediKeySpace("statusmessage")]
    public class StatusMessage
    {
        public StatusMessage() { }
        public StatusMessage(Bitmap bitmap)
        {
            MemoryStream ms = new();
            bitmap.Save(ms, ImageFormat.Jpeg);
            Image = Convert.ToBase64String(ms.ToArray());
        }

        public string Status { get; set; } = string.Empty;
        public StatusMessageType Type { get; set; }
        public string? Image { get; set; }
    }

    public enum StatusMessageType
    {
        Info,
        Warning,
        Error
    }
}
