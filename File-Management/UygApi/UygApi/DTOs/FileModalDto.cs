namespace UygApi.DTOs
{
    public class FileModalDto : BaseDto
    {
        public string FileName { get; set; }

        public string FileType { get; set; }

        public string Extension { get; set; }

        public string Description { get; set; }

        public string FilePath { get; set; }

        public string UploadedBy { get; set; }

        public DateTime UploadOn { get; set; }

        public int CategoryId { get; set; }
        public CategoryDto? Category { get; set; }
        public string? UserId { get; set; }
    }
}
