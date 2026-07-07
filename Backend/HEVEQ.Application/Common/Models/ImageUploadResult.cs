namespace HEVEQ.Application.Common.Models
{
    public class ImageUploadResult
    {
        public string Url { get; set; } = string.Empty;
        public string PublicId { get; set; } = string.Empty;
        public string Folder { get; set; } = string.Empty;
        public string Format { get; set; } = string.Empty;
        public int Width { get; set; }
        public int Height { get; set; }
        public long Bytes { get; set; }
        public bool IsPublic { get; set; } = true;
    }
}