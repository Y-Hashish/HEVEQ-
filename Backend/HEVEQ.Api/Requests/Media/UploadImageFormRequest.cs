namespace HEVEQ.Api.Requests.Media
{
    public class UploadImageFormRequest
    {
        public IFormFile File { get; set; } = null!;
        public string? Purpose { get; set; }
        public Guid? ReferenceId { get; set; }
    }
}
