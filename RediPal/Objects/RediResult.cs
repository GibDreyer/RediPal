namespace RedipalCore.Objects
{
    public class RediResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = "";
    }

    public class RediResults
    {
        public bool Success { get; set; }
        public string[] Message { get; set; } = System.Array.Empty<string>();
    }
}