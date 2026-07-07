namespace HEVEQ.Application.Common.Options
{
    public class CloudinarySettings
    {
        public string CloudName { get; set; } = string.Empty;
        public string ApiKey { get; set; } = string.Empty;
        public string ApiSecret { get; set; } = string.Empty;
        public int MaxImageSizeInMb { get; set; } = 10;
    }
}